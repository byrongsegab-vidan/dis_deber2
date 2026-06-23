<%@ Page Title="Estadísticas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Estadisticas.aspx.cs" Inherits="dis_deber2.Estadisticas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Estadísticas de productos</h2>

    <div class="row mb-4">
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">Productos por categoría</div>
                <div class="card-body"><canvas id="chartCategorias" height="200"></canvas></div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">Stock total por categoría</div>
                <div class="card-body"><canvas id="chartStock" height="200"></canvas></div>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlProducto" runat="server" Visible="false">
        <h4><asp:Label ID="lblProducto" runat="server" /></h4>
        <div id="carouselProducto" class="carousel slide producto-carousel mb-4" data-bs-ride="carousel">
            <div class="carousel-inner">
                <asp:Repeater ID="rptCarrusel" runat="server">
                    <ItemTemplate>
                        <div class='carousel-item <%# (bool)Eval("activo") ? "active" : "" %>'>
                            <img src='<%# Eval("url") %>' class="d-block w-100 rounded" alt="Imagen producto" style="max-height:400px;object-fit:contain;" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <button class="carousel-control-prev" type="button" data-bs-target="#carouselProducto" data-bs-slide="prev">
                <span class="carousel-control-prev-icon"></span>
            </button>
            <button class="carousel-control-next" type="button" data-bs-target="#carouselProducto" data-bs-slide="next">
                <span class="carousel-control-next-icon"></span>
            </button>
        </div>
    </asp:Panel>

    <asp:HiddenField ID="hfChartLabels" runat="server" />
    <asp:HiddenField ID="hfChartTotales" runat="server" />
    <asp:HiddenField ID="hfChartStock" runat="server" />

    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var labels = (document.getElementById('<%= hfChartLabels.ClientID %>').value || '').split('|');
            var totales = (document.getElementById('<%= hfChartTotales.ClientID %>').value || '').split('|').map(Number);
            var stock = (document.getElementById('<%= hfChartStock.ClientID %>').value || '').split('|').map(Number);
            if (labels.length && labels[0]) {
                new Chart(document.getElementById('chartCategorias'), { type: 'bar', data: { labels: labels, datasets: [{ label: 'Productos', data: totales, backgroundColor: '#0d6efd' }] } });
                new Chart(document.getElementById('chartStock'), { type: 'pie', data: { labels: labels, datasets: [{ data: stock, backgroundColor: ['#0d6efd', '#198754', '#ffc107', '#dc3545', '#6f42c1'] }] } });
            }
        });
    </script>
</asp:Content>
