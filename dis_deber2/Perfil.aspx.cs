using System;
using System.Web.UI;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Perfil : Page
    {
        private int UsuarioId => Convert.ToInt32(Session["usu_id"]);





        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.RequerirLogin()) return;

            if (!IsPostBack)
                CargarPerfil();
        }







        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            if (fuFoto.HasFile && !RutasImagen.EsImagenValida(fuFoto.PostedFile, out var errorImagen))
            {
                Mostrar(errorImagen, false);
                return;
            }

            var u = new usuario
            {
                usu_id = UsuarioId,
                usu_cedula = txtCedula.Text.Trim(),
                usu_nombres = txtNombres.Text.Trim(),
                usu_apellidos = txtApellidos.Text.Trim(),
                usu_nick = txtNick.Text.Trim(),
                usu_direccion = txtDireccion.Text.Trim(),
                usu_celular = txtCelular.Text.Trim(),
                usu_fecha_cumple = string.IsNullOrWhiteSpace(txtCumple.Text)
                    ? (DateTime?)null
                    : DateTime.Parse(txtCumple.Text)
            };

            if (!u.ActualizarPerfil())
            {
                Mostrar("No se pudieron guardar los datos.", false);
                return;
            }

            if (fuFoto.HasFile)
            {
                var ruta = RutasImagen.GuardarImagen(fuFoto.PostedFile, RutasImagen.CarpetaUsuarios, out errorImagen);
                if (ruta == null)
                {
                    Mostrar("Datos guardados, pero la foto no es válida: " + errorImagen, false);
                    return;
                }

                if (!new imagen_usuario().GuardarPrincipal(UsuarioId, ruta))
                {
                    Mostrar("Datos guardados, pero no se pudo actualizar la foto.", false);
                    return;
                }

                imgPerfil.ImageUrl = RutasImagen.ResolverUrl(ruta);
            }

            Session["usu_nombre"] = u.usu_nombres + " " + u.usu_apellidos;
            Mostrar("Perfil actualizado correctamente.", true);
        }

        private void CargarPerfil()
        {
            var row = new usuario().ObtenerPorId(UsuarioId);
            if (row == null)
            {
                Mostrar("No se encontró su perfil.", false);
                btnGuardar.Enabled = false;
                return;
            }

            txtCedula.Text = row["usu_cedula"]?.ToString();
            txtNombres.Text = row["usu_nombres"]?.ToString();
            txtApellidos.Text = row["usu_apellidos"]?.ToString();
            txtNick.Text = row["usu_nick"]?.ToString();
            txtCorreo.Text = row["usu_correo"]?.ToString();
            txtRol.Text = row["tusu_nombre"]?.ToString();
            txtCelular.Text = row["usu_celular"]?.ToString();
            txtDireccion.Text = row["usu_direccion"]?.ToString();

            if (row["usu_fecha_cumple"] != DBNull.Value)
                txtCumple.Text = Convert.ToDateTime(row["usu_fecha_cumple"]).ToString("yyyy-MM-dd");

            var rutaFoto = new imagen_usuario().ObtenerRutaPrincipal(UsuarioId);
            imgPerfil.ImageUrl = RutasImagen.ResolverUrl(rutaFoto);
        }

        private void Mostrar(string mensaje, bool exito)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = exito ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMensaje.Visible = true;
        }
    }
}
