using System;
using System.Web;

namespace dis_deber2.CapaPresentacion
{
    public static class SesionHelper
    {
        public const int RolAdministrador = 1;
        public const int RolCliente = 2;

        public static bool EstaAutenticado()
        {
            return HttpContext.Current.Session["usu_id"] != null;
        }

        public static int ObtenerTipoUsuarioId()
        {
            var valor = HttpContext.Current.Session["tusu_id"];
            return valor != null ? Convert.ToInt32(valor) : 0;
        }

        public static bool EsAdmin()
        {
            return ObtenerTipoUsuarioId() == RolAdministrador;
        }

        public static bool EsCliente()
        {
            return ObtenerTipoUsuarioId() == RolCliente;
        }

        public static bool RequerirLogin()
        {
            if (!EstaAutenticado())
            {
                Redirigir("~/Login.aspx");
                return false;
            }
            return true;
        }

        public static bool RequerirAdmin()
        {
            if (!RequerirLogin())
                return false;

            if (!EsAdmin())
            {
                Redirigir("~/Catalogo.aspx");
                return false;
            }
            return true;
        }

        public static bool RequerirCliente()
        {
            if (!RequerirLogin())
                return false;

            if (!EsCliente())
            {
                Redirigir("~/Default.aspx");
                return false;
            }
            return true;
        }

        /// <summary>Redirect sin ThreadAbortException (evita Runtime Error en hosting compartido).</summary>
        public static void Redirigir(string url)
        {
            var ctx = HttpContext.Current;
            if (ctx == null) return;
            ctx.Response.Redirect(url, false);
            ctx.ApplicationInstance.CompleteRequest();
        }

        public static void Iniciar(int usuId, string nombre, string correo, int tipoUsuarioId, string tipoNombre)
        {
            var s = HttpContext.Current.Session;
            s["usu_id"] = usuId;
            s["usu_nombre"] = nombre;
            s["usu_correo"] = correo;
            s["tusu_id"] = tipoUsuarioId;
            s["tusu_nombre"] = tipoNombre;
        }

        public static void Cerrar()
        {
            HttpContext.Current.Session.Clear();
        }
    }
}
