<%@ Page Title="Iniciar sesion" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="dis_deber2.Login" ResponseEncoding="utf-8" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Login - dis_deber2</title>
    <link href="~/Content/Auth.css?v=4" rel="stylesheet" type="text/css" runat="server" />
    <style>
        /* Acento naranja — garantizado aunque el navegador cachee CSS antiguo */
        #form1 input[type="submit"].auth-btn-primary,
        #form1 .auth-btn-primary {
            background: linear-gradient(135deg, #FF6B00 0%, #FF9500 55%, #FFB347 100%) !important;
            background-color: #FF6B00 !important;
            border: none !important;
            color: #ffffff !important;
            border-radius: 12px !important;
            box-shadow: 0 8px 24px rgba(255, 107, 0, 0.4) !important;
        }
        #form1 input[type="submit"].auth-btn-primary:hover,
        #form1 .auth-btn-primary:hover {
            background: linear-gradient(135deg, #FF7A1A 0%, #FFA033 55%, #FFC266 100%) !important;
            box-shadow: 0 12px 28px rgba(255, 107, 0, 0.5) !important;
        }
        #form1 .auth-link-accent,
        #form1 .auth-footer a {
            color: #FF6B00 !important;
            font-weight: 700 !important;
        }
        #form1 .auth-link-accent:hover,
        #form1 .auth-footer a:hover {
            color: #E85D00 !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" class="auth-page">
        <div class="auth-card">
            <div class="auth-form-side">
                <h1 class="auth-title">Bienvenido de nuevo &#128075;</h1>
                <p class="auth-subtitle">Hoy es un gran d&iacute;a para empezar. Inicia sesi&oacute;n y gestiona tus productos con energ&iacute;a.</p>

                <asp:Label ID="lblMensaje" runat="server" CssClass="auth-alert auth-alert-error" Visible="false" />

                <div class="auth-field">
                    <label for="txtCorreo">Correo electr&oacute;nico</label>
                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="auth-input" TextMode="Email" placeholder="ejemplo@tucorreo.com" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCorreo" ErrorMessage="Ingresa tu correo" CssClass="auth-validator" Display="Dynamic" />
                </div>

                <div class="auth-field">
                    <label for="txtContrasena">Contrase&ntilde;a</label>
                    <asp:TextBox ID="txtContrasena" runat="server" CssClass="auth-input" TextMode="Password" placeholder="Tu clave segura — &iexcl;t&uacute; puedes!" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtContrasena" ErrorMessage="Ingresa tu contrase&ntilde;a" CssClass="auth-validator" Display="Dynamic" />
                </div>

                <div class="auth-forgot-wrap">
                    <asp:HyperLink ID="lnkRecuperar" runat="server" NavigateUrl="~/RecuperarCuenta.aspx" CssClass="auth-forgot">&iquest;Olvidaste tu contrase&ntilde;a?</asp:HyperLink>
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Iniciar sesi&oacute;n" CssClass="auth-btn-primary" OnClick="btnLogin_Click" />

                <p class="auth-footer">
                    &iquest;No tienes cuenta?
                    <asp:HyperLink ID="lnkRegistrarse" runat="server" NavigateUrl="~/Registrarse.aspx" Text="Reg&iacute;strate" CssClass="auth-link-accent" />
                </p>

                <p class="auth-copy">&copy; <%: DateTime.Now.Year %> DIS_DEBER2 &mdash; TODOS LOS DERECHOS RESERVADOS</p>
            </div>

            <div class="auth-image-side" aria-hidden="true">
                <img src="~/Content/Imagenes/login-fondo.png" runat="server" id="imgLoginFondo" alt="" />
            </div>
        </div>
    </form>
</body>
</html>
