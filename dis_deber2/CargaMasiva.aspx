<%@ Page Title="Carga masiva" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CargaMasiva.aspx.cs" Inherits="dis_deber2.CargaMasiva" %>



<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="admin-page-header">

        <h1>Carga masiva desde Excel / CSV</h1>

        <p>Sube un archivo Excel (.xlsx, .xls) o CSV. Soporta hasta <strong>50 000</strong> filas de productos (archivo máx. 100 MB). En archivos grandes se usa importación rápida por lotes.</p>

    </div>



    <div class="admin-glass-card card-magenta mb-4">

        <h3 class="admin-card-title">Flujo de carga masiva</h3>

        <div class="row g-3 align-items-end">

            <div class="col-md-3">

                <label class="form-label">Modo</label>

                <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-select">

                    <asp:ListItem Value="auto" Text="Automático (recomendado)" Selected="True" />

                    <asp:ListItem Value="prov" Text="Solo proveedores" />

                    <asp:ListItem Value="pro" Text="Solo productos" />

                </asp:DropDownList>

            </div>

            <div class="col-md-5">

                <label class="form-label">Archivo Excel o CSV</label>

                <asp:FileUpload ID="fuArchivo" runat="server" CssClass="form-control" accept=".csv,.xls,.xlsx" />

            </div>

            <div class="col-md-2">

                <asp:Button ID="btnImportar" runat="server" Text="Importar" CssClass="admin-btn-glow btn-magenta w-100" OnClick="btnImportar_Click" />

            </div>

            <div class="col-md-2">

                <asp:Button ID="btnPrevisualizar" runat="server" Text="Previsualizar" CssClass="btn btn-outline-primary w-100" OnClick="btnPrevisualizar_Click" CausesValidation="false" />

            </div>

        </div>

        <div class="row g-3 mt-2">

            <div class="col-md-6">

                <div class="form-check">

                    <asp:CheckBox ID="chkOmitirImagenes" runat="server" CssClass="form-check-input" Checked="true" />

                    <label class="form-check-label" for="chkOmitirImagenes">Omitir imágenes (recomendado para +500 filas)</label>

                </div>

            </div>

            <div class="col-md-6">

                <div class="form-check">

                    <asp:CheckBox ID="chkModoRapido" runat="server" CssClass="form-check-input" />

                    <label class="form-check-label" for="chkModoRapido">Forzar importación rápida por lotes</label>

                </div>

            </div>

        </div>

    </div>



    <asp:Label ID="lblMsg" runat="server" Visible="false" CssClass="alert d-block" />

    <asp:Literal ID="litResumen" runat="server" Visible="false" />



    <asp:Panel ID="pnlHojas" runat="server" Visible="false" CssClass="mb-3">

        <asp:Repeater ID="rptHojas" runat="server">

            <ItemTemplate>

                <h5 class="mt-3">Hoja: <%# Eval("Key") %> (<%# Eval("RowCount") %> filas)</h5>

            </ItemTemplate>

        </asp:Repeater>

    </asp:Panel>



    <asp:GridView ID="gvPreview" runat="server" CssClass="table table-sm table-bordered table-striped bg-white" AutoGenerateColumns="True" Visible="false" />



    <div class="admin-glass-card card-green mt-4">

        <h3 class="admin-card-title">Plantillas y formato</h3>

        <p class="small text-muted mb-2">Descarga las plantillas de ejemplo (ábrelas con Excel). Para carga completa, crea un libro con 3 hojas: <strong>Categorias</strong>, <strong>Proveedores</strong>, <strong>Productos</strong>.</p>

        <div class="d-flex flex-wrap gap-2 mb-3">

            <a class="btn btn-sm btn-outline-success" href="<%= ResolveUrl("~/Plantillas/categorias.csv") %>">Plantilla categorías</a>

            <a class="btn btn-sm btn-outline-success" href="<%= ResolveUrl("~/Plantillas/proveedores.csv") %>">Plantilla proveedores</a>

            <a class="btn btn-sm btn-outline-success" href="<%= ResolveUrl("~/Plantillas/productos.csv") %>">Plantilla productos</a>

        </div>



        <h6>Formato Amazon / scraping (CSV directo)</h6>

        <p class="small mb-2">También se acepta CSV con columnas de Amazon: <code>title</code>, <code>asin</code>, <code>price</code>, <code>currency</code>, <code>brand</code>, <code>images</code> (URLs separadas por <code>~</code>), <code>description</code>, <code>breadcrumbs</code>, etc. El sistema detecta el formato automáticamente, crea categorías desde <code>breadcrumbs</code>, proveedores desde <code>brand</code> y convierte precios INR a USD.</p>



        <h6>Columnas — Productos (con imágenes)</h6>

        <pre class="bg-light p-2 small mb-3">pro_nombre,pro_descripcion,pro_precio,pro_stock,cat_nombre,prov_nombre,pro_imagen_1,pro_imagen_2,pro_imagen_3

Collar perro,Collar ajustable,12.50,50,Deportes,TechSupply SA,https://...,https://...,https://...</pre>



        <h6>Columnas — Proveedores</h6>

        <pre class="bg-light p-2 small mb-3">prov_nombre,prov_ruc,prov_telefono,prov_correo,prov_direccion</pre>



        <h6>Columnas — Categorías</h6>

        <pre class="bg-light p-2 small mb-3">cat_nombre</pre>



        <h6>Dónde conseguir datos de prueba</h6>

        <ul class="small mb-0">

            <li><a href="https://www.kaggle.com/datasets" target="_blank" rel="noopener">Kaggle Datasets</a> — busca "products csv" o "ecommerce products" y adapta columnas a la plantilla.</li>

            <li><a href="https://github.com/odoo/documentation/tree/master/sample_data" target="_blank" rel="noopener">Datos de ejemplo Odoo</a> — catálogos exportables a Excel.</li>

            <li><a href="https://picsum.photos/" target="_blank" rel="noopener">Lorem Picsum</a> — URLs de imágenes de prueba para columnas <code>pro_imagen_1</code>, etc.</li>

            <li>Usa las plantillas incluidas en <code>~/Plantillas/</code> del proyecto: ya traen filas listas para importar.</li>

        </ul>

    </div>

</asp:Content>


