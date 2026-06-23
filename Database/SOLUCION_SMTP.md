# Solucionar error SMTP (Gmail)

## Error que viste
*"Codigo generado pero no se pudo enviar el correo"*

El OTP **si se genera** en la base de datos. Falla el envio por Gmail SMTP.

---

## Pasos para corregir (animaliahogar@gmail.com)

### 1. Verificar contraseña de aplicacion
1. Inicia sesion en Google con **animaliahogar@gmail.com**
2. Ve a: https://myaccount.google.com/apppasswords
3. Crea una nueva contraseña de aplicacion (nombre: `dis_deber2`)
4. Copia los 16 caracteres **sin espacios**
5. En `Web.config`, actualiza **los dos lugares**:
   - `appSettings` → `SmtpClave`
   - `system.net/mailSettings/network` → `password`

### 2. Verificar que coincidan
```xml
<add key="SmtpUsuario" value="animaliahogar@gmail.com" />
<add key="CorreoRemitente" value="animaliahogar@gmail.com" />
```
Usuario SMTP y remitente deben ser la **misma cuenta** que genero la contraseña de aplicacion.

### 3. Reiniciar IIS Express
Despues de cambiar Web.config:
- Detener la app (Shift+F5)
- Volver a ejecutar (F5)

---

## Modo desarrollo (mientras arreglas Gmail)

Con `debug="true"` en Web.config, si el correo falla la pantalla muestra:
**"Modo desarrollo: puede usar el codigo OTP generado: XXXXXX"**

Asi puedes probar recuperacion aunque Gmail falle.

---

## Errores comunes de Gmail

| Mensaje | Causa |
|---------|--------|
| Authentication failed | Contrasena de aplicacion incorrecta o expirada |
| Must issue STARTTLS | Ya corregido con TLS 1.2 en el codigo |
| Less secure apps | Usar contrasena de **aplicacion**, no la clave normal |

---

## Probar de nuevo
1. Recuperar cuenta con cedula + correo registrados
2. Si Gmail funciona → revisa bandeja (y spam) de chesterbening05@gmail.com
3. Si falla → lee el mensaje de error detallado en pantalla
