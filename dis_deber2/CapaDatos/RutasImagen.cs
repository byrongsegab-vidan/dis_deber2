using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace dis_deber2.CapaDatos
{
    public static class RutasImagen
    {
        /// <summary>Carpeta raíz para archivos subidos por el usuario.</summary>
        public const string CarpetaRaiz = "Subidas";

        /// <summary>Imágenes de productos (mínimo 3 por producto).</summary>
        public const string CarpetaProductos = "Productos";

        /// <summary>Imágenes de perfil de usuario.</summary>
        public const string CarpetaUsuarios = "Usuarios";

        /// <summary>Imágenes fijas del sitio (login, banners).</summary>
        public const string CarpetaImagenesSitio = "Imagenes";

        private static readonly string[] ExtensionesImagenPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int TamanoMaximoImagenBytes = 5 * 1024 * 1024;

        public static string RutaRelativa(string carpeta, string nombreArchivo)
        {
            return "~/" + CarpetaRaiz + "/" + carpeta + "/" + nombreArchivo;
        }

        public static string RutaFisica(string carpeta, string nombreArchivo)
        {
            return HttpContext.Current.Server.MapPath(RutaRelativa(carpeta, nombreArchivo));
        }

        public static string RutaImagenSitio(string nombreArchivo)
        {
            return "~/Content/" + CarpetaImagenesSitio + "/" + nombreArchivo;
        }

        public static string ResolverUrl(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
                return "https://placehold.co/80x80?text=Sin+img";

            if (rutaRelativa.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return rutaRelativa;

            // Compatibilidad con rutas antiguas en inglés guardadas en BD
            if (rutaRelativa.Contains("/Uploads/"))
                rutaRelativa = rutaRelativa.Replace("/Uploads/", "/" + CarpetaRaiz + "/");

            return ResolveApp(rutaRelativa);
        }

        public static string GuardarArchivo(HttpPostedFile archivo, string carpeta)
        {
            if (archivo == null || archivo.ContentLength == 0)
                return null;

            var extension = Path.GetExtension(archivo.FileName);
            var nombre = Guid.NewGuid().ToString("N") + extension;
            var carpetaFisica = HttpContext.Current.Server.MapPath("~/" + CarpetaRaiz + "/" + carpeta + "/");

            if (!Directory.Exists(carpetaFisica))
                Directory.CreateDirectory(carpetaFisica);

            var rutaCompleta = Path.Combine(carpetaFisica, nombre);
            archivo.SaveAs(rutaCompleta);
            return RutaRelativa(carpeta, nombre);
        }

        public static bool EsImagenValida(HttpPostedFile archivo, out string mensaje)
        {
            mensaje = null;

            if (archivo == null || archivo.ContentLength == 0)
            {
                mensaje = "Debe seleccionar una imagen.";
                return false;
            }

            if (archivo.ContentLength > TamanoMaximoImagenBytes)
            {
                mensaje = "La imagen no puede superar 5 MB.";
                return false;
            }

            var extension = Path.GetExtension(archivo.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !ExtensionesImagenPermitidas.Contains(extension))
            {
                mensaje = "Solo se permiten imágenes JPG, JPEG, PNG, GIF o WEBP.";
                return false;
            }

            var tipo = (archivo.ContentType ?? string.Empty).ToLowerInvariant();
            if (!tipo.StartsWith("image/"))
            {
                mensaje = "El archivo seleccionado no es una imagen válida.";
                return false;
            }

            return true;
        }

        public static string GuardarImagen(HttpPostedFile archivo, string carpeta, out string mensaje)
        {
            if (!EsImagenValida(archivo, out mensaje))
                return null;

            return GuardarArchivo(archivo, carpeta);
        }

        /// <summary>Descarga una imagen desde URL o reutiliza una ruta virtual existente.</summary>
        public static string GuardarDesdeUrl(string url, string carpeta, out string mensaje)
        {
            mensaje = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                mensaje = "URL vacía.";
                return null;
            }

            url = url.Trim();

            if (url.StartsWith("~/", StringComparison.Ordinal) || url.StartsWith("/Subidas/", StringComparison.OrdinalIgnoreCase))
            {
                if (url.StartsWith("/Subidas/"))
                    url = "~/" + CarpetaRaiz + url.Substring("/Subidas".Length);
                return url;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                mensaje = "La URL debe comenzar con http:// o https://";
                return null;
            }

            try
            {
                byte[] datos;
                using (var client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent, "dis_deber2-carga-masiva/1.0");
                    datos = client.DownloadData(url);
                }

                if (datos == null || datos.Length == 0)
                {
                    mensaje = "La URL no devolvió datos.";
                    return null;
                }

                if (datos.Length > TamanoMaximoImagenBytes)
                {
                    mensaje = "La imagen supera 5 MB.";
                    return null;
                }

                var extension = ObtenerExtensionDesdeUrl(url);
                if (!ExtensionesImagenPermitidas.Contains(extension))
                    extension = ".jpg";

                var nombre = Guid.NewGuid().ToString("N") + extension;
                var carpetaFisica = HttpContext.Current.Server.MapPath("~/" + CarpetaRaiz + "/" + carpeta + "/");
                if (!Directory.Exists(carpetaFisica))
                    Directory.CreateDirectory(carpetaFisica);

                File.WriteAllBytes(Path.Combine(carpetaFisica, nombre), datos);
                return RutaRelativa(carpeta, nombre);
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return null;
            }
        }

        private static string ObtenerExtensionDesdeUrl(string url)
        {
            try
            {
                var path = new Uri(url).AbsolutePath;
                var ext = Path.GetExtension(path)?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(ext) && ExtensionesImagenPermitidas.Contains(ext))
                    return ext;
            }
            catch
            {
                // ignorar
            }

            return ".jpg";
        }

        private static string ResolveApp(string virtualPath)
        {
            var app = HttpContext.Current.Request.ApplicationPath;
            if (app == "/")
                return VirtualPathUtility.ToAbsolute(virtualPath);

            return app.TrimEnd('/') + VirtualPathUtility.ToAbsolute(virtualPath);
        }
    }
}
