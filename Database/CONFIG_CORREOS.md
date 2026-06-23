# Configuración de correos

## Cuentas de login (base de datos)

| Rol | Correo | Contraseña |
|-----|--------|------------|
| **Administrador** | byrong.segab@gmail.com | Admin123 |
| **Usuario (Cliente)** | jadan.vinicio77@gmail.com | User123 |

## Correo remitente (envía OTP, QR, recuperación)

**animaliahogar@gmail.com**

Este correo es el que usa el sistema para **enviar** mensajes. Los usuarios **reciben** el OTP en su propio correo (byrong, jadan, etc.).

Configurado en `Web.config`:
- `CorreoRemitente` = animaliahogar@gmail.com
- `SmtpUsuario` = animaliahogar@gmail.com

---

## Cómo obtener la contraseña de aplicación (Gmail)

En tu captura estás en **Seguridad y acceso**. Ya tienes **Verificación en 2 pasos** activada (6 ene), que es requisito.

### Pasos

1. En la misma pantalla, haz clic en **Verificación en 2 pasos**.
2. Baja hasta el final de esa página.
3. Busca **Contraseñas de aplicaciones** (App passwords).
4. Clic en **Contraseñas de aplicaciones**.
5. Nombre de la app: por ejemplo `dis_deber2`.
6. Google genera un código de **16 caracteres** (ej: `abcd efgh ijkl mnop`).
7. Copia ese código **sin espacios** en `Web.config`:

```xml
<add key="SmtpClave" value="abcdefghijklmnop" />
```

Y también en:

```xml
<network ... password="abcdefghijklmnop" ... />
```

### Acceso directo

https://myaccount.google.com/apppasswords

(Solo funciona si la verificación en 2 pasos está activa, como en tu cuenta.)

### Si no ves "Contraseñas de aplicaciones"

- Confirma que la verificación en 2 pasos sigue activa.
- Usa el buscador arriba en Google Cuenta: escribe **contraseñas de aplicaciones**.

---

## Script SQL de actualización

Si ya creaste la BD antes, ejecuta:

`Database/actualizar_usuarios_correos.sql`

No borra la base de datos; solo inserta/actualiza los usuarios.
