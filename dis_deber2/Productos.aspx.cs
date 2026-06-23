using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Productos : Page
    {
        private const int TamanoPagina = 5;
        private const int MinimoImagenes = 3;
        private const int MaximoImagenes = 11;

        private int PaginaActual
        {
            get { return ViewState["Pagina"] != null ? (int)ViewState["Pagina"] : 1; }
            set { ViewState["Pagina"] = value; }
        }





        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirAdmin()) return;
            if (!IsPostBack)
            {
                CargarCombos();
                CargarGrid();
            }





            if (IsPostBack)
                pnlVisualizador.Visible = false;
        }

        protected string RutasImagenUrl(object ruta) => RutasImagen.ResolverUrl(ruta?.ToString());

        private void CargarCombos()
        {
            var cats = new categoria().ListarActivas();
            ddlCategoria.DataSource = cats;
            ddlCategoria.DataTextField = "cat_nombre";
            ddlCategoria.DataValueField = "cat_id";
            ddlCategoria.DataBind();
            ddlCategoria.Items.Insert(0, new ListItem("-- Categoría --", ""));

            ddlFiltroCategoria.DataSource = cats;
            ddlFiltroCategoria.DataTextField = "cat_nombre";
            ddlFiltroCategoria.DataValueField = "cat_id";
            ddlFiltroCategoria.DataBind();
            ddlFiltroCategoria.Items.Insert(0, new ListItem("-- Todas --", ""));

            var provs = new prov().ListarActivos();
            ddlProveedor.DataSource = provs;
            ddlProveedor.DataTextField = "prov_nombre";
            ddlProveedor.DataValueField = "prov_id";
            ddlProveedor.DataBind();
            ddlProveedor.Items.Insert(0, new ListItem("-- Proveedor --", ""));

            ddlFiltroProveedor.DataSource = provs;
            ddlFiltroProveedor.DataTextField = "prov_nombre";
            ddlFiltroProveedor.DataValueField = "prov_id";
            ddlFiltroProveedor.DataBind();
            ddlFiltroProveedor.Items.Insert(0, new ListItem("-- Todos --", ""));
        }

        private void CargarGrid()
        {
            int? catId = string.IsNullOrEmpty(ddlFiltroCategoria.SelectedValue)
                ? (int?)null
                : int.Parse(ddlFiltroCategoria.SelectedValue);
            int? provId = string.IsNullOrEmpty(ddlFiltroProveedor.SelectedValue)
                ? (int?)null
                : int.Parse(ddlFiltroProveedor.SelectedValue);

            if (!TryParsePrecio(txtFiltroPrecioMin.Text, out var precioMin, out var errorMin))
            {
                MostrarMensaje(errorMin, false);
                return;
            }

            if (!TryParsePrecio(txtFiltroPrecioMax.Text, out var precioMax, out var errorMax))
            {
                MostrarMensaje(errorMax, false);
                return;
            }

            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            {
                MostrarMensaje("El precio mínimo no puede ser mayor que el precio máximo.", false);
                return;
            }

            var producto = new pro();
            var dt = producto.ListarPaginado(
                txtFiltroNombre.Text.Trim(),
                catId,
                provId,
                precioMin,
                precioMax,
                PaginaActual,
                TamanoPagina,
                out var total,
                ordenAlfabetico: chkOrdenAlfabetico.Checked);

            gvProductos.DataSource = dt;
            gvProductos.DataBind();

            var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
            lblPaginacion.Text = "Página " + PaginaActual + " de " + Math.Max(totalPaginas, 1) + " (" + total + " productos)";
            btnPaginaAnterior.Enabled = PaginaActual > 1;
            btnPaginaSiguiente.Enabled = PaginaActual < totalPaginas;

            MostrarResumenFiltro(catId, provId, precioMin, precioMax, total);
        }

        private void MostrarResumenFiltro(int? catId, int? provId, decimal? precioMin, decimal? precioMax, int total)
        {
            var hayFiltro = !string.IsNullOrWhiteSpace(txtFiltroNombre.Text)
                || catId.HasValue
                || provId.HasValue
                || precioMin.HasValue
                || precioMax.HasValue;

            if (!hayFiltro)
            {
                lblFiltroResumen.Visible = false;
                return;
            }

            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(txtFiltroNombre.Text))
                partes.Add("Nombre: \"" + txtFiltroNombre.Text.Trim() + "\"");
            if (catId.HasValue)
                partes.Add("Categoría: " + ddlFiltroCategoria.SelectedItem.Text);
            if (provId.HasValue)
                partes.Add("Proveedor: " + ddlFiltroProveedor.SelectedItem.Text);
            if (precioMin.HasValue || precioMax.HasValue)
            {
                if (precioMin.HasValue && precioMax.HasValue)
                    partes.Add("Precio: $" + precioMin.Value.ToString("N2") + " — $" + precioMax.Value.ToString("N2"));
                else if (precioMin.HasValue)
                    partes.Add("Precio desde: $" + precioMin.Value.ToString("N2"));
                else
                    partes.Add("Precio hasta: $" + precioMax.Value.ToString("N2"));
            }

            var sb = new StringBuilder();
            sb.Append("<strong>Filtros activos:</strong> ");
            sb.Append(string.Join(" · ", partes));
            sb.Append(" — <strong>" + total + " producto(s)</strong> encontrado(s).");

            if (total == 0)
            {
                sb.Append(" Ningún registro cumple <em>todos</em> esos criterios a la vez.");
                if (provId.HasValue && (precioMin.HasValue || precioMax.HasValue))
                    sb.Append(" Pruebe ampliar el rango de precio o quitar el proveedor.");
            }

            lblFiltroResumen.Text = sb.ToString();
            lblFiltroResumen.Visible = true;
        }

        private static bool TryParsePrecio(string texto, out decimal? valor, out string error)
        {
            valor = null;
            error = null;

            if (string.IsNullOrWhiteSpace(texto))
                return true;

            var normalizado = texto.Trim().Replace(',', '.');
            if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero))
            {
                valor = numero;
                return true;
            }

            error = "El valor de precio \"" + texto.Trim() + "\" no es válido. Use números como 50 o 35.50";
            return false;
        }

        protected void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (PaginaActual > 1) PaginaActual--;
            CargarGrid();
        }

        protected void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            PaginaActual++;
            CargarGrid();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            PaginaActual = 1;
            CargarGrid();
        }

        protected void btnLimpiarFiltro_Click(object sender, EventArgs e)
        {
            txtFiltroNombre.Text = string.Empty;
            txtFiltroPrecioMin.Text = string.Empty;
            txtFiltroPrecioMax.Text = string.Empty;
            ddlFiltroCategoria.SelectedIndex = 0;
            ddlFiltroProveedor.SelectedIndex = 0;
            chkOrdenAlfabetico.Checked = false;
            PaginaActual = 1;
            lblFiltroResumen.Visible = false;
            CargarGrid();
        }

        protected void chkOrdenAlfabetico_CheckedChanged(object sender, EventArgs e)
        {
            PaginaActual = 1;
            CargarGrid();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtPrecio.Text) || string.IsNullOrWhiteSpace(txtStock.Text))
            {
                MostrarMensaje("Complete nombre, precio y stock.", false);
                return;
            }

            var esEdicion = !string.IsNullOrEmpty(hfProId.Value);
            var imgSvc = new imagen_producto();
            var rutasNuevas = new List<string>();

            if (esEdicion)
            {
                rutasNuevas.AddRange(GuardarArchivosSubidos(ObtenerUploadsExtra()));
                var proId = int.Parse(hfProId.Value);
                var totalActual = imgSvc.ContarActivas(proId);
                var totalImgs = totalActual + rutasNuevas.Count;
                if (totalImgs > MaximoImagenes)
                {
                    MostrarMensaje("El producto no puede tener más de " + MaximoImagenes + " fotos.", false);
                    return;
                }
                if (totalImgs < MinimoImagenes)
                {
                    MostrarMensaje("El producto debe tener al menos " + MinimoImagenes + " fotos activas.", false);
                    return;
                }
            }
            else
            {
                rutasNuevas.AddRange(GuardarArchivosSubidos(ObtenerUploadsNuevo()));
                if (rutasNuevas.Count > MaximoImagenes)
                {
                    MostrarMensaje("Puede subir como máximo " + MaximoImagenes + " fotos.", false);
                    return;
                }
                if (rutasNuevas.Count < MinimoImagenes)
                {
                    MostrarMensaje("Debe subir al menos " + MinimoImagenes + " imágenes válidas (JPG, PNG, GIF o WEBP).", false);
                    return;
                }
            }

            var p = new pro
            {
                pro_nombre = txtNombre.Text.Trim(),
                pro_descripcion = txtDescripcion.Text.Trim(),
                pro_precio = decimal.Parse(txtPrecio.Text),
                pro_stock = int.Parse(txtStock.Text),
                cat_id = string.IsNullOrEmpty(ddlCategoria.SelectedValue) ? (int?)null : int.Parse(ddlCategoria.SelectedValue),
                prov_id = string.IsNullOrEmpty(ddlProveedor.SelectedValue) ? (int?)null : int.Parse(ddlProveedor.SelectedValue),
                pro_imagen_ruta = rutasNuevas.Count > 0 ? rutasNuevas[0] : null
            };

            var ok = false;

            if (!esEdicion)
            {
                ok = p.Insertar();
                if (ok)
                {
                    for (var i = 0; i < rutasNuevas.Count; i++)
                    {
                        new imagen_producto
                        {
                            pro_id = p.pro_id,
                            ipro_ruta = rutasNuevas[i],
                            ipro_es_principal = i == 0
                        }.Insertar();
                    }
                    p.ActualizarImagenPrincipal(p.pro_id);
                    MostrarMensaje("Producto creado correctamente.", true);
                }
                else
                {
                    MostrarMensaje("No se pudo crear el producto.", false);
                }
            }
            else
            {
                p.pro_id = int.Parse(hfProId.Value);
                if (rutasNuevas.Count == 0)
                {
                    var row = p.ObtenerPorId(p.pro_id);
                    p.pro_imagen_ruta = row?["pro_imagen_ruta"]?.ToString();
                }
                else
                {
                    p.pro_imagen_ruta = rutasNuevas[0];
                }

                ok = p.Actualizar();

                var sinImagenesPrevias = imgSvc.ContarActivas(p.pro_id) == 0;
                for (var i = 0; i < rutasNuevas.Count; i++)
                {
                    new imagen_producto
                    {
                        pro_id = p.pro_id,
                        ipro_ruta = rutasNuevas[i],
                        ipro_es_principal = sinImagenesPrevias && i == 0
                    }.Insertar();
                }

                p.ActualizarImagenPrincipal(p.pro_id);

                if (ok)
                    MostrarMensaje("Producto actualizado correctamente.", true);
            }

            if (!ok && esEdicion)
                MostrarMensaje("No se pudo guardar.", false);

            if (!esEdicion)
                LimpiarFormulario();
            else if (ok)
            {
                CargarGaleria(p.pro_id);
            }

            CargarGrid();
        }

        protected void gvProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var id = int.Parse(e.CommandArgument.ToString());
            var p = new pro();

            if (e.CommandName == "Visualizar")
            {
                MostrarCarruselProducto(id);
                return;
            }

            if (e.CommandName == "Editar")
            {
                var row = p.ObtenerPorId(id);
                if (row == null) return;

                hfProId.Value = id.ToString();
                lblFormTitulo.Text = "Editar producto #" + id;
                pnlImagenesNuevo.Visible = false;
                pnlImagenesEditar.Visible = true;

                txtNombre.Text = row["pro_nombre"].ToString();
                txtDescripcion.Text = row["pro_descripcion"]?.ToString() ?? string.Empty;
                txtPrecio.Text = row["pro_precio"].ToString();
                txtStock.Text = row["pro_stock"].ToString();
                ddlCategoria.SelectedIndex = 0;
                ddlProveedor.SelectedIndex = 0;
                if (row["cat_id"] != DBNull.Value && ddlCategoria.Items.FindByValue(row["cat_id"].ToString()) != null)
                    ddlCategoria.SelectedValue = row["cat_id"].ToString();
                if (row["prov_id"] != DBNull.Value && ddlProveedor.Items.FindByValue(row["prov_id"].ToString()) != null)
                    ddlProveedor.SelectedValue = row["prov_id"].ToString();

                CargarGaleria(id);
                return;
            }

            if (e.CommandName == "BorradoFisico")
            {
                p.pro_id = id;
                p.EliminarFisico();
                CargarGrid();
            }
        }

        protected void rptImagenes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "QuitarImagen" || string.IsNullOrEmpty(hfProId.Value))
                return;

            var iproId = int.Parse(e.CommandArgument.ToString());
            var proId = int.Parse(hfProId.Value);
            var imgSvc = new imagen_producto();

            if (imgSvc.ContarActivas(proId) <= MinimoImagenes)
            {
                MostrarMensaje("No puede quedar con menos de " + MinimoImagenes + " fotos.", false);
                return;
            }

            imgSvc.EliminarLogico(iproId);
            new pro().ActualizarImagenPrincipal(proId);
            CargarGaleria(proId);
            MostrarMensaje("Foto eliminada.", true);
        }

        protected void gvProductos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var ddl = (DropDownList)e.Row.FindControl("ddlEstado");
            if (ddl == null) return;

            var estado = DataBinder.Eval(e.Row.DataItem, "pro_estado")?.ToString()?.Trim() ?? "A";
            ddl.SelectedValue = estado == "I" ? "I" : "A";
        }

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var row = (GridViewRow)ddl.NamingContainer;
            var keys = gvProductos.DataKeys[row.RowIndex];
            var id = Convert.ToInt32(keys.Values["pro_id"]);
            var estadoAnterior = keys.Values["pro_estado"]?.ToString()?.Trim() ?? "A";

            if (ddl.SelectedValue == estadoAnterior)
                return;

            var p = new pro { pro_id = id };
            p.CambiarEstado(ddl.SelectedValue);
            CargarGrid();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e) => LimpiarFormulario();

        protected void btnCerrarVisualizador_Click(object sender, EventArgs e)
        {
            pnlVisualizador.Visible = false;
        }

        private void CargarGaleria(int proId)
        {
            rptImagenes.DataSource = new imagen_producto().ListarPorProducto(proId);
            rptImagenes.DataBind();

            var actuales = new imagen_producto().ContarActivas(proId);
            var puedeAgregar = Math.Max(0, MaximoImagenes - actuales);
            lblLimiteFotosEditar.Text = "Tiene " + actuales + " foto(s). Puede agregar hasta " + puedeAgregar + " más.";
        }

        private void MostrarCarruselProducto(int productoId)
        {
            var row = new pro().ObtenerPorId(productoId);
            if (row == null)
            {
                MostrarMensaje("Producto no encontrado.", false);
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

        private FileUpload[] ObtenerUploadsNuevo()
        {
            return new[]
            {
                fuImg1, fuImg2, fuImg3, fuImg4, fuImg5, fuImg6,
                fuImg7, fuImg8, fuImg9, fuImg10, fuImg11
            };
        }

        private FileUpload[] ObtenerUploadsExtra()
        {
            return new[]
            {
                fuExtra1, fuExtra2, fuExtra3, fuExtra4,
                fuExtra5, fuExtra6, fuExtra7, fuExtra8
            };
        }

        private List<string> GuardarArchivosSubidos(params FileUpload[] uploads)
        {
            var rutas = new List<string>();
            foreach (var fu in uploads)
            {
                if (fu == null || !fu.HasFile)
                    continue;

                if (!RutasImagen.EsImagenValida(fu.PostedFile, out _))
                    continue;

                var ruta = RutasImagen.GuardarImagen(fu.PostedFile, RutasImagen.CarpetaProductos, out _);
                if (ruta != null)
                    rutas.Add(ruta);
            }

            return rutas;
        }

        private void LimpiarFormulario()
        {
            hfProId.Value = string.Empty;
            lblFormTitulo.Text = "Nuevo producto";
            pnlImagenesNuevo.Visible = true;
            pnlImagenesEditar.Visible = false;
            txtNombre.Text = txtDescripcion.Text = txtPrecio.Text = txtStock.Text = string.Empty;
            ddlCategoria.SelectedIndex = 0;
            ddlProveedor.SelectedIndex = 0;
            rptImagenes.DataSource = null;
            rptImagenes.DataBind();
        }

        private void MostrarMensaje(string msg, bool ok)
        {
            lblMsg.Text = msg;
            lblMsg.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMsg.Visible = true;
        }
    }
}
