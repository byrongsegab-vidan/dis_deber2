<%@ Page Title="Catálogo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Catalogo.aspx.cs" Inherits="dis_deber2.Catalogo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-page-header">
        <h1>Catálogo de productos</h1>
        <p>Explora los productos disponibles y agrégalos a tu carrito de tienda.</p>
    </div>

    <div class="d-flex justify-content-end mb-3">
        <a runat="server" href="~/Carrito.aspx" class="btn btn-warning">
            Carrito de tienda (<asp:Label ID="lblCarritoCount" runat="server" Text="0" />)
        </a>
    </div>

    <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="alert d-block" />

    <div class="admin-glass-card mb-3">
            <div class="row g-3 align-items-end">
                <div class="col-md-5">
                    <label class="form-label">Buscar producto</label>
                    <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Nombre del producto..." />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Categoría</label>
                    <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-3">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-primary w-100" OnClick="btnFiltrar_Click" />
                </div>
            </div>
            <div class="admin-filter-toolbar admin-filter-toolbar--compact mt-3 pt-3">
                <div class="admin-sort-check">
                    <asp:CheckBox ID="chkOrdenAlfabetico" runat="server" Text="Ordenar alfabéticamente"
                        AutoPostBack="true" OnCheckedChanged="chkOrdenAlfabetico_CheckedChanged" />
                </div>
            </div>
    </div>

    <div class="admin-glass-card table-responsive">
        <asp:GridView ID="gvProductos" runat="server" CssClass="table table-hover table-bordered align-middle"
            AutoGenerateColumns="false" DataKeyNames="pro_id" OnRowCommand="gvProductos_RowCommand"
            AllowPaging="true" PageSize="8" OnPageIndexChanging="gvProductos_PageIndexChanging"
            PagerStyle-CssClass="pagination" GridLines="None">
            <Columns>
                <asp:TemplateField HeaderText="Imagen">
                    <ItemTemplate>
                        <div class="text-center" style="min-width:90px">
                            <asp:Image ID="imgProducto" runat="server" Width="64" Height="64" CssClass="rounded object-fit-cover mb-1"
                                ImageUrl='<%# RutasImagenUrl(Eval("pro_imagen_ruta")) %>' />
                            <asp:LinkButton runat="server" CommandName="Visualizar" CommandArgument='<%# Eval("pro_id") %>'
                                CssClass="btn btn-sm btn-outline-primary py-0 px-2" Text="Ver fotos" CausesValidation="false" />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="pro_nombre" HeaderText="Producto" />
                <asp:BoundField DataField="cat_nombre" HeaderText="Categoría" />
                <asp:BoundField DataField="prov_nombre" HeaderText="Proveedor" />
                <asp:BoundField DataField="pro_precio" HeaderText="Precio" DataFormatString="{0:C}" HtmlEncode="false" />
                <asp:BoundField DataField="pro_stock" HeaderText="Stock" />
                <asp:TemplateField HeaderText="Cantidad">
                    <ItemTemplate>
                        <asp:TextBox ID="txtCantidad" runat="server" CssClass="form-control form-control-sm" TextMode="Number"
                            Text="1" min="1" max='<%# Eval("pro_stock") %>' style="width:80px" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acción">
                    <ItemTemplate>
                        <asp:Button ID="btnAgregar" runat="server" Text="Agregar al carrito" CssClass="btn btn-sm btn-success"
                            CommandName="Agregar" CommandArgument='<%# Eval("pro_id") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert alert-warning mb-0">No hay productos disponibles con ese filtro.</div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <asp:Panel ID="pnlVisualizador" runat="server" Visible="false" CssClass="producto-visualizador-overlay">
        <div class="producto-visualizador-card">
            <asp:LinkButton ID="btnCerrarVisualizador" runat="server" CssClass="producto-visualizador-cerrar"
                Text="×" ToolTip="Cerrar" CausesValidation="false" OnClick="btnCerrarVisualizador_Click" />
            <h5 class="producto-visualizador-titulo">
                <asp:Label ID="lblModalProducto" runat="server" />
            </h5>
            <asp:Panel ID="pnlSinImagenesModal" runat="server" Visible="false" CssClass="alert alert-info mb-0">
                Este producto no tiene fotos registradas.
            </asp:Panel>
            <asp:Panel ID="pnlCarruselModal" runat="server">
                <div id="carouselCatalogo" class="carousel slide producto-carousel" data-bs-ride="false">
                    <div class="carousel-inner rounded">
                        <asp:Repeater ID="rptCarruselModal" runat="server">
                            <ItemTemplate>
                                <div class='carousel-item <%# (bool)Eval("activo") ? "active" : "" %>'>
                                    <img src='<%# Eval("url") %>' class="d-block w-100" alt="Foto producto" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <button class="carousel-control-prev" type="button" data-bs-target="#carouselCatalogo" data-bs-slide="prev">
                        <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                        <span class="visually-hidden">Anterior</span>
                    </button>
                    <button class="carousel-control-next" type="button" data-bs-target="#carouselCatalogo" data-bs-slide="next">
                        <span class="carousel-control-next-icon" aria-hidden="true"></span>
                        <span class="visually-hidden">Siguiente</span>
                    </button>
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
