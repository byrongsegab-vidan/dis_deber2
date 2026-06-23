<%@ Page Title="Recuperar cuenta" Language="C#" AutoEventWireup="true" CodeBehind="RecuperarCuenta.aspx.cs" Inherits="dis_deber2.RecuperarCuenta" ResponseEncoding="utf-8" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <title>Recuperar cuenta - dis_deber2</title>
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server" class="container py-5">
        <div class="row justify-content-center">
            <div class="col-md-5">
                <div class="card shadow">
                    <div class="card-body p-4">
                        <h3 class="mb-4 text-center">Recuperar cuenta</h3>
                        <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="alert d-block" />

                        <asp:Panel ID="pnlPaso1" runat="server">
                            <p class="text-muted small">Ingrese c&eacute;dula y correo registrados. Le enviaremos un c&oacute;digo OTP.</p>
                            <div class="mb-3">
                                <label class="form-label">C&eacute;dula</label>
                                <asp:TextBox ID="txtCedula" runat="server" CssClass="form-control" MaxLength="10" />
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Correo</label>
                                <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" TextMode="Email" />
                            </div>
                            <asp:Button ID="btnEnviarOtp" runat="server" Text="Enviar c&oacute;digo" CssClass="btn btn-primary w-100" OnClick="btnEnviarOtp_Click" />
                        </asp:Panel>

                        <asp:Panel ID="pnlPaso2" runat="server" Visible="false">
                            <asp:Panel ID="pnlOtpDev" runat="server" Visible="false" CssClass="alert alert-warning mb-3">
                                <strong>Modo desarrollo:</strong> Gmail bloqueado en su red. Codigo OTP:
                                <div class="fs-4 fw-bold text-center my-2"><asp:Label ID="lblOtpDev" runat="server" /></div>
                            </asp:Panel>
                            <p class="text-muted small">Ingrese el c&oacute;digo recibido en su correo.</p>
                            <div class="mb-3">
                                <label class="form-label">C&oacute;digo OTP</label>
                                <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control" MaxLength="6" />
                            </div>
                            <asp:Button ID="btnValidarOtp" runat="server" Text="Validar c&oacute;digo" CssClass="btn btn-primary w-100" OnClick="btnValidarOtp_Click" />
                        </asp:Panel>

                        <asp:Panel ID="pnlPaso3" runat="server" Visible="false">
                            <p class="text-muted small">Cree su nueva contrase&ntilde;a.</p>
                            <div class="mb-3">
                                <label class="form-label">Nueva contrase&ntilde;a</label>
                                <asp:TextBox ID="txtNuevaClave" runat="server" CssClass="form-control" TextMode="Password" />
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Confirmar contrase&ntilde;a</label>
                                <asp:TextBox ID="txtConfirmarClave" runat="server" CssClass="form-control" TextMode="Password" />
                                <asp:CompareValidator runat="server" ControlToValidate="txtConfirmarClave" ControlToCompare="txtNuevaClave"
                                    ErrorMessage="Las contrase&ntilde;as no coinciden" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <asp:Button ID="btnGuardarClave" runat="server" Text="Guardar nueva contrase&ntilde;a" CssClass="btn btn-success w-100" OnClick="btnGuardarClave_Click" />
                        </asp:Panel>

                        <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="~/Login.aspx" CssClass="btn btn-link w-100 mt-3">Volver al login</asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
