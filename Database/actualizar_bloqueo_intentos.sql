-- Ejecutar en dis_deber2_db si ya creaste la base antes
USE [dis_deber2_db];
GO

-- Bloqueo por intentos fallidos: estado B = Bloqueado
-- A = Activo | B = Bloqueado | I = Inactivo (borrado lógico)

IF OBJECT_ID('sp_login_registrar_fallo', 'P') IS NOT NULL DROP PROCEDURE sp_login_registrar_fallo;
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

IF OBJECT_ID('sp_login_exitoso', 'P') IS NOT NULL DROP PROCEDURE sp_login_exitoso;
GO
CREATE PROCEDURE [dbo].[sp_login_exitoso]
    @correo VARCHAR(150)
AS
BEGIN
    UPDATE tbl_usuario
    SET usu_intentos = 0
    WHERE usu_correo = @correo AND usu_estado = 'A';
END;
GO

IF OBJECT_ID('sp_obtener_estado_login', 'P') IS NOT NULL DROP PROCEDURE sp_obtener_estado_login;
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

-- Recuperación: permitir cuentas bloqueadas
ALTER PROCEDURE [dbo].[sp_validar_cedula_correo]
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

ALTER PROCEDURE [dbo].[sp_generar_otp]
    @correo VARCHAR(150),
    @tipo VARCHAR(20),
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

ALTER PROCEDURE [dbo].[sp_validar_otp]
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

ALTER PROCEDURE [dbo].[sp_cambiar_contrasena]
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

PRINT 'Bloqueo por intentos configurado. Estado B = cuenta bloqueada.';
GO
