using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace dis_deber2.CapaPresentacion
{
    public static class EnvioCorreo
    {
        static EnvioCorreo()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.Expect100Continue = true;
        }

        public static string CorreoRemitente => ConfigurationManager.AppSettings["CorreoRemitente"];
        public static string CorreoAdministrador => ConfigurationManager.AppSettings["CorreoAdministrador"];
        public static string CorreoUsuario => ConfigurationManager.AppSettings["CorreoUsuario"];

        public static bool EnviarOtp(string destinatario, string otp, string tipo)
        {
            string error;
            return EnviarOtp(destinatario, otp, tipo, out error);
        }

        public static bool EnviarOtp(string destinatario, string otp, string tipo, out string error)
        {
            var asunto = tipo == "RECUPERACION"
                ? "Recuperacion de cuenta - dis_deber2"
                : "Codigo OTP - dis_deber2";

            var cuerpo = new StringBuilder()
                .Append("<h3>dis_deber2</h3>")
                .Append("<p>Su codigo de verificacion es:</p>")
                .Append("<h2 style='letter-spacing:4px'>").Append(otp).Append("</h2>")
                .Append("<p>Tipo: ").Append(tipo).Append("</p>")
                .Append("<p>Valido por 10 minutos.</p>")
                .Append("<p><small>Enviado desde ").Append(CorreoRemitente).Append("</small></p>")
                .ToString();

            return Enviar(destinatario, asunto, cuerpo, out error);
        }

        public static bool Enviar(string destinatario, string asunto, string cuerpoHtml, out string error)
        {
            error = null;
            var host = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            var usuario = ConfigurationManager.AppSettings["SmtpUsuario"];
            var clave = (ConfigurationManager.AppSettings["SmtpClave"] ?? string.Empty).Replace(" ", "").Trim();
            var nombre = ConfigurationManager.AppSettings["NombreRemitente"] ?? "dis_deber2";

            if (string.IsNullOrEmpty(clave) || clave.StartsWith("TU_", StringComparison.OrdinalIgnoreCase))
            {
                error = "Falta configurar SmtpClave en Web.config.";
                return false;
            }

            if (!PuedeConectar(host, 587) && !PuedeConectar(host, 465))
            {
                error = "Su red o antivirus bloquea la conexion a Gmail (puertos 587 y 465). "
                    + "Pruebe otro WiFi, desactive VPN o use el codigo OTP en modo desarrollo.";
                return false;
            }

            var puertos = new[] { 587, 465 };
            var configurado = ConfigurationManager.AppSettings["SmtpPort"];
            if (int.TryParse(configurado, out var puertoPreferido) && puertoPreferido > 0)
                puertos = new[] { puertoPreferido, puertoPreferido == 587 ? 465 : 587 };

            Exception ultimoError = null;
            foreach (var puerto in puertos)
            {
                if (!PuedeConectar(host, puerto))
                    continue;

                try
                {
                    using (var msg = CrearMensaje(destinatario, asunto, cuerpoHtml, nombre))
                    using (var smtp = new SmtpClient(host, puerto))
                    {
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(usuario, clave);
                        smtp.Timeout = 60000;
                        smtp.Send(msg);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    ultimoError = ex;
                }
            }

            error = ultimoError != null
                ? (ultimoError.InnerException?.Message ?? ultimoError.Message)
                : "No se pudo conectar al servidor SMTP.";
            return false;
        }

        private static MailMessage CrearMensaje(string destinatario, string asunto, string cuerpoHtml, string nombre)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(CorreoRemitente, nombre),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            msg.To.Add(destinatario);
            return msg;
        }

        private static bool PuedeConectar(string host, int puerto)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, puerto, null, null);
                    var ok = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                    if (!ok) return false;
                    client.EndConnect(result);
                    return client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ModoDepuracion()
        {
            var ctx = HttpContext.Current;
            return ctx != null && ctx.IsDebuggingEnabled;
        }
    }
}
