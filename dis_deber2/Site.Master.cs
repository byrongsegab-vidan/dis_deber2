using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.EstaAutenticado())
                return;

            var nombre = Session["usu_nombre"] + " (" + Session["tusu_nombre"] + ")";

            if (SesionHelper.EsAdmin())
            {
                ConfigurarLayoutAdmin(nombre);
            }
            else if (SesionHelper.EsCliente())
            {
                ConfigurarLayoutCliente(nombre);
            }
        }

        private void ConfigurarLayoutAdmin(string nombre)
        {
            pnlRoot.CssClass = "admin-shell";
            pnlSidebar.Visible = true;
            pnlSidebarCliente.Visible = false;
            pnlAdminTopbar.Visible = true;
            pnlNavbarCliente.Visible = false;
            pnlFooterCliente.Visible = false;
            pnlContentWrap.CssClass = "admin-content admin-inner-pages";

            lblUsuarioAdmin.Text = nombre;
            MarcarNavAdminActivo();
        }

        private void ConfigurarLayoutCliente(string nombre)
        {
            pnlRoot.CssClass = "admin-shell";
            pnlSidebar.Visible = false;
            pnlSidebarCliente.Visible = true;
            pnlAdminTopbar.Visible = true;
            pnlNavbarCliente.Visible = false;
            pnlFooterCliente.Visible = false;
            pnlContentWrap.CssClass = "admin-content admin-inner-pages";

            lblUsuarioAdmin.Text = nombre;
            var cantidadCarrito = CarritoHelper.CantidadProductos().ToString();
            lblNavCarritoCli.Text = cantidadCarrito;
            MarcarNavClienteActivo();
        }

        private void MarcarNavAdminActivo()
        {
            var pagina = Path.GetFileName(Request.Path).ToLowerInvariant();
            ResaltarNav(navInicio, pagina == "default.aspx" || pagina == "");
            ResaltarNav(navPerfil, pagina == "perfil.aspx");
            ResaltarNav(navBuscar, pagina == "buscarproductos.aspx");
            ResaltarNav(navProductos, pagina == "productos.aspx");
            ResaltarNav(navProveedores, pagina == "proveedores.aspx" || pagina == "categorias.aspx");
            ResaltarNav(navEstadisticas, pagina == "estadisticas.aspx");
            ResaltarNav(navCarga, pagina == "cargamasiva.aspx");
        }

        private void MarcarNavClienteActivo()
        {
            var pagina = Path.GetFileName(Request.Path).ToLowerInvariant();
            ResaltarNav(navCliCatalogo, pagina == "catalogo.aspx");
            ResaltarNav(navCliCarrito, pagina == "carrito.aspx");
            ResaltarNav(navCliRombito, pagina == "rombito.aspx");
            ResaltarNav(navCliPerfil, pagina == "perfil.aspx");
        }

        private static void ResaltarNav(HtmlAnchor enlace, bool activo)
        {
            if (enlace == null) return;
            enlace.Attributes["class"] = activo ? "active" : string.Empty;
        }

        protected void lnkSalir_Click(object sender, EventArgs e)
        {
            SesionHelper.Cerrar();
            SesionHelper.Redirigir("~/Login.aspx");
        }
    }
}
