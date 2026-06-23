using System;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class BuscarProductos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirAdmin()) return;
            if (!IsPostBack)
            {
                ddlCategoria.DataSource = new categoria().ListarActivas();
                ddlCategoria.DataTextField = "cat_nombre";
                ddlCategoria.DataValueField = "cat_id";
                ddlCategoria.DataBind();
                ddlCategoria.Items.Insert(0, new ListItem("Todas", ""));
                if (!string.IsNullOrEmpty(Request.QueryString["q"]))
                    txtBuscar.Text = Request.QueryString["q"];
                Buscar();
            }
        }

        protected string RutasImagenUrl(object ruta) => RutasImagen.ResolverUrl(ruta?.ToString());

        protected void txtBuscar_TextChanged(object sender, EventArgs e) => Buscar();
        protected void FiltroChanged(object sender, EventArgs e) => Buscar();

        private void Buscar()
        {
            int? catId = string.IsNullOrEmpty(ddlCategoria.SelectedValue) ? (int?)null : int.Parse(ddlCategoria.SelectedValue);
            decimal? min = decimal.TryParse(txtPrecioMin.Text, out var pmin) ? pmin : (decimal?)null;
            decimal? max = decimal.TryParse(txtPrecioMax.Text, out var pmax) ? pmax : (decimal?)null;

            var dt = new pro().BusquedaRapida(txtBuscar.Text.Trim(), catId, min, max, chkOrdenAlfabetico.Checked);
            rptResultados.DataSource = dt;
            rptResultados.DataBind();
            lblSinResultados.Visible = dt.Rows.Count == 0;
        }
    }
}
