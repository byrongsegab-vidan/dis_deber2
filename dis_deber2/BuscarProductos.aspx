<%@ Page Title="Buscar productos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BuscarProductos.aspx.cs" Inherits="dis_deber2.BuscarProductos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Búsqueda rápida de productos</h2>
    <p class="text-muted">Estilo Facebook: escribe y filtra al instante por categoría, precio y proveedor.</p>

    <asp:UpdatePanel ID="upBusqueda" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="card mb-3">
                <div class="card-body">
                    <div class="row g-3 align-items-end">
                        <div class="col-md-4">
                            <label class="form-label">Buscar</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-lg" placeholder="¿Qué producto buscas?" AutoPostBack="true" OnTextChanged="txtBuscar_TextChanged" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Categoría</label>
                            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FiltroChanged" />
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Precio mín</label>
                            <asp:TextBox ID="txtPrecioMin" runat="server" CssClass="form-control" TextMode="Number" AutoPostBack="true" OnTextChanged="FiltroChanged" />
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Precio máx</label>
                            <asp:TextBox ID="txtPrecioMax" runat="server" CssClass="form-control" TextMode="Number" AutoPostBack="true" OnTextChanged="FiltroChanged" />
                        </div>
                        <div class="col-md-1">
                            <asp:Button ID="btnBuscar" runat="server" Text="Ir" CssClass="btn btn-primary w-100" OnClick="FiltroChanged" />
                        </div>
                    </div>
                    <div class="admin-filter-toolbar admin-filter-toolbar--compact mt-3 pt-3">
                        <div class="admin-sort-check">
                            <asp:CheckBox ID="chkOrdenAlfabetico" runat="server" Text="Ordenar alfabéticamente" CssClass="form-check-input"
                                AutoPostBack="true" OnCheckedChanged="FiltroChanged" />
                        </div>
                    </div>
                </div>
            </div>

            <asp:Repeater ID="rptResultados" runat="server">
                <HeaderTemplate><div class="row g-3"></HeaderTemplate>
                <ItemTemplate>
                    <div class="col-md-4 col-lg-3">
                        <div class="card h-100 shadow-sm product-card">
                            <asp:Image ID="img" runat="server" CssClass="card-img-top" Height="160" ImageUrl='<%# RutasImagenUrl(Eval("pro_imagen_ruta")) %>' />
                            <div class="card-body">
                                <h6 class="card-title"><%# Eval("pro_nombre") %></h6>
                                <p class="card-text small text-muted mb-1"><%# Eval("cat_nombre") %> · <%# Eval("prov_nombre") %></p>
                                <strong class="text-primary"><%# string.Format("{0:C}", Eval("pro_precio")) %></strong>
                                <span class="badge bg-secondary float-end">Stock: <%# Eval("pro_stock") %></span>
                            </div>
                            <div class="card-footer bg-white border-0">
                                <a href='<%# "Estadisticas.aspx?id=" + Eval("pro_id") %>' class="btn btn-sm btn-outline-primary w-100">Ver estadísticas</a>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>

            <asp:Label ID="lblSinResultados" runat="server" CssClass="alert alert-warning" Visible="false" Text="No se encontraron productos." />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="txtBuscar" EventName="TextChanged" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
