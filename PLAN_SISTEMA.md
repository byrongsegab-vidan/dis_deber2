# Plan del sistema (dis_deber2)

Orden de trabajo: **BD local → funcionalidades → pruebas → publicar en SmarterASP al 100%**

---

## 1. Login (correo + contraseña)

- Validar que el **correo exista** en `tbl_usuario` antes de cualquier acción.
- Si no existe → mensaje: *"El correo no está registrado"*.
- SP: `sp_correo_existe`

## 2. Segundo factor: OTP + QR

Flujo:

1. Usuario ingresa correo (debe existir).
2. Sistema genera OTP de 6 dígitos → `sp_generar_otp` tipo `LOGIN`.
3. Envía OTP al **correo** (SMTP).
4. Muestra **QR** en pantalla con el mismo código (para escanear con cámara/celular).
5. Usuario ingresa OTP manualmente **o** escanea QR → `sp_validar_otp`.
6. Si es válido → sesión iniciada.

**QR:** librería QRCoder (NuGet). Contenido del QR: `OTP:123456|byrong.segab@gmail.com`

## 3. Recuperación de cuenta

Flujo:

1. Página `RecuperarCuenta.aspx`: cédula + correo.
2. Validar que **ambos coincidan** en el mismo usuario → `sp_validar_cedula_correo`.
3. Generar OTP tipo `RECUPERACION` → enviar al correo.
4. Usuario ingresa OTP → `sp_validar_otp`.
5. Formulario nueva contraseña → `sp_cambiar_contrasena`.

## 4. Carga masiva Excel + fotos

Columnas del Excel/CSV de productos:

| Columna | Descripción |
|---------|-------------|
| pro_nombre | Nombre del producto |
| pro_descripcion | Descripción |
| pro_precio | Precio |
| pro_stock | Stock |
| cat_id | ID categoría |
| prov_id | ID proveedor |
| **pro_imagen_archivo** | Nombre del archivo de imagen (ej: `laptop.jpg`) |

**Cómo funcionan las fotos:**

1. Subir Excel + carpeta ZIP con imágenes **o** subir imágenes aparte a `Subidas/Productos/`.
2. Al importar, el sistema busca `Subidas/Productos/{pro_imagen_archivo}`.
3. Guarda ruta relativa `~/Subidas/Productos/laptop.jpg` en `pro_imagen_ruta` y `tbl_imagen_producto`.
4. En grid y búsqueda se muestran con `RutasImagen.ResolverUrl()`.

**En hosting (SmarterASP):** misma estructura de carpetas `/Subidas/Productos/` con permiso de escritura.

## 6. Borrado lógico y borrado físico

| Tipo | Qué hace | Campo / acción |
|------|----------|----------------|
| **Borrado lógico** | Marca inactivo, no borra fila | `estado = 'I'` + `fecha_baja` |
| **Borrado físico** | Elimina fila de la BD | `DELETE` |
| **Reactivar** | Revive un borrado lógico | `estado = 'A'`, `fecha_baja = NULL` |

Stored procedures por entidad:
- Producto: `sp_producto_borrado_logico`, `sp_producto_borrado_fisico`, `sp_producto_reactivar`
- Proveedor: `sp_proveedor_borrado_logico`, `sp_proveedor_borrado_fisico`, `sp_proveedor_reactivar`
- Usuario: `sp_usuario_borrado_logico`, `sp_usuario_borrado_fisico`, `sp_usuario_reactivar`
- Categoría: `sp_categoria_borrado_logico`, `sp_categoria_borrado_fisico`

**Proveedor borrado físico:** el trigger `trg_proveedor_eliminar_respaldo` guarda `prov_id` en `prov_id_respaldo` de los productos hijos antes de eliminar.

Clases C#: métodos `EliminarLogico()`, `EliminarFisico()`, `Reactivar()` en `pro`, `prov`, `usuario`.

---

## 7. Publicación SmarterASP (al final)

Configurar en `Web.config`:

- **Connection string** SQL del panel SmarterASP.
- **SMTP** para envío de OTP (SmarterASP da servidor SMTP en el panel).
- Carpetas `Subidas/Productos` y `Subidas/Usuarios` con permisos Write.
- Compilar en **Release**, `debug="false"`.

Ver también: `PUBLICAR_SMARTERASP.md`

---

## Estado actual

| Parte | Estado |
|-------|--------|
| Script SQL (tablas + OTP + SPs) | Listo |
| Login básico correo/clave | Código base |
| OTP + QR + correo | Pendiente |
| Recuperación cuenta | Pendiente |
| Excel con imágenes automáticas | Pendiente (columna definida) |
| Publicación | Al final |
