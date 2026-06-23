<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="dis_deber2._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
        <div class="admin-page-header">
            <h1>Panel de administración</h1>
            <p>Gestión de productos, proveedores, estadísticas y carga masiva.</p>
        </div>

        <div class="admin-search-bar">
            <asp:TextBox ID="txtBuscarDash" runat="server" placeholder="Buscar productos, categorías..." />
            <asp:LinkButton ID="btnBuscarDash" runat="server" CssClass="admin-search-btn" OnClick="btnBuscarDash_Click" Text="🔍" />
        </div>

        <div class="admin-grid-top">
            <div class="admin-glass-card card-cyan">
                <span class="admin-card-icon-corner">📦</span>
                <h3 class="admin-card-title">Inventario de Productos</h3>
                <div class="admin-donut-wrap">
                    <div id="donutChart" runat="server" class="admin-donut"></div>
                    <div class="admin-cat-chips">
                        <asp:Repeater ID="rptCategorias" runat="server">
                            <ItemTemplate>
                                <div class="admin-cat-chip">
                                    <span><%# IconoCategoria(Container.ItemIndex) %></span>
                                    <span><%# Eval("categoria") %> (<%# Eval("total_productos") %>)</span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
                <div class="admin-stat-row">
                    <span>Total productos activos</span>
                    <strong><asp:Label ID="lblTotalProductos" runat="server" Text="0" /></strong>
                </div>
                <a runat="server" href="~/Productos.aspx" class="admin-btn-glow btn-cyan">Gestionar Productos</a>
            </div>

            <div class="admin-glass-card card-green">
                <span class="admin-card-icon-corner">🤝</span>
                <h3 class="admin-card-title">Red de Proveedores</h3>
                <div class="admin-map-placeholder">🌍</div>
                <p class="admin-card-title" style="font-size:0.85rem;margin-bottom:8px;">Top proveedores</p>
                <ul class="admin-prov-list">
                    <asp:Repeater ID="rptProveedores" runat="server">
                        <ItemTemplate>
                            <li><%# Eval("prov_nombre") %></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
                <div class="admin-stat-row">
                    <span>Proveedores activos</span>
                    <strong><asp:Label ID="lblTotalProveedores" runat="server" Text="0" /></strong>
                </div>
                <a runat="server" href="~/Proveedores.aspx" class="admin-btn-glow btn-green">Gestionar Proveedores</a>
            </div>
        </div>

        <div class="admin-glass-card card-magenta">
            <h3 class="admin-card-title">Flujo de Carga Masiva</h3>
            <div class="admin-upload-zone">
                <div class="admin-dropzone">
                    <div class="admin-dropzone-icons">📄 📊</div>
                    <p>Arrastra CSV o XLS aquí</p>
                    <p class="text-muted small mt-1">Previsualización antes de importar</p>
                </div>
                <div class="admin-progress-wrap">
                    <div class="admin-progress-icon">☁️⬆️</div>
                    <div class="admin-progress-bar">
                        <div class="admin-progress-fill" style="width:30%"></div>
                    </div>
                    <div class="admin-progress-label">Listo para importar datos</div>
                </div>
            </div>
            <div class="mt-3 text-end">
                <a runat="server" href="~/CargaMasiva.aspx" class="admin-btn-glow btn-magenta">Iniciar Carga</a>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
