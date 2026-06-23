using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Catalogo : Page
    {
        private const int TamanoPagina = 8;

        private int PaginaActual
        {
            get { return ViewState["Pagina"] != null ? (int)ViewState["Pagina"] : 1; }
            set { ViewState["Pagina"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirCliente()) return;

            if (!IsPostBack)
            {
                CargarCategorias();
                CargarProductos();
            }

            ActualizarContadorCarrito();

            if (IsPostBack)
                pnlVisualizador.Visible = false;
        }

        protected string RutasImagenUrl(object ruta) => RutasImagen.ResolverUrl(ruta?.ToString());

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            PaginaActual = 1;
            gvProductos.PageIndex = 0;
            CargarProductos();
        }

        protected void chkOrdenAlfabetico_CheckedChanged(object sender, EventArgs e)
        {
            PaginaActual = 1;
            gvProductos.PageIndex = 0;
            CargarProductos();
        }

        protected void gvProductos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProductos.PageIndex = e.NewPageIndex;
            PaginaActual = e.NewPageIndex + 1;
            CargarProductos();
        }

        protected void gvProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Visualizar")
            {
                MostrarCarruselProducto(int.Parse(e.CommandArgument.ToString()));
                return;
            }

            if (e.CommandName != "Agregar")
                return;

            var proId = Convert.ToInt32(e.CommandArgument);
            var row = (GridViewRow)((Control)e.CommandSource).NamingContainer;
            var txtCantidad = (TextBox)row.FindControl("txtCantidad");
            var cantidad = int.TryParse(txtCantidad.Text, out var qty) ? qty : 0;

            var mensaje = CarritoHelper.Agregar(proId, cantidad);
            MostrarMensaje(mensaje, mensaje.StartsWith("¡"));
            ActualizarContadorCarrito();
            CargarProductos();
        }

        protected void btnCerrarVisualizador_Click(object sender, EventArgs e)
        {
            pnlVisualizador.Visible = false;
        }

        private void CargarCategorias()
        {
            ddlCategoria.DataSource = new categoria().ListarActivas();
            ddlCategoria.DataTextField = "cat_nombre";
            ddlCategoria.DataValueField = "cat_id";
            ddlCategoria.DataBind();
            ddlCategoria.Items.Insert(0, new ListItem("Todas las categorías", ""));
        }

        private void CargarProductos()
        {
            int? catId = string.IsNullOrEmpty(ddlCategoria.SelectedValue)
                ? (int?)null
                : int.Parse(ddlCategoria.SelectedValue);

            int total;
            var dt = new pro().ListarPaginado(
                txtBuscar.Text.Trim(),
                catId,
                null,
                null,
                null,
                PaginaActual,
                TamanoPagina,
                out total,
                incluirInactivos: false,
                ordenAlfabetico: chkOrdenAlfabetico.Checked);
            gvProductos.DataSource = dt;
            gvProductos.DataBind();
        }

        private void MostrarCarruselProducto(int productoId)
        {
            var row = new pro().ObtenerPorId(productoId);
            if (row == null)
            {
                MostrarMensaje("Producto no encontrado.", false);
                return;
            }

            if (row["pro_estado"]?.ToString()?.Trim() != "A")
            {
                MostrarMensaje("Este producto no está disponible.", false);
                return;
            }

            lblModalProducto.Text = row["pro_nombre"].ToString();

            var dt = new DataTable();
            dt.Columns.Add("url");
            dt.Columns.Add("activo", typeof(bool));

            var rutasVistas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var esPrimera = true;

            var rutaPrincipal = row["pro_imagen_ruta"]?.ToString();
            if (!string.IsNullOrWhiteSpace(rutaPrincipal))
            {
                dt.Rows.Add(RutasImagen.ResolverUrl(rutaPrincipal), true);
                rutasVistas.Add(rutaPrincipal);
                esPrimera = false;
            }

            var imgs = new imagen_producto().ListarPorProducto(productoId);
            foreach (DataRow img in imgs.Rows)
            {
                var ruta = img["ipro_ruta"]?.ToString();
                if (string.IsNullOrWhiteSpace(ruta) || rutasVistas.Contains(ruta))
                    continue;

                dt.Rows.Add(RutasImagen.ResolverUrl(ruta), esPrimera);
                rutasVistas.Add(ruta);
                esPrimera = false;
            }

            if (dt.Rows.Count == 0)
            {
                pnlSinImagenesModal.Visible = true;
                pnlCarruselModal.Visible = false;
            }
            else
            {
                pnlSinImagenesModal.Visible = false;
                pnlCarruselModal.Visible = true;
                rptCarruselModal.DataSource = dt;
                rptCarruselModal.DataBind();
            }

            pnlVisualizador.Visible = true;
        }

        private void ActualizarContadorCarrito()
        {
            lblCarritoCount.Text = CarritoHelper.CantidadProductos().ToString();
        }

        private void MostrarMensaje(string mensaje, bool exito)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = exito ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMensaje.Visible = true;
        }
    }
}
