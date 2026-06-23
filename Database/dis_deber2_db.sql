USE [master]
GO

IF DB_ID(N'dis_deber2_db') IS NOT NULL
BEGIN
    ALTER DATABASE [dis_deber2_db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [dis_deber2_db];
END
GO

CREATE DATABASE [dis_deber2_db]
GO

USE [dis_deber2_db]
GO

-- Encriptar / desencriptar contraseña
CREATE FUNCTION [dbo].[encritptaCon](@clave VARCHAR(50))
RETURNS VARBINARY(MAX)
AS
BEGIN
    RETURN ENCRYPTBYPASSPHRASE('cl@ve', @clave);
END;
GO

CREATE FUNCTION [dbo].[desencriptaCon](@clave VARBINARY(MAX))
RETURNS VARCHAR(50)
AS
BEGIN
    RETURN CONVERT(VARCHAR(50), DECRYPTBYPASSPHRASE('cl@ve', @clave));
END;
GO

CREATE TABLE [dbo].[tbl_tipo_usuario](
    [tusu_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [tusu_nombre] VARCHAR(50) NULL,
    [tusu_estado] CHAR(1) NULL DEFAULT 'A'
);
GO

CREATE TABLE [dbo].[tbl_usuario](
    [usu_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [usu_cedula] VARCHAR(10) NULL,
    [usu_nombres] VARCHAR(50) NULL,
    [usu_apellidos] VARCHAR(50) NULL,
    [usu_direccion] VARCHAR(50) NULL,
    [usu_celular] VARCHAR(10) NULL,
    [usu_correo] VARCHAR(150) NOT NULL,
    [usu_fecha_creacion] DATETIME NULL DEFAULT GETDATE(),
    [usu_fecha_cumple] DATE NULL,
    [usu_nick] VARCHAR(50) NULL,
    [usu_contraseña] VARBINARY(MAX) NULL,
    [usu_intentos] INT NULL DEFAULT 0,
    [usu_codigo_OTP] VARCHAR(10) NULL,
    [usu_otp_expira] DATETIME NULL,
    [usu_otp_tipo] VARCHAR(20) NULL, -- LOGIN | RECUPERACION
    [usu_estado] CHAR(1) NULL DEFAULT 'A',
    [usu_fecha_baja] DATETIME NULL,
    [tusu_id] INT NULL,
    CONSTRAINT UQ_usuario_correo UNIQUE ([usu_correo]),
    CONSTRAINT FK_usuario_tipo FOREIGN KEY ([tusu_id]) REFERENCES [dbo].[tbl_tipo_usuario]([tusu_id])
);
GO

CREATE INDEX IX_usuario_cedula_correo ON [dbo].[tbl_usuario]([usu_cedula], [usu_correo]);
GO

CREATE TABLE [dbo].[tbl_proveedor](
    [prov_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [prov_nombre] VARCHAR(100) NOT NULL,
    [prov_ruc] VARCHAR(13) NULL,
    [prov_telefono] VARCHAR(15) NULL,
    [prov_correo] VARCHAR(150) NULL,
    [prov_direccion] VARCHAR(150) NULL,
    [prov_estado] CHAR(1) NULL DEFAULT 'A',
    [prov_fecha_baja] DATETIME NULL
);
GO

CREATE TABLE [dbo].[tbl_categoria](
    [cat_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [cat_nombre] VARCHAR(80) NOT NULL,
    [cat_estado] CHAR(1) NULL DEFAULT 'A',
    [cat_fecha_baja] DATETIME NULL
);
GO

CREATE TABLE [dbo].[tbl_producto](
    [pro_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [pro_nombre] VARCHAR(500) NOT NULL,
    [pro_descripcion] VARCHAR(500) NULL,
    [pro_precio] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [pro_stock] INT NOT NULL DEFAULT 0,
    [pro_imagen_ruta] VARCHAR(255) NULL,
    [pro_estado] CHAR(1) NULL DEFAULT 'A',
    [pro_fecha_baja] DATETIME NULL,
    [pro_fecha_creacion] DATETIME NULL DEFAULT GETDATE(),
    [cat_id] INT NULL,
    [prov_id] INT NULL,
    [prov_id_respaldo] INT NULL,
    CONSTRAINT FK_producto_categoria FOREIGN KEY ([cat_id]) REFERENCES [dbo].[tbl_categoria]([cat_id]),
    CONSTRAINT FK_producto_proveedor FOREIGN KEY ([prov_id]) REFERENCES [dbo].[tbl_proveedor]([prov_id])
);
GO

CREATE TABLE [dbo].[tbl_imagen_producto](
    [ipro_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [pro_id] INT NOT NULL,
    [ipro_ruta] VARCHAR(255) NOT NULL,
    [ipro_es_principal] BIT NOT NULL DEFAULT 0,
    [ipro_estado] CHAR(1) NULL DEFAULT 'A',
    [ipro_fecha_baja] DATETIME NULL,
    CONSTRAINT FK_imagen_producto FOREIGN KEY ([pro_id]) REFERENCES [dbo].[tbl_producto]([pro_id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[tbl_imagen_usuario](
    [iusu_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [usu_id] INT NOT NULL,
    [iusu_ruta] VARCHAR(255) NOT NULL,
    [iusu_es_principal] BIT NOT NULL DEFAULT 0,
    [iusu_estado] CHAR(1) NULL DEFAULT 'A',
    [iusu_fecha_baja] DATETIME NULL,
    CONSTRAINT FK_imagen_usuario FOREIGN KEY ([usu_id]) REFERENCES [dbo].[tbl_usuario]([usu_id]) ON DELETE CASCADE
);
GO

-- Respaldo padre-hijo: al eliminar proveedor se guarda prov_id en prov_id_respaldo
CREATE TRIGGER trg_proveedor_eliminar_respaldo
ON [dbo].[tbl_proveedor]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET p.prov_id_respaldo = p.prov_id,
        p.prov_id = NULL
    FROM [dbo].[tbl_producto] p
    INNER JOIN deleted d ON p.prov_id = d.prov_id;

    DELETE FROM [dbo].[tbl_proveedor]
    WHERE prov_id IN (SELECT prov_id FROM deleted);
END;
GO

CREATE PROCEDURE [dbo].[sp_restaurar_proveedor_productos]
    @prov_id INT
AS
BEGIN
    UPDATE [dbo].[tbl_producto]
    SET prov_id = @prov_id,
        prov_id_respaldo = NULL
    WHERE prov_id_respaldo = @prov_id;
END;
GO

-- ========== AUTENTICACIÓN: correo, OTP y recuperación ==========

-- Valida si el correo existe (antes de enviar OTP)
CREATE PROCEDURE [dbo].[sp_correo_existe]
    @correo VARCHAR(150),
    @existe BIT OUTPUT
AS
BEGIN
    SET @existe = CASE WHEN EXISTS (
        SELECT 1 FROM tbl_usuario WHERE usu_correo = @correo AND usu_estado = 'A'
    ) THEN 1 ELSE 0 END;
END;
GO

-- Valida cédula + correo (activos y bloqueados, para recuperación)
CREATE PROCEDURE [dbo].[sp_validar_cedula_correo]
    @cedula VARCHAR(10),
    @correo VARCHAR(150),
    @usu_id INT OUTPUT
AS
BEGIN
    SET @usu_id = NULL;
    SELECT @usu_id = usu_id
    FROM tbl_usuario
    WHERE usu_cedula = @cedula AND usu_correo = @correo AND usu_estado IN ('A', 'B');
END;
GO

CREATE PROCEDURE [dbo].[sp_obtener_estado_login]
    @correo VARCHAR(150),
    @usu_id INT OUTPUT,
    @estado CHAR(1) OUTPUT,
    @intentos INT OUTPUT
AS
BEGIN
    SET @usu_id = NULL;
    SET @estado = NULL;
    SET @intentos = 0;

    SELECT @usu_id = usu_id, @estado = usu_estado, @intentos = ISNULL(usu_intentos, 0)
    FROM tbl_usuario
    WHERE usu_correo = @correo AND usu_estado IN ('A', 'B');
END;
GO

CREATE PROCEDURE [dbo].[sp_login_registrar_fallo]
    @correo VARCHAR(150),
    @intentos INT OUTPUT,
    @bloqueado BIT OUTPUT
AS
BEGIN
    SET @intentos = 0;
    SET @bloqueado = 0;

    UPDATE tbl_usuario
    SET usu_intentos = ISNULL(usu_intentos, 0) + 1
    WHERE usu_correo = @correo AND usu_estado IN ('A', 'B');

    SELECT @intentos = usu_intentos FROM tbl_usuario WHERE usu_correo = @correo;

    IF @intentos >= 3
    BEGIN
        UPDATE tbl_usuario SET usu_estado = 'B' WHERE usu_correo = @correo;
        SET @bloqueado = 1;
    END
END;
GO

CREATE PROCEDURE [dbo].[sp_login_exitoso]
    @correo VARCHAR(150)
AS
BEGIN
    UPDATE tbl_usuario SET usu_intentos = 0 WHERE usu_correo = @correo AND usu_estado = 'A';
END;
GO

-- Genera y guarda OTP (login 2FA o recuperación). Válido 10 minutos.
CREATE PROCEDURE [dbo].[sp_generar_otp]
    @correo VARCHAR(150),
    @tipo VARCHAR(20), -- LOGIN | RECUPERACION
    @otp VARCHAR(10) OUTPUT,
    @usu_id INT OUTPUT
AS
BEGIN
    SET @otp = NULL;
    SET @usu_id = NULL;

    SELECT @usu_id = usu_id FROM tbl_usuario WHERE usu_correo = @correo AND usu_estado IN ('A', 'B');
    IF @usu_id IS NULL RETURN;

    SET @otp = RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS VARCHAR(6)), 6);

    UPDATE tbl_usuario
    SET usu_codigo_OTP = @otp,
        usu_otp_expira = DATEADD(MINUTE, 10, GETDATE()),
        usu_otp_tipo = @tipo
    WHERE usu_id = @usu_id;
END;
GO

-- Valida OTP ingresado (correo + código + tipo)
CREATE PROCEDURE [dbo].[sp_validar_otp]
    @correo VARCHAR(150),
    @otp VARCHAR(10),
    @tipo VARCHAR(20),
    @valido BIT OUTPUT,
    @usu_id INT OUTPUT
AS
BEGIN
    SET @valido = 0;
    SET @usu_id = NULL;

    SELECT @usu_id = usu_id
    FROM tbl_usuario
    WHERE usu_correo = @correo
      AND usu_codigo_OTP = @otp
      AND usu_otp_tipo = @tipo
      AND usu_otp_expira >= GETDATE()
      AND usu_estado IN ('A', 'B');

    IF @usu_id IS NOT NULL
    BEGIN
        SET @valido = 1;
        UPDATE tbl_usuario
        SET usu_codigo_OTP = NULL, usu_otp_expira = NULL, usu_otp_tipo = NULL
        WHERE usu_id = @usu_id;
    END
END;
GO

-- Cambiar contraseña tras recuperación: desbloquea y reinicia intentos
CREATE PROCEDURE [dbo].[sp_cambiar_contrasena]
    @usu_id INT,
    @nueva_clave VARCHAR(50)
AS
BEGIN
    UPDATE tbl_usuario
    SET usu_contraseña = dbo.encritptaCon(@nueva_clave),
        usu_codigo_OTP = NULL,
        usu_otp_expira = NULL,
        usu_otp_tipo = NULL,
        usu_intentos = 0,
        usu_estado = 'A'
    WHERE usu_id = @usu_id AND usu_estado IN ('A', 'B');
END;
GO

-- ========== BORRADO LÓGICO (estado I) Y BORRADO FÍSICO (DELETE) ==========
-- A = Activo | B = Bloqueado | I = Inactivo (borrado lógico)

-- PRODUCTO
CREATE PROCEDURE [dbo].[sp_producto_borrado_logico] @pro_id INT
AS
BEGIN
    UPDATE tbl_producto SET pro_estado = 'I', pro_fecha_baja = GETDATE() WHERE pro_id = @pro_id;
    UPDATE tbl_imagen_producto SET ipro_estado = 'I', ipro_fecha_baja = GETDATE() WHERE pro_id = @pro_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_producto_borrado_fisico] @pro_id INT
AS
BEGIN
    DELETE FROM tbl_imagen_producto WHERE pro_id = @pro_id;
    DELETE FROM tbl_producto WHERE pro_id = @pro_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_producto_reactivar] @pro_id INT
AS
BEGIN
    UPDATE tbl_producto SET pro_estado = 'A', pro_fecha_baja = NULL WHERE pro_id = @pro_id;
    UPDATE tbl_imagen_producto SET ipro_estado = 'A', ipro_fecha_baja = NULL WHERE pro_id = @pro_id;
END;
GO

-- PROVEEDOR (físico usa trigger de respaldo padre-hijo)
CREATE PROCEDURE [dbo].[sp_proveedor_borrado_logico] @prov_id INT
AS
BEGIN
    UPDATE tbl_proveedor SET prov_estado = 'I', prov_fecha_baja = GETDATE() WHERE prov_id = @prov_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_proveedor_borrado_fisico] @prov_id INT
AS
BEGIN
    DELETE FROM tbl_proveedor WHERE prov_id = @prov_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_proveedor_reactivar] @prov_id INT
AS
BEGIN
    UPDATE tbl_proveedor SET prov_estado = 'A', prov_fecha_baja = NULL WHERE prov_id = @prov_id;
END;
GO

-- USUARIO
CREATE PROCEDURE [dbo].[sp_usuario_borrado_logico] @usu_id INT
AS
BEGIN
    UPDATE tbl_usuario SET usu_estado = 'I', usu_fecha_baja = GETDATE() WHERE usu_id = @usu_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_usuario_borrado_fisico] @usu_id INT
AS
BEGIN
    DELETE FROM tbl_imagen_usuario WHERE usu_id = @usu_id;
    DELETE FROM tbl_usuario WHERE usu_id = @usu_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_usuario_reactivar] @usu_id INT
AS
BEGIN
    UPDATE tbl_usuario SET usu_estado = 'A', usu_fecha_baja = NULL WHERE usu_id = @usu_id;
END;
GO

-- CATEGORÍA
CREATE PROCEDURE [dbo].[sp_categoria_borrado_logico] @cat_id INT
AS
BEGIN
    UPDATE tbl_categoria SET cat_estado = 'I', cat_fecha_baja = GETDATE() WHERE cat_id = @cat_id;
END;
GO

CREATE PROCEDURE [dbo].[sp_categoria_borrado_fisico] @cat_id INT
AS
BEGIN
    UPDATE tbl_producto SET cat_id = NULL WHERE cat_id = @cat_id;
    DELETE FROM tbl_categoria WHERE cat_id = @cat_id;
END;
GO

INSERT INTO tbl_tipo_usuario (tusu_nombre, tusu_estado) VALUES ('Administrador', 'A'), ('Cliente', 'A');

-- Correo remitente del sistema: animaliahogar@gmail.com (configurado en Web.config)
INSERT INTO tbl_usuario (usu_cedula, usu_nombres, usu_apellidos, usu_correo, usu_nick, usu_contraseña, tusu_id, usu_estado)
VALUES
('1720000001', 'Byron', 'Segab', 'byrong.segab@gmail.com', 'byron.admin', dbo.encritptaCon('Admin123'), 1, 'A'),
('1720000002', 'Jadan', 'Vinicio', 'jadan.vinicio77@gmail.com', 'jadan.user', dbo.encritptaCon('User123'), 2, 'A');

INSERT INTO tbl_categoria (cat_nombre) VALUES ('Electrónica'), ('Ropa'), ('Alimentos'), ('Hogar'), ('Deportes');
INSERT INTO tbl_proveedor (prov_nombre, prov_ruc, prov_telefono, prov_correo, prov_direccion)
VALUES ('TechSupply SA', '1790123456001', '0991234567', 'ventas@techsupply.com', 'Quito'),
       ('ModaExpress', '1790987654001', '0987654321', 'info@modaexpress.com', 'Guayaquil');

INSERT INTO tbl_producto (pro_nombre, pro_descripcion, pro_precio, pro_stock, cat_id, prov_id)
VALUES ('Laptop HP 15', 'Laptop 8GB RAM 256GB SSD', 650.00, 25, 1, 1),
       ('Camiseta Nike', 'Camiseta deportiva talla M', 35.50, 100, 2, 2),
       ('Arroz Premium 5kg', 'Arroz de grano largo', 4.80, 500, 3, 1),
       ('Silla Ergonómica', 'Silla de oficina ajustable', 120.00, 40, 4, 1),
       ('Balón Fútbol', 'Balón oficial talla 5', 28.00, 80, 5, 2);
GO
