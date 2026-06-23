<%@ Page Title="Carrito" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Carrito.aspx.cs" Inherits="dis_deber2.Carrito" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <div>
            <h2 class="mb-1">Mi carrito</h2>
            <p class="text-muted mb-0">Revisa tus productos y confirma tu compra.</p>
        </div>
        <a runat="server" href="~/Catalogo.aspx" class="btn btn-outline-primary">Seguir comprando</a>
    </div>

    <asp:Panel ID="pnlCarritoVacio" runat="server" Visible="false" CssClass="alert alert-info">
        Tu carrito está vacío. <a runat="server" href="~/Catalogo.aspx">Ir al catálogo</a>
    </asp:Panel>

    <asp:Panel ID="pnlCarrito" runat="server">
        <div class="table-responsive">
            <asp:GridView ID="gvCarrito" runat="server" CssClass="table table-bordered align-middle"
                AutoGenerateColumns="false" DataKeyNames="ProId" OnRowCommand="gvCarrito_RowCommand" GridLines="None">
                <Columns>
                    <asp:BoundField DataField="Nombre" HeaderText="Producto" />
                    <asp:BoundField DataField="Precio" HeaderText="Precio unitario" DataFormatString="{0:C}" HtmlEncode="false" />
                    <asp:TemplateField HeaderText="Cantidad">
                        <ItemTemplate>
                            <asp:TextBox ID="txtCantidad" runat="server" CssClass="form-control form-control-sm"
                                TextMode="Number" Text='<%# Eval("Cantidad") %>' style="width:80px" min="1" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Subtotal" HeaderText="Subtotal" DataFormatString="{0:C}" HtmlEncode="false" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:Button ID="btnActualizar" runat="server" Text="Actualizar" CssClass="btn btn-sm btn-outline-secondary me-1"
                                CommandName="Actualizar" CommandArgument='<%# Eval("ProId") %>' />
                            <asp:Button ID="btnQuitar" runat="server" Text="Quitar" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Quitar" CommandArgument='<%# Eval("ProId") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="card mt-3">
            <div class="card-body d-flex justify-content-between align-items-center flex-wrap gap-2">
                <div>
                    <span class="text-muted">Total a pagar:</span>
                    <h3 class="text-primary mb-0"><asp:Label ID="lblTotal" runat="server" /></h3>
                </div>
                <asp:Button ID="btnConfirmar" runat="server" Text="Confirmar compra" CssClass="btn btn-lg btn-success"
                    OnClick="btnConfirmar_Click" />
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlResumenCompra" runat="server" Visible="false" CssClass="mt-4">
        <div class="card border-success shadow">
            <div class="card-header bg-success text-white">
                <h4 class="mb-0">Compra confirmada</h4>
            </div>
            <div class="card-body">
                <p class="mb-1"><strong>Cliente:</strong> <asp:Label ID="lblResumenCliente" runat="server" /></p>
                <p class="mb-1"><strong>Fecha:</strong> <asp:Label ID="lblResumenFecha" runat="server" /></p>
                <p class="mb-3"><strong>Correo:</strong> <asp:Label ID="lblResumenCorreo" runat="server" /></p>

                <div class="table-responsive">
                    <asp:GridView ID="gvResumen" runat="server" CssClass="table table-sm table-striped"
                        AutoGenerateColumns="false" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Nombre" HeaderText="Producto" />
                            <asp:BoundField DataField="Cantidad" HeaderText="Cant." />
                            <asp:BoundField DataField="Precio" HeaderText="Precio" DataFormatString="{0:C}" HtmlEncode="false" />
                            <asp:BoundField DataField="Subtotal" HeaderText="Subtotal" DataFormatString="{0:C}" HtmlEncode="false" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="text-end mt-3">
                    <h5>Total pagado: <asp:Label ID="lblResumenTotal" runat="server" CssClass="text-success" /></h5>
                </div>

                <p class="text-muted small mt-3 mb-0">Esta es una vista de demostración. No se registró el pedido en la base de datos.</p>
            </div>
            <div class="card-footer bg-white">
                <a runat="server" href="~/Catalogo.aspx" class="btn btn-primary">Volver al catálogo</a>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
