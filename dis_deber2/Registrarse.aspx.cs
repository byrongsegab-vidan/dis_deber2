using System;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Registrarse : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionHelper.EstaAutenticado())
                Response.Redirect("~/Default.aspx");
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            if (!fuFoto.HasFile)
            {
                Mostrar("Debe subir una foto de perfil.", false);
                return;
            }

            if (!RutasImagen.EsImagenValida(fuFoto.PostedFile, out var errorImagen))
            {
                Mostrar(errorImagen, false);
                return;
            }

            var u = new usuario();
            var correo = txtCorreo.Text.Trim();

            if (u.CorreoExiste(correo))
            {
                Mostrar("El correo ya está registrado.", false);
                return;
            }

            u.usu_cedula = txtCedula.Text.Trim();
            u.usu_nombres = txtNombres.Text.Trim();
            u.usu_apellidos = txtApellidos.Text.Trim();
            u.usu_correo = correo;
            u.usu_nick = string.IsNullOrWhiteSpace(txtNick.Text) ? correo.Split('@')[0] : txtNick.Text.Trim();
            u.usu_contrasena = txtContrasena.Text;

            var nuevoId = u.RegistrarCliente();
            if (!nuevoId.HasValue)
            {
                Mostrar("No se pudo crear la cuenta.", false);
                return;
            }

            var ruta = RutasImagen.GuardarImagen(fuFoto.PostedFile, RutasImagen.CarpetaUsuarios, out errorImagen);
            if (ruta == null)
            {
                Mostrar(errorImagen ?? "No se pudo guardar la foto de perfil.", false);
                return;
            }

            if (!new imagen_usuario().GuardarPrincipal(nuevoId.Value, ruta))
            {
                Mostrar("Cuenta creada, pero no se pudo registrar la foto. Inicie sesión y actualice su perfil.", true);
                btnRegistrar.Enabled = false;
                return;
            }

            Mostrar("Cuenta creada correctamente. Ya puede iniciar sesión.", true);
            btnRegistrar.Enabled = false;
        }

        private void Mostrar(string msg, bool ok)
        {
            lblMensaje.Text = msg;
            lblMensaje.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMensaje.Visible = true;
        }
    }
}
