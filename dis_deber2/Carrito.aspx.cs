using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Carrito : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirCliente()) return;

            if (!IsPostBack)
                CargarCarrito();
        }

        protected void gvCarrito_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var proId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Quitar")
            {
                CarritoHelper.Eliminar(proId);
                CargarCarrito();
                return;
            }

            if (e.CommandName == "Actualizar")
            {
                var row = (GridViewRow)((Control)e.CommandSource).NamingContainer;
                var txtCantidad = (TextBox)row.FindControl("txtCantidad");
                var cantidad = int.TryParse(txtCantidad.Text, out var qty) ? qty : 0;
                CarritoHelper.ActualizarCantidad(proId, cantidad);
                CargarCarrito();
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            var items = CarritoHelper.Obtener();
            if (items.Count == 0)
            {
                CargarCarrito();
                return;
            }

            var copia = items.Select(i => new CarritoItem
            {
                ProId = i.ProId,
                Nombre = i.Nombre,
                Precio = i.Precio,
                Cantidad = i.Cantidad
            }).ToList();

            var total = copia.Sum(i => i.Subtotal);

            lblResumenCliente.Text = Session["usu_nombre"]?.ToString();
            lblResumenCorreo.Text = Session["usu_correo"]?.ToString();
            lblResumenFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            lblResumenTotal.Text = total.ToString("C");

            gvResumen.DataSource = copia;
            gvResumen.DataBind();

            pnlCarrito.Visible = false;
            pnlResumenCompra.Visible = true;

            CarritoHelper.Limpiar();
        }

        private void CargarCarrito()
        {
            var items = CarritoHelper.Obtener();
            var vacio = items.Count == 0;

            pnlCarritoVacio.Visible = vacio;
            pnlCarrito.Visible = !vacio;
            pnlResumenCompra.Visible = false;

            if (vacio)
                return;

            gvCarrito.DataSource = items;
            gvCarrito.DataBind();
            lblTotal.Text = CarritoHelper.Total().ToString("C");
        }
    }
}
