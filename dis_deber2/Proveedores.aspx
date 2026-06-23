<%@ Page Title="Proveedores" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Proveedores.aspx.cs" Inherits="dis_deber2.Proveedores" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>CRUD Proveedores</h2>
    <asp:Label ID="lblMsg" runat="server" Visible="false" CssClass="alert d-block" />

    <div class="card mb-4">
        <div class="card-body row g-3">
            <div class="col-md-3"><asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" placeholder="Nombre" /></div>
            <div class="col-md-2"><asp:TextBox ID="txtRuc" runat="server" CssClass="form-control" placeholder="RUC" /></div>
            <div class="col-md-2"><asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" placeholder="Teléfono" /></div>
            <div class="col-md-3"><asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" placeholder="Correo" /></div>
            <div class="col-md-2"><asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" placeholder="Dirección" /></div>
            <div class="col-md-12">
                <asp:HiddenField ID="hfProvId" runat="server" />
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnRestaurar" runat="server" Text="Restaurar productos hijos" CssClass="btn btn-warning" OnClick="btnRestaurar_Click" CausesValidation="false" />
                <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" CssClass="btn btn-secondary" OnClick="btnLimpiar_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <div class="admin-filter-toolbar mb-3">
        <div class="admin-sort-check">
            <asp:CheckBox ID="chkInactivos" runat="server" Text="Mostrar inactivos (borrado lógico)" CssClass="form-check-input"
                AutoPostBack="true" OnCheckedChanged="chkInactivos_CheckedChanged" />
        </div>
        <div class="admin-sort-check">
            <asp:CheckBox ID="chkProvOrdenAlfabetico" runat="server" Text="Ordenar alfabéticamente" CssClass="form-check-input"
                AutoPostBack="true" OnCheckedChanged="chkProvOrdenAlfabetico_CheckedChanged" />
        </div>
    </div>

    <asp:GridView ID="gvProveedores" runat="server" CssClass="table table-striped" AutoGenerateColumns="False"
        AllowPaging="True" PageSize="5" OnPageIndexChanging="gvProveedores_PageIndexChanging" OnRowCommand="gvProveedores_RowCommand">
        <Columns>
            <asp:BoundField DataField="prov_id" HeaderText="ID" />
            <asp:BoundField DataField="prov_nombre" HeaderText="Nombre" />
            <asp:BoundField DataField="prov_ruc" HeaderText="RUC" />
            <asp:BoundField DataField="prov_telefono" HeaderText="Teléfono" />
            <asp:BoundField DataField="prov_correo" HeaderText="Correo" />
            <asp:BoundField DataField="prov_estado" HeaderText="Estado" />
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="Editar" CommandArgument='<%# Eval("prov_id") %>' CssClass="btn btn-sm btn-warning">Editar</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="BorradoLogico" CommandArgument='<%# Eval("prov_id") %>' CssClass="btn btn-sm btn-secondary"
                        OnClientClick="return confirm('¿Borrado lógico?');">Inactivar</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="BorradoFisico" CommandArgument='<%# Eval("prov_id") %>' CssClass="btn btn-sm btn-danger"
                        OnClientClick="return confirm('¿Borrado FÍSICO? Los productos hijos guardarán el proveedor en respaldo.');">Eliminar</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("prov_id") %>' CssClass="btn btn-sm btn-success">Reactivar</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <hr class="my-5" id="categorias" />

    <h2>CRUD Categorías</h2>
    <asp:Label ID="lblMsgCat" runat="server" Visible="false" CssClass="alert d-block" />

    <div class="card mb-4">
        <div class="card-body row g-3 align-items-end">
            <div class="col-md-6">
                <label class="form-label fw-bold"><asp:Label ID="lblCatFormTitulo" runat="server" Text="Nueva categoría" /></label>
                <asp:TextBox ID="txtCatNombre" runat="server" CssClass="form-control" placeholder="Ej: Electrónica" />
            </div>
            <div class="col-md-6">
                <asp:HiddenField ID="hfCatId" runat="server" />
                <asp:Button ID="btnCatGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnCatGuardar_Click" />
                <asp:Button ID="btnCatLimpiar" runat="server" Text="Limpiar" CssClass="btn btn-secondary ms-2" OnClick="btnCatLimpiar_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-md-4">
                    <label class="form-label">Filtrar por nombre</label>
                    <asp:TextBox ID="txtCatFiltroNombre" runat="server" CssClass="form-control" placeholder="Ej: Ropa" />
                </div>
            </div>
            <div class="admin-filter-toolbar mt-3 pt-3">
                <div class="admin-sort-check">
                    <asp:CheckBox ID="chkCatOrdenAlfabetico" runat="server" Text="Ordenar alfabéticamente" CssClass="form-check-input"
                        AutoPostBack="true" OnCheckedChanged="chkCatOrdenAlfabetico_CheckedChanged" />
                </div>
                <div class="admin-filter-actions">
                    <asp:Button ID="btnCatFiltrar" runat="server" Text="Buscar" CssClass="btn btn-primary" OnClick="btnCatFiltrar_Click" CausesValidation="false" />
                    <asp:Button ID="btnCatLimpiarFiltro" runat="server" Text="Ver lista completa" CssClass="btn btn-outline-secondary" OnClick="btnCatLimpiarFiltro_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
    </div>

    <asp:GridView ID="gvCategorias" runat="server" CssClass="table table-striped table-bordered align-middle bg-white"
        AutoGenerateColumns="False" AllowPaging="True" PageSize="5"
        OnPageIndexChanging="gvCategorias_PageIndexChanging" OnRowCommand="gvCategorias_RowCommand"
        OnRowDataBound="gvCategorias_RowDataBound" DataKeyNames="cat_id,cat_estado">
        <Columns>
            <asp:BoundField DataField="cat_id" HeaderText="ID" />
            <asp:BoundField DataField="cat_nombre" HeaderText="Nombre" />
            <asp:TemplateField HeaderText="Estado">
                <ItemTemplate>
                    <asp:DropDownList ID="ddlCatEstado" runat="server" CssClass="form-select form-select-sm"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlCatEstado_SelectedIndexChanged">
                        <asp:ListItem Value="A" Text="ACTIVO" />
                        <asp:ListItem Value="I" Text="INACTIVO" />
                    </asp:DropDownList>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Acciones">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="EditarCat" CommandArgument='<%# Eval("cat_id") %>'
                        CssClass="btn btn-sm btn-warning me-1" CausesValidation="false">Editar</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="BorradoFisicoCat" CommandArgument='<%# Eval("cat_id") %>'
                        CssClass="btn btn-sm btn-danger" CausesValidation="false"
                        OnClientClick="return confirm('¿Borrado FÍSICO? Los productos quedarán sin categoría.');">Eliminar</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            <div class="alert alert-warning mb-0">No hay categorías que coincidan con el filtro.</div>
        </EmptyDataTemplate>
    </asp:GridView>
</asp:Content>
