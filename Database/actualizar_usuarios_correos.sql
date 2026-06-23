-- ============================================================
-- Script de ACTUALIZACIÓN (no borra la base de datos)
-- Ejecutar en SSMS conectado a dis_deber2_db
-- ============================================================

USE [dis_deber2_db];
GO

-- 1. Usuarios: administrador y cliente
IF NOT EXISTS (SELECT 1 FROM tbl_usuario WHERE usu_correo = 'byrong.segab@gmail.com')
BEGIN
    INSERT INTO tbl_usuario (usu_cedula, usu_nombres, usu_apellidos, usu_correo, usu_nick, usu_contraseña, tusu_id, usu_estado)
    VALUES ('1720000001', 'Byron', 'Segab', 'byrong.segab@gmail.com', 'byron.admin', dbo.encritptaCon('Admin123'), 1, 'A');
END
ELSE
BEGIN
    UPDATE tbl_usuario
    SET usu_cedula = '1720000001',
        usu_nombres = 'Byron',
        usu_apellidos = 'Segab',
        usu_nick = 'byron.admin',
        usu_contraseña = dbo.encritptaCon('Admin123'),
        tusu_id = 1,
        usu_estado = 'A',
        usu_fecha_baja = NULL
    WHERE usu_correo = 'byrong.segab@gmail.com';
END
GO

IF NOT EXISTS (SELECT 1 FROM tbl_usuario WHERE usu_correo = 'jadan.vinicio77@gmail.com')
BEGIN
    INSERT INTO tbl_usuario (usu_cedula, usu_nombres, usu_apellidos, usu_correo, usu_nick, usu_contraseña, tusu_id, usu_estado)
    VALUES ('1720000002', 'Jadan', 'Vinicio', 'jadan.vinicio77@gmail.com', 'jadan.user', dbo.encritptaCon('User123'), 2, 'A');
END
ELSE
BEGIN
    UPDATE tbl_usuario
    SET usu_cedula = '1720000002',
        usu_nombres = 'Jadan',
        usu_apellidos = 'Vinicio',
        usu_nick = 'jadan.user',
        usu_contraseña = dbo.encritptaCon('User123'),
        tusu_id = 2,
        usu_estado = 'A',
        usu_fecha_baja = NULL
    WHERE usu_correo = 'jadan.vinicio77@gmail.com';
END
GO

-- Desactivar usuario demo anterior si existía
IF EXISTS (SELECT 1 FROM tbl_usuario WHERE usu_correo = 'admin@itsco.edu.ec')
BEGIN
    UPDATE tbl_usuario SET usu_estado = 'I', usu_fecha_baja = GETDATE() WHERE usu_correo = 'admin@itsco.edu.ec';
END
GO

-- 2. Verificación
SELECT u.usu_id, u.usu_correo, u.usu_nombres, u.usu_apellidos, t.tusu_nombre, u.usu_estado,
       dbo.desencriptaCon(u.usu_contraseña) AS clave
FROM tbl_usuario u
LEFT JOIN tbl_tipo_usuario t ON u.tusu_id = t.tusu_id
WHERE u.usu_correo IN ('byrong.segab@gmail.com', 'jadan.vinicio77@gmail.com');
GO

PRINT 'Actualización de usuarios completada.';
GO
