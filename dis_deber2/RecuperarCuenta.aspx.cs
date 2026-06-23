using System;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public partial class RecuperarCuenta : System.Web.UI.Page
    {
        private string CorreoRecuperacion
        {
            get { return ViewState["CorreoRec"] as string; }
            set { ViewState["CorreoRec"] = value; }
        }

        private int UsuIdRecuperacion
        {
            get { return ViewState["UsuIdRec"] != null ? (int)ViewState["UsuIdRec"] : 0; }
            set { ViewState["UsuIdRec"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SesionHelper.EstaAutenticado())
                Response.Redirect("~/Default.aspx");
        }

        protected void btnEnviarOtp_Click(object sender, EventArgs e)
        {
            var u = new usuario();
            var cedula = txtCedula.Text.Trim();
            var correo = txtCorreo.Text.Trim();

            var usuId = u.ValidarCedulaCorreo(cedula, correo);
            if (usuId == null)
            {
                Mostrar("Cédula y correo no coinciden con ninguna cuenta activa.", false);
                return;
            }

            var otp = u.GenerarOtp(correo, "RECUPERACION");
            if (string.IsNullOrEmpty(otp))
            {
                Mostrar("No se pudo generar el código.", false);
                return;
            }

            if (!EnvioCorreo.EnviarOtp(correo, otp, "RECUPERACION", out var smtpError))
            {
                CorreoRecuperacion = correo;
                UsuIdRecuperacion = usuId.Value;
                pnlPaso1.Visible = false;
                pnlPaso2.Visible = true;
                lblOtpDev.Text = otp;
                pnlOtpDev.Visible = EnvioCorreo.ModoDepuracion();

                if (EnvioCorreo.ModoDepuracion())
                {
                    Mostrar("No se pudo enviar el correo (" + smtpError + "). Use el codigo OTP mostrado abajo para continuar.", true);
                    return;
                }

                Mostrar("No se pudo enviar el correo. " + smtpError, false);
                pnlPaso1.Visible = true;
                pnlPaso2.Visible = false;
                return;
            }

            lblOtpDev.Visible = false;
            pnlOtpDev.Visible = false;

            CorreoRecuperacion = correo;
            UsuIdRecuperacion = usuId.Value;
            pnlPaso1.Visible = false;
            pnlPaso2.Visible = true;
            Mostrar("Código enviado a " + correo, true);
        }

        protected void btnValidarOtp_Click(object sender, EventArgs e)
        {
            var u = new usuario();
            var usuId = u.ValidarOtp(CorreoRecuperacion, txtOtp.Text.Trim(), "RECUPERACION");
            if (usuId == null)
            {
                Mostrar("Código OTP inválido o expirado.", false);
                return;
            }

            UsuIdRecuperacion = usuId.Value;
            pnlPaso2.Visible = false;
            pnlPaso3.Visible = true;
            Mostrar("Código validado. Ingrese su nueva contraseña.", true);
        }

        protected void btnGuardarClave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            var u = new usuario();
            if (u.CambiarContrasena(UsuIdRecuperacion, txtNuevaClave.Text))
            {
                Mostrar("Contraseña actualizada y cuenta desbloqueada. Ya puede iniciar sesión.", true);
                pnlPaso3.Visible = false;
            }
            else
            {
                Mostrar("No se pudo actualizar la contraseña.", false);
            }
        }

        private void Mostrar(string msg, bool ok)
        {
            lblMensaje.Text = msg;
            lblMensaje.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMensaje.Visible = true;
        }
    }
}
