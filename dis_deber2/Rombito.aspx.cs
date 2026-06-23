using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Rombito : Page
    {
        private int UsuarioId => Convert.ToInt32(Session["usu_id"]);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirCliente()) return;

            if (!IsPostBack)
                CargarHistorial();
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            var errorEntrada = GeneradorRombo.ValidarTexto(txtNumero.Text);
            if (errorEntrada != null)
            {
                Mostrar(errorEntrada, false);
                pnlResultado.Visible = false;
                return;
            }

            var n = int.Parse(txtNumero.Text.Trim());
            var resultado = GeneradorRombo.Generar(n);
            if (!resultado.Exito)
            {
                Mostrar(resultado.MensajeError, false);
                pnlResultado.Visible = false;
                return;
            }

            new figura_rombo().Guardar(UsuarioId, resultado.Numero, resultado.EsPar, resultado.Orientacion, resultado.Patron);

            litRombo.Text = Server.HtmlEncode(resultado.Patron);
            lblInfoRombo.Text = "n = " + resultado.Numero + " · " + resultado.Orientacion;
            pnlResultado.Visible = true;
            Mostrar("Rombito generado y guardado en MongoDB.", true);

            CargarHistorial();
        }

        protected void gvHistorial_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ver") return;

            var figId = Convert.ToInt32(e.CommandArgument);
            var patron = new figura_rombo().ObtenerPatron(figId, UsuarioId);
            if (string.IsNullOrEmpty(patron))
            {
                Mostrar("No se encontró la figura.", false);
                return;
            }

            var row = ((Control)e.CommandSource).NamingContainer as GridViewRow;
            if (row != null)
            {
                var numero = row.Cells[1].Text;
                var orientacion = row.Cells[2].Text;
                lblInfoRombo.Text = "n = " + numero + " · " + orientacion;
            }

            litRombo.Text = Server.HtmlEncode(patron);
            pnlResultado.Visible = true;
        }

        private void CargarHistorial()
        {
            gvHistorial.DataSource = new figura_rombo().ListarPorUsuario(UsuarioId);
            gvHistorial.DataBind();
        }

        private void Mostrar(string mensaje, bool ok)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMensaje.Visible = true;
        }
    }
}
