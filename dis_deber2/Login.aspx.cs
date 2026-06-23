using System;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionHelper.EstaAutenticado())
            {
                if (SesionHelper.EsCliente())
                    SesionHelper.Redirigir("~/Catalogo.aspx");
                else
                    SesionHelper.Redirigir("~/Default.aspx");
                return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var correo = txtCorreo.Text.Trim();
            var user = new usuario();
            var estado = user.ObtenerEstadoLogin(correo);

            if (!estado.Existe)
            {
                MostrarError("El correo no está registrado. Verifique o cree una cuenta.");
                return;
            }

            if (estado.Bloqueada)
            {
                MostrarError("Cuenta bloqueada por intentos fallidos. Use el botón Recuperar cuenta para desbloquearla.");
                return;
            }

            var row = user.ValidarLogin(correo, txtContrasena.Text);
            if (row == null)
            {
                user.RegistrarIntentoFallido(correo, out var intentos, out var bloqueada);
                if (bloqueada)
                {
                    MostrarError("Cuenta bloqueada: 3 intentos fallidos. Use Recuperar cuenta para restablecer el acceso.");
                    return;
                }

                var restantes = 3 - intentos;
                MostrarError("Contraseña incorrecta. Le quedan " + restantes + " intento(s) antes del bloqueo.");
                return;
            }

            SesionHelper.Iniciar(
                Convert.ToInt32(row["usu_id"]),
                row["usu_nombres"] + " " + row["usu_apellidos"],
                row["usu_correo"].ToString(),
                Convert.ToInt32(row["tusu_id"]),
                row["tusu_nombre"].ToString());

            if (SesionHelper.EsCliente())
                SesionHelper.Redirigir("~/Catalogo.aspx");
            else
                SesionHelper.Redirigir("~/Default.aspx");
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = "auth-alert auth-alert-error";
            lblMensaje.Visible = true;
        }
    }
}
