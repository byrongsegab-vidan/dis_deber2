using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Proveedores : Page
    {





        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirAdmin()) return;
            if (!IsPostBack)
            {
                CargarGrid();
                CargarGridCategorias();
            }
        }






        private void CargarGrid()
        {
            gvProveedores.DataSource = new prov().Listar(string.Empty, chkInactivos.Checked, chkProvOrdenAlfabetico.Checked);
            gvProveedores.DataBind();
        }

        private void CargarGridCategorias()
        {
            gvCategorias.DataSource = new categoria().Listar(txtCatFiltroNombre.Text.Trim(), incluirInactivos: true, ordenAlfabetico: chkCatOrdenAlfabetico.Checked);
            gvCategorias.DataBind();
        }

        protected void chkProvOrdenAlfabetico_CheckedChanged(object sender, EventArgs e) => CargarGrid();

        protected void chkCatOrdenAlfabetico_CheckedChanged(object sender, EventArgs e) => CargarGridCategorias();

        protected void chkInactivos_CheckedChanged(object sender, EventArgs e) => CargarGrid();

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            var p = new prov
            {
                prov_nombre = txtNombre.Text.Trim(),
                prov_ruc = txtRuc.Text.Trim(),
                prov_telefono = txtTelefono.Text.Trim(),
                prov_correo = txtCorreo.Text.Trim(),
                prov_direccion = txtDireccion.Text.Trim()
            };

            bool ok;
            if (string.IsNullOrEmpty(hfProvId.Value))
                ok = p.Insertar();
            else
            {
                p.prov_id = int.Parse(hfProvId.Value);
                ok = p.Actualizar();
            }

            MostrarMensajeProv(ok ? "Proveedor guardado." : "Error al guardar.", ok);
            Limpiar();
            CargarGrid();
        }

        protected void btnRestaurar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfProvId.Value))
            {
                MostrarMensajeProv("Seleccione un proveedor para restaurar sus productos hijos.", false);
                return;
            }

            var p = new prov { prov_id = int.Parse(hfProvId.Value) };
            p.RestaurarProductosHijos();
            MostrarMensajeProv("Productos restaurados al proveedor seleccionado.", true);
        }

        protected void gvProveedores_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProveedores.PageIndex = e.NewPageIndex;
            CargarGrid();
        }

        protected void gvProveedores_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var p = new prov();
            if (e.CommandName == "Editar")
            {
                var dt = p.Listar(string.Empty, true);
                var rows = dt.Select("prov_id = " + id);
                if (rows.Length == 0) return;
                var row = rows[0];
                hfProvId.Value = id.ToString();
                txtNombre.Text = row["prov_nombre"].ToString();
                txtRuc.Text = row["prov_ruc"].ToString();
                txtTelefono.Text = row["prov_telefono"].ToString();
                txtCorreo.Text = row["prov_correo"].ToString();
                txtDireccion.Text = row["prov_direccion"].ToString();
            }
            else if (e.CommandName == "BorradoLogico")
            {
                p.prov_id = id;
                p.EliminarLogico();
                CargarGrid();
            }
            else if (e.CommandName == "BorradoFisico")
            {
                p.prov_id = id;
                p.EliminarFisico();
                CargarGrid();
            }
            else if (e.CommandName == "Reactivar")
            {
                p.prov_id = id;
                p.Reactivar();
                CargarGrid();
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e) => Limpiar();

        private void Limpiar()
        {
            hfProvId.Value = string.Empty;
            txtNombre.Text = txtRuc.Text = txtTelefono.Text = txtCorreo.Text = txtDireccion.Text = string.Empty;
        }

        protected void btnCatGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCatNombre.Text))
            {
                MostrarMensajeCat("Ingrese el nombre de la categoría.", false);
                return;
            }

            var cat = new categoria { cat_nombre = txtCatNombre.Text.Trim() };
            var esEdicion = !string.IsNullOrEmpty(hfCatId.Value);
            bool ok;

            if (esEdicion)
            {
                cat.cat_id = int.Parse(hfCatId.Value);
                ok = cat.Actualizar();
            }
            else
            {
                ok = cat.Insertar();
            }

            if (ok)
            {
                MostrarMensajeCat(esEdicion ? "Categoría actualizada correctamente." : "Categoría creada correctamente.", true);
                LimpiarCategoria();
                CargarGridCategorias();
            }
            else
            {
                MostrarMensajeCat("No se pudo guardar la categoría.", false);
            }
        }

        protected void btnCatLimpiar_Click(object sender, EventArgs e) => LimpiarCategoria();

        protected void btnCatFiltrar_Click(object sender, EventArgs e) => CargarGridCategorias();

        protected void btnCatLimpiarFiltro_Click(object sender, EventArgs e)
        {
            txtCatFiltroNombre.Text = string.Empty;
            chkCatOrdenAlfabetico.Checked = false;
            CargarGridCategorias();
        }

        protected void gvCategorias_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCategorias.PageIndex = e.NewPageIndex;
            CargarGridCategorias();
        }

        protected void gvCategorias_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var cat = new categoria();

            if (e.CommandName == "EditarCat")
            {
                var row = cat.ObtenerPorId(id);
                if (row == null) return;

                hfCatId.Value = id.ToString();
                lblCatFormTitulo.Text = "Editar categoría #" + id;
                txtCatNombre.Text = row["cat_nombre"].ToString();
                return;
            }

            if (e.CommandName == "BorradoFisicoCat")
            {
                cat.cat_id = id;
                cat.EliminarFisico();
                LimpiarCategoria();
                CargarGridCategorias();
                MostrarMensajeCat("Categoría eliminada físicamente.", true);
            }
        }

        protected void gvCategorias_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var ddl = (DropDownList)e.Row.FindControl("ddlCatEstado");
            if (ddl == null) return;

            var estado = DataBinder.Eval(e.Row.DataItem, "cat_estado")?.ToString()?.Trim() ?? "A";
            ddl.SelectedValue = estado == "I" ? "I" : "A";
        }

        protected void ddlCatEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var row = (GridViewRow)ddl.NamingContainer;
            var keys = gvCategorias.DataKeys[row.RowIndex];
            var id = Convert.ToInt32(keys.Values["cat_id"]);
            var estadoAnterior = keys.Values["cat_estado"]?.ToString()?.Trim() ?? "A";

            if (ddl.SelectedValue == estadoAnterior)
                return;

            var cat = new categoria { cat_id = id };
            cat.CambiarEstado(ddl.SelectedValue);
            CargarGridCategorias();
        }

        private void LimpiarCategoria()
        {
            hfCatId.Value = string.Empty;
            lblCatFormTitulo.Text = "Nueva categoría";
            txtCatNombre.Text = string.Empty;
        }

        private void MostrarMensajeProv(string msg, bool ok)
        {
            lblMsg.Text = msg;
            lblMsg.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMsg.Visible = true;
        }

        private void MostrarMensajeCat(string msg, bool ok)
        {
            lblMsgCat.Text = msg;
            lblMsgCat.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMsgCat.Visible = true;
        }
    }
}
