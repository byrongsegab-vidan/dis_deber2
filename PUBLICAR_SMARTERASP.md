# Publicación en SmarterASP.NET

## 1. Crear cuenta
- Ir a https://www.smarterasp.net y registrarse (60 días gratis, HTTPS incluido).
- Crear sitio ASP.NET (.NET 4.x) y base de datos SQL Server desde el panel.

## 2. Base de datos
1. En el panel SmarterASP → Databases → SQL Server → Create Database.
2. Abrir SQL Management Studio o el phpMyAdmin-like tool del panel.
3. Ejecutar `Database/dis_deber2_db.sql` (ajustar `CREATE DATABASE` si el hosting ya creó la BD).
4. Anotar: **Server**, **Database name**, **User**, **Password**.

## 3. Connection string
En `Web.config`, reemplazar `cnDisDeber2`:
```xml
<add name="cnDisDeber2"
     connectionString="Data Source=TU_SERVIDOR;Initial Catalog=TU_BD;User Id=TU_USUARIO;Password=TU_CLAVE;"
     providerName="System.Data.SqlClient" />
```

## 4. Publicar desde Visual Studio
1. Compilar en **Release**.
2. Clic derecho en el proyecto → **Publicar**.
3. Elegir **FTP** o **Web Deploy** (SmarterASP lo indica en el panel).
4. Subir todo el contenido publicado.

## 5. Carpetas de imágenes en hosting
- Crear en el servidor (File Manager): `/Subidas/Productos/` y `/Subidas/Usuarios/`.
- Dar permisos de escritura (Write) a esas carpetas.
- En código se guarda ruta relativa: `~/Subidas/Productos/guid.jpg`
- En hosting la URL será: `https://tudominio.smarterasp.net/Subidas/Productos/guid.jpg`

## 6. HTTPS y dominio
- SmarterASP activa SSL gratis en subdominio `tunombre.smarterasp.net`.
- Dominio propio: Panel → Domains → apuntar DNS al hosting.

## 7. Login demo
- Admin: `byrong.segab@gmail.com` / `Admin123`
- Usuario: `jadan.vinicio77@gmail.com` / `User123`
