<%@ Page Title="Registrarse" Language="C#" AutoEventWireup="true" CodeBehind="Registrarse.aspx.cs" Inherits="dis_deber2.Registrarse" ResponseEncoding="utf-8" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <title>Registrarse - dis_deber2</title>
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server" class="container py-5">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card shadow">
                    <div class="card-body p-4">
                        <h3 class="mb-2 text-center">Crear cuenta</h3>
                        <p class="text-muted text-center small mb-4">Registro como usuario normal (Cliente)</p>
                        <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="alert d-block" />
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label class="form-label">C&eacute;dula</label>
                                <asp:TextBox ID="txtCedula" runat="server" CssClass="form-control" MaxLength="10" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCedula" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Nick</label>
                                <asp:TextBox ID="txtNick" runat="server" CssClass="form-control" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Nombres</label>
                                <asp:TextBox ID="txtNombres" runat="server" CssClass="form-control" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombres" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Apellidos</label>
                                <asp:TextBox ID="txtApellidos" runat="server" CssClass="form-control" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtApellidos" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <div class="col-12">
                                <label class="form-label">Correo</label>
                                <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" TextMode="Email" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCorreo" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Contrase&ntilde;a</label>
                                <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtContrasena" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Confirmar contrase&ntilde;a</label>
                                <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password" />
                                <asp:CompareValidator runat="server" ControlToValidate="txtConfirmar" ControlToCompare="txtContrasena"
                                    ErrorMessage="Las contrase&ntilde;as no coinciden" CssClass="text-danger" Display="Dynamic" />
                            </div>
                            <div class="col-12">
                                <label class="form-label">Foto de perfil <span class="text-danger">*</span></label>
                                <asp:FileUpload ID="fuFoto" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp,.jpg,.jpeg,.png,.gif,.webp" />
                                <small class="text-muted">Solo im&aacute;genes JPG, PNG, GIF o WEBP (m&aacute;x. 5 MB).</small>
                            </div>
                        </div>
                        <asp:Button ID="btnRegistrar" runat="server" Text="Registrarse" CssClass="btn btn-success w-100 mt-4" OnClick="btnRegistrar_Click" />
                        <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="~/Login.aspx" CssClass="btn btn-link w-100 mt-2">Ya tengo cuenta - Iniciar sesi&oacute;n</asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
