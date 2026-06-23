using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class _Default : Page
    {
        private static readonly string[] ColoresDonut = { "#FF6B00", "#7F56D9", "#00B4D8", "#10B981", "#FF007A", "#FFB347" };
        private static readonly string[] IconosCategoria = { "??", "??", "??", "??", "??", "??" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirLogin()) return;

            if (SesionHelper.EsCliente())
            {
                Response.Redirect("~/Catalogo.aspx");
                return;
            }

            pnlAdmin.Visible = SesionHelper.EsAdmin();

            if (!IsPostBack && SesionHelper.EsAdmin())
                CargarDashboard();
        }

        protected void btnBuscarDash_Click(object sender, EventArgs e)
        {
            var q = txtBuscarDash.Text.Trim();
            Response.Redirect(string.IsNullOrEmpty(q)
                ? "~/BuscarProductos.aspx"
                : "~/BuscarProductos.aspx?q=" + Server.UrlEncode(q));
        }

        private void CargarDashboard()
        {
            var stats = new pro().EstadisticasPorCategoria();
            var totalProductos = stats.AsEnumerable().Sum(r => Convert.ToInt32(r["total_productos"]));
            lblTotalProductos.Text = totalProductos.ToString();

            donutChart.Attributes["style"] = "background: conic-gradient(" + ConstruirDonut(stats) + ");";

            rptCategorias.DataSource = stats;
            rptCategorias.DataBind();

            var proveedores = new prov().ListarActivos();
            if (proveedores.Rows.Count > 0)
                rptProveedores.DataSource = proveedores.AsEnumerable().Take(3).CopyToDataTable();
            else
                rptProveedores.DataSource = proveedores;
            rptProveedores.DataBind();

            lblTotalProveedores.Text = proveedores.Rows.Count.ToString();
        }

        protected string IconoCategoria(int index)
        {
            return IconosCategoria[index % IconosCategoria.Length];
        }

        private string ConstruirDonut(DataTable stats)
        {
            if (stats.Rows.Count == 0)
                return "#E5E7EB 0% 100%";

            var total = stats.AsEnumerable().Sum(r => Convert.ToInt32(r["total_productos"]));
            if (total == 0)
                return "#E5E7EB 0% 100%";

            var sb = new StringBuilder();
            double acumulado = 0;

            for (var i = 0; i < stats.Rows.Count; i++)
            {
                var cant = Convert.ToInt32(stats.Rows[i]["total_productos"]);
                var pct = cant * 100.0 / total;
                var color = ColoresDonut[i % ColoresDonut.Length];
                var fin = acumulado + pct;
                sb.AppendFormat("{0} {1:F1}% {2:F1}%, ", color, acumulado, fin);
                acumulado = fin;
            }

            return sb.ToString().TrimEnd(',', ' ');
        }
    }
}
