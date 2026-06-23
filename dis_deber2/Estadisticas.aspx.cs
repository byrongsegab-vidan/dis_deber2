using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Estadisticas : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirAdmin()) return;
            if (!IsPostBack)
            {
                CargarCharts();
                if (int.TryParse(Request.QueryString["id"], out var id))
                    CargarCarrusel(id);
            }
        }

        private void CargarCharts()
        {
            var dt = new pro().EstadisticasPorCategoria();
            hfChartLabels.Value = string.Join("|", dt.AsEnumerable().Select(r => r["categoria"].ToString()));
            hfChartTotales.Value = string.Join("|", dt.AsEnumerable().Select(r => r["total_productos"].ToString()));
            hfChartStock.Value = string.Join("|", dt.AsEnumerable().Select(r => r["stock_total"].ToString()));
        }

        private void CargarCarrusel(int productoId)
        {
            var p = new pro();
            var row = p.ObtenerPorId(productoId);
            if (row == null) return;

            lblProducto.Text = row["pro_nombre"] + " - $" + row["pro_precio"];
            pnlProducto.Visible = true;

            var imgs = new imagen_producto().ListarPorProducto(productoId);
            var dt = new DataTable();
            dt.Columns.Add("url");
            dt.Columns.Add("activo", typeof(bool));

            var first = true;
            if (!string.IsNullOrEmpty(row["pro_imagen_ruta"]?.ToString()))
            {
                dt.Rows.Add(RutasImagen.ResolverUrl(row["pro_imagen_ruta"].ToString()), true);
                first = false;
            }

            foreach (DataRow img in imgs.Rows)
            {
                dt.Rows.Add(RutasImagen.ResolverUrl(img["ipro_ruta"].ToString()), first);
                first = false;
            }

            rptCarrusel.DataSource = dt;
            rptCarrusel.DataBind();
        }
    }
}
