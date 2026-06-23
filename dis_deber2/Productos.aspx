<%@ Page Title="Productos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Productos.aspx.cs" Inherits="dis_deber2.Productos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-page-header">
        <h1>CRUD Productos</h1>
        <p>Administra productos, estados e imágenes (mínimo 3, máximo 11 por producto).</p>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="alert alert-info d-block" Visible="false" />

    <div class="admin-glass-card card-cyan mb-4">
        <h3 class="admin-card-title"><asp:Label ID="lblFormTitulo" runat="server" Text="Nuevo producto" /></h3>
        <div class="row g-3">
            <div class="col-md-3">
                <label class="form-label">Nombre</label>
                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Descripción</label>
                <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-2">


                <label class="form-label">Precio</label>
                <asp:TextBox ID="txtPrecio" runat="server" CssClass="form-control" TextMode="Number" step="0.01" />
            </div>
            <div class="col-md-2">
                <label class="form-label">Stock</label>
                <asp:TextBox ID="txtStock" runat="server" CssClass="form-control" TextMode="Number" />
            </div>
            <div class="col-md-2">
                <label class="form-label">Categoría</label>
                <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Proveedor</label>


                <asp:DropDownList ID="ddlProveedor" runat="server" CssClass="form-select" />
            </div>
        </div>

        <asp:Panel ID="pnlImagenesNuevo" runat="server" CssClass="mt-3">
            <label class="form-label fw-bold">Fotos del producto (mínimo 3, máximo 11) <span class="text-danger">*</span></label>
            <div class="row g-2" id="contenedorFotosNuevo">
                <div class="col-md-4 foto-slot-nuevo">
                    <asp:FileUpload ID="fuImg1" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 1</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo">
                    <asp:FileUpload ID="fuImg2" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 2</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo">
                    <asp:FileUpload ID="fuImg3" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 3</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg4" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 4</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg5" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 5</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg6" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 6</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg7" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 7</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg8" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 8</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg9" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 9</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg10" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 10</small>
                </div>
                <div class="col-md-4 foto-slot-nuevo d-none extra-foto-nuevo">
                    <asp:FileUpload ID="fuImg11" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Foto 11</small>
                </div>
            </div>
            <button type="button" id="btnAgregarFotoNuevo" class="btn btn-sm btn-outline-secondary mt-2">+ Agregar foto</button>
            <small class="text-muted d-block mt-1">Solo JPG, PNG, GIF o WEBP. Ruta: ~/Subidas/Productos/</small>
        </asp:Panel>

        <asp:Panel ID="pnlImagenesEditar" runat="server" Visible="false" CssClass="mt-3">
            <label class="form-label fw-bold">Fotos del producto (mínimo 3, máximo 11)</label>
            <asp:Repeater ID="rptImagenes" runat="server" OnItemCommand="rptImagenes_ItemCommand">
                <HeaderTemplate><div class="d-flex flex-wrap gap-3 mb-3"></HeaderTemplate>
                <ItemTemplate>
                    <div class="text-center border rounded p-2 bg-white">
                        <asp:Image ID="imgGal" runat="server" Width="90" Height="90" CssClass="rounded object-fit-cover"
                            ImageUrl='<%# RutasImagenUrl(Eval("ipro_ruta")) %>' />
                        <div class="mt-1">
                            <asp:LinkButton runat="server" CommandName="QuitarImagen" CommandArgument='<%# Eval("ipro_id") %>'
                                CssClass="btn btn-sm btn-outline-danger" CausesValidation="false"
                                OnClientClick="return confirm('¿Eliminar esta foto?');">Quitar</asp:LinkButton>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>
            <label class="form-label">Subir fotos adicionales</label>
            <asp:Label ID="lblLimiteFotosEditar" runat="server" CssClass="text-muted small d-block mb-2" />
            <div class="row g-2" id="contenedorFotosEditar">
                <div class="col-md-4 foto-slot-editar">
                    <asp:FileUpload ID="fuExtra1" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 1</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra2" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 2</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra3" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 3</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra4" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 4</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra5" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 5</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra6" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 6</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra7" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 7</small>
                </div>
                <div class="col-md-4 foto-slot-editar d-none extra-foto-editar">
                    <asp:FileUpload ID="fuExtra8" runat="server" CssClass="form-control" accept="image/jpeg,image/png,image/gif,image/webp" />
                    <small class="text-muted">Nueva foto 8</small>
                </div>
            </div>
            <button type="button" id="btnAgregarFotoEditar" class="btn btn-sm btn-outline-secondary mt-2">+ Agregar foto</button>
            <small class="text-muted d-block mt-1">Solo JPG, PNG, GIF o WEBP. Ruta: ~/Subidas/Productos/</small>
        </asp:Panel>

        <div class="mt-3">
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="admin-btn-glow btn-cyan" OnClick="btnGuardar_Click" />
            <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" CssClass="btn btn-secondary ms-2" OnClick="btnLimpiar_Click" CausesValidation="false" />
        </div>
        <asp:HiddenField ID="hfProId" runat="server" />
    </div>

    <div class="admin-glass-card card-green mb-3">
        <h3 class="admin-card-title">Filtrar productos</h3>
        <p class="text-muted small mb-3">Complete uno o más campos y pulse Buscar. Los filtros se combinan (deben cumplirse todos). Deje precio mín. o máx. vacío si no desea limitar por ese extremo.</p>
        <div class="row g-3 align-items-end">
            <div class="col-md-3">
                <label class="form-label">Nombre</label>
                <asp:TextBox ID="txtFiltroNombre" runat="server" CssClass="form-control" placeholder="Ej: Arroz" />
            </div>
            <div class="col-md-2">
                <label class="form-label">Categoría</label>
                <asp:DropDownList ID="ddlFiltroCategoria" runat="server" CssClass="form-select" />
            </div>
            <div class="col-md-2">
                <label class="form-label">Proveedor</label>
                <asp:DropDownList ID="ddlFiltroProveedor" runat="server" CssClass="form-select" />
            </div>
            <div class="col-md-2">
                <label class="form-label">Precio mín.</label>
                <asp:TextBox ID="txtFiltroPrecioMin" runat="server" CssClass="form-control" TextMode="Number" step="0.01" placeholder="Ej: 10" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Precio máx.</label>
                <asp:TextBox ID="txtFiltroPrecioMax" runat="server" CssClass="form-control" TextMode="Number" step="0.01" placeholder="Ej: 100" />
            </div>
        </div>
        <div class="admin-filter-toolbar mt-3 pt-3">
            <div class="admin-sort-check">
                <asp:CheckBox ID="chkOrdenAlfabetico" runat="server" CssClass="form-check-input"
                    Text="Ordenar alfabéticamente" AutoPostBack="true" OnCheckedChanged="chkOrdenAlfabetico_CheckedChanged" />
            </div>
            <div class="admin-filter-actions">
                <asp:Button ID="btnFiltrar" runat="server" Text="Buscar" CssClass="admin-btn-glow btn-green" OnClick="btnFiltrar_Click" CausesValidation="false" />
                <asp:Button ID="btnLimpiarFiltro" runat="server" Text="Ver lista completa" CssClass="btn btn-outline-secondary" OnClick="btnLimpiarFiltro_Click" CausesValidation="false" />
            </div>
        </div>
        <asp:Label ID="lblFiltroResumen" runat="server" Visible="false" CssClass="alert alert-secondary mt-3 mb-0 py-2 small d-block" />
    </div>

    <div class="table-responsive">
        <asp:GridView ID="gvProductos" runat="server" CssClass="table table-striped table-bordered align-middle bg-white"
            AutoGenerateColumns="False" AllowPaging="False"
            OnRowCommand="gvProductos_RowCommand" OnRowDataBound="gvProductos_RowDataBound"
            DataKeyNames="pro_id,pro_estado">
            <Columns>
                <asp:BoundField DataField="pro_id" HeaderText="ID" />
                <asp:TemplateField HeaderText="Imagen">
                    <ItemTemplate>
                        <div class="text-center" style="min-width:90px">
                            <asp:Image ID="imgProd" runat="server" Width="60" Height="60" CssClass="rounded mb-1"
                                ImageUrl='<%# RutasImagenUrl(Eval("pro_imagen_ruta")) %>' />
                            <asp:LinkButton runat="server" CommandName="Visualizar" CommandArgument='<%# Eval("pro_id") %>'
                                CssClass="btn btn-sm btn-outline-primary py-0 px-2" Text="Visualizar" CausesValidation="false" />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="pro_nombre" HeaderText="Nombre" />
                <asp:BoundField DataField="cat_nombre" HeaderText="Categoría" />
                <asp:BoundField DataField="prov_nombre" HeaderText="Proveedor" />
                <asp:BoundField DataField="pro_precio" HeaderText="Precio" DataFormatString="{0:C}" HtmlEncode="false" />
                <asp:BoundField DataField="pro_stock" HeaderText="Stock" />
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
                        <asp:LinkButton runat="server" CommandName="Editar" CommandArgument='<%# Eval("pro_id") %>'
                            CssClass="btn btn-sm btn-warning me-1" CausesValidation="false">Editar</asp:LinkButton>
                        <asp:LinkButton runat="server" CommandName="BorradoFisico" CommandArgument='<%# Eval("pro_id") %>'
                            CssClass="btn btn-sm btn-danger" CausesValidation="false"
                            OnClientClick="return confirm('¿Borrado FÍSICO? Se eliminará de la base de datos.');">Eliminar</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert alert-warning mb-0">
                    No hay productos que coincidan con los filtros aplicados.
                    Revise el rango de precios o use <strong>Ver lista completa</strong>.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <div class="d-flex justify-content-between align-items-center mt-2">
        <asp:Button ID="btnPaginaAnterior" runat="server" Text="« Anterior" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnPaginaAnterior_Click" CausesValidation="false" />
        <asp:Label ID="lblPaginacion" runat="server" CssClass="text-muted" />
        <asp:Button ID="btnPaginaSiguiente" runat="server" Text="Siguiente »" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnPaginaSiguiente_Click" CausesValidation="false" />
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
                <div id="carouselVisualizador" class="carousel slide producto-carousel" data-bs-ride="false">
                    <div class="carousel-inner rounded">
                        <asp:Repeater ID="rptCarruselModal" runat="server">
                            <ItemTemplate>
                                <div class='carousel-item <%# (bool)Eval("activo") ? "active" : "" %>'>
                                    <img src='<%# Eval("url") %>' class="d-block w-100" alt="Foto producto" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <button class="carousel-control-prev" type="button" data-bs-target="#carouselVisualizador" data-bs-slide="prev">
                        <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                        <span class="visually-hidden">Anterior</span>
                    </button>
                    <button class="carousel-control-next" type="button" data-bs-target="#carouselVisualizador" data-bs-slide="next">
                        <span class="carousel-control-next-icon" aria-hidden="true"></span>
                        <span class="visually-hidden">Siguiente</span>
                    </button>
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>

    <script>
        (function () {
            function mostrarSiguiente(selectorExtra, btnId) {
                var ocultos = document.querySelectorAll(selectorExtra + '.d-none');
                if (ocultos.length === 0) return;
                ocultos[0].classList.remove('d-none');
                if (document.querySelectorAll(selectorExtra + '.d-none').length === 0) {
                    var btn = document.getElementById(btnId);
                    if (btn) btn.disabled = true;
                }
            }

            document.addEventListener('DOMContentLoaded', function () {
                var btnNuevo = document.getElementById('btnAgregarFotoNuevo');
                if (btnNuevo) {
                    btnNuevo.addEventListener('click', function () {
                        mostrarSiguiente('.extra-foto-nuevo', 'btnAgregarFotoNuevo');
                    });
                }

                var btnEditar = document.getElementById('btnAgregarFotoEditar');
                if (btnEditar) {
                    btnEditar.addEventListener('click', function () {
                        mostrarSiguiente('.extra-foto-editar', 'btnAgregarFotoEditar');
                    });
                }
            });
        })();
    </script>
</asp:Content>
