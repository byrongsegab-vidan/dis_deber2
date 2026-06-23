<%@ Page Title="Rombito" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Rombito.aspx.cs" Inherits="dis_deber2.Rombito" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-page-header">
        <h1>Generador de rombito</h1>
        <p>
            Contorno del rombo con capas interiores en <strong>espiral</strong>.
            Números <strong>pares</strong>: diagonales desde la <strong>izquierda</strong>.
            Números <strong>impares</strong>: diagonales desde la <strong>derecha</strong>.
        </p>
    </div>

    <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="alert d-block mb-3" />

    <div class="admin-glass-card mb-4">
        <div class="row g-3 align-items-end">
            <div class="col-md-4">
                <label class="form-label" for="<%= txtNumero.ClientID %>">Tamaño (n)</label>
                <asp:TextBox ID="txtNumero" runat="server" CssClass="form-control" TextMode="Number"
                    placeholder="1 – 20" min="1" max="20" step="1" ValidationGroup="vgRombo" />
                <small class="text-muted">Solo números enteros del 1 al 20.</small>
                <asp:RequiredFieldValidator ID="rfvNumero" runat="server" ControlToValidate="txtNumero"
                    ValidationGroup="vgRombo" ErrorMessage="Ingrese un número." CssClass="text-danger d-block"
                    Display="Dynamic" />
                <asp:RangeValidator ID="rvNumero" runat="server" ControlToValidate="txtNumero"
                    ValidationGroup="vgRombo" Type="Integer" MinimumValue="1" MaximumValue="20"
                    ErrorMessage="El tamaño debe estar entre 1 y 20." CssClass="text-danger d-block"
                    Display="Dynamic" />
            </div>
            <div class="col-md-4">
                <asp:Button ID="btnGenerar" runat="server" Text="Generar rombito" CssClass="btn btn-primary w-100"
                    OnClick="btnGenerar_Click" ValidationGroup="vgRombo" CausesValidation="true" />
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlResultado" runat="server" Visible="false" CssClass="admin-glass-card mb-4">
        <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">
            <h3 class="admin-card-title mb-0">Resultado</h3>
            <asp:Label ID="lblInfoRombo" runat="server" CssClass="badge rounded-pill text-bg-secondary" />
        </div>
        <pre class="rombo-salida mb-0"><asp:Literal ID="litRombo" runat="server" /></pre>
    </asp:Panel>

    <div class="admin-glass-card">
        <h3 class="admin-card-title mb-3">Mis figuras guardadas</h3>
        <div class="table-responsive">
            <asp:GridView ID="gvHistorial" runat="server" CssClass="table table-striped table-hover mb-0"
                AutoGenerateColumns="False" DataKeyNames="fig_id"
                OnRowCommand="gvHistorial_RowCommand" EmptyDataText="Aún no has generado figuras.">
                <Columns>
                    <asp:BoundField DataField="fig_id" HeaderText="#" ItemStyle-Width="60px" />
                    <asp:BoundField DataField="numero" HeaderText="n" ItemStyle-Width="60px" />
                    <asp:BoundField DataField="orientacion" HeaderText="Orientación" />
                    <asp:BoundField DataField="fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:TemplateField HeaderText="Acción" ItemStyle-Width="120px">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="ver" CommandArgument='<%# Eval("fig_id") %>' Text="Ver" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <style>
        .rombo-salida {
            font-family: Consolas, "Courier New", monospace;
            font-size: 13px;
            line-height: 1.2;
            white-space: pre;
            overflow-x: auto;
            background: rgba(255, 255, 255, 0.65);
            padding: 1rem;
            border-radius: 12px;
            border: 1px solid rgba(127, 86, 217, 0.15);
        }
    </style>
</asp:Content>
