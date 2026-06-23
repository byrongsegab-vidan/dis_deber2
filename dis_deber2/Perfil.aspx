<%@ Page Title="Mi perfil" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Perfil.aspx.cs" Inherits="dis_deber2.Perfil" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
        <div class="col-lg-10">
            <h2 class="mb-1">Mi perfil</h2>
            <p class="text-muted mb-4">Consulta y actualiza tus datos personales.</p>

            <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="alert d-block" />

            <div class="card shadow-sm">
                <div class="card-body p-4">
                    <div class="row g-4">
                        <div class="col-md-4 text-center">
                            <asp:Image ID="imgPerfil" runat="server" CssClass="rounded-circle border mb-3" Width="160" Height="160" />
                            <p class="small text-muted mb-2">Foto de perfil</p>
                            <asp:FileUpload ID="fuFoto" runat="server" CssClass="form-control form-control-sm" accept="image/jpeg,image/png,image/gif,image/webp,.jpg,.jpeg,.png,.gif,.webp" />
                            <small class="text-muted d-block mt-2">JPG, PNG, GIF o WEBP. M&aacute;x. 5 MB.</small>
                        </div>

                        <div class="col-md-8">
                            <div class="row g-3">
                                <div class="col-md-6">
                                    <label class="form-label">C&eacute;dula</label>
                                    <asp:TextBox ID="txtCedula" runat="server" CssClass="form-control" MaxLength="10" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Nick</label>
                                    <asp:TextBox ID="txtNick" runat="server" CssClass="form-control" MaxLength="50" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Nombres</label>
                                    <asp:TextBox ID="txtNombres" runat="server" CssClass="form-control" MaxLength="50" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNombres" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Apellidos</label>
                                    <asp:TextBox ID="txtApellidos" runat="server" CssClass="form-control" MaxLength="50" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtApellidos" ErrorMessage="Requerido" CssClass="text-danger" Display="Dynamic" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Correo</label>
                                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" ReadOnly="true" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Rol</label>
                                    <asp:TextBox ID="txtRol" runat="server" CssClass="form-control" ReadOnly="true" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Celular</label>
                                    <asp:TextBox ID="txtCelular" runat="server" CssClass="form-control" MaxLength="10" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label">Fecha de cumplea&ntilde;os</label>
                                    <asp:TextBox ID="txtCumple" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                                <div class="col-12">
                                    <label class="form-label">Direcci&oacute;n</label>
                                    <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" MaxLength="50" />
                                </div>
                            </div>

                            <asp:Button ID="btnGuardar" runat="server" Text="Guardar cambios" CssClass="btn btn-primary mt-4"
                                OnClick="btnGuardar_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
