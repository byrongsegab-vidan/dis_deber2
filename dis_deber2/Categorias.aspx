<%@ Page Title="Categorías" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Categorias.aspx.cs" Inherits="dis_deber2.Categorias" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-page-header">
        <h1>CRUD Categorías</h1>
        <p>Administra las categorías de productos del catálogo.</p>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="alert alert-info d-block" Visible="false" />

    <div class="admin-glass-card card-magenta mb-4">
        <h3 class="admin-card-title"><asp:Label ID="lblFormTitulo" runat="server" Text="Nueva categoría" /></h3>
        <div class="row g-3 align-items-end">
            <div class="col-md-6">
                <label class="form-label">Nombre</label>
                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" placeholder="Ej: Electrónica" />
            </div>
            <div class="col-md-6">
                <asp:HiddenField ID="hfCatId" runat="server" />
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="admin-btn-glow btn-cyan" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" CssClass="btn btn-secondary ms-2" OnClick="btnLimpiar_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <div class="admin-glass-card card-green mb-3">
        <h3 class="admin-card-title">Filtrar categorías</h3>
        <div class="row g-3 align-items-end">
            <div class="col-md-4">
                <label class="form-label">Nombre</label>
                <asp:TextBox ID="txtFiltroNombre" runat="server" CssClass="form-control" placeholder="Ej: Ropa" />
            </div>
            <div class="col-md-4 d-flex gap-2">
                <asp:Button ID="btnFiltrar" runat="server" Text="Buscar" CssClass="admin-btn-glow btn-green flex-fill" OnClick="btnFiltrar_Click" CausesValidation="false" />
                <asp:Button ID="btnLimpiarFiltro" runat="server" Text="Ver lista completa" CssClass="btn btn-outline-secondary flex-fill" OnClick="btnLimpiarFiltro_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="gvCategorias" runat="server" CssClass="table table-striped table-bordered align-middle bg-white"
            AutoGenerateColumns="False" AllowPaging="False"
            OnRowCommand="gvCategorias_RowCommand" OnRowDataBound="gvCategorias_RowDataBound"
            DataKeyNames="cat_id,cat_estado">
            <Columns>
                <asp:BoundField DataField="cat_id" HeaderText="ID" />
                <asp:BoundField DataField="cat_nombre" HeaderText="Nombre" />
                <asp:TemplateField HeaderText="Estado">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select form-select-sm"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_SelectedIndexChanged">
                            <asp:ListItem Value="A" Text="ACTIVO" />
                            <asp:ListItem Value="I" Text="INACTIVO" />
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="Editar" CommandArgument='<%# Eval("cat_id") %>'
                            CssClass="btn btn-sm btn-warning me-1" CausesValidation="false">Editar</asp:LinkButton>
                        <asp:LinkButton runat="server" CommandName="BorradoFisico" CommandArgument='<%# Eval("cat_id") %>'
                            CssClass="btn btn-sm btn-danger" CausesValidation="false"
                            OnClientClick="return confirm('¿Borrado FÍSICO? Los productos quedarán sin categoría.');">Eliminar</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert alert-warning mb-0">No hay categorías que coincidan con el filtro.</div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>

