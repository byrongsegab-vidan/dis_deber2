using System.Collections.Generic;
using System.Text;

namespace dis_deber2.CapaDatos
{
    public class CargaMasivaResultado
    {
        public int CategoriasInsertadas { get; set; }
        public int ProveedoresInsertados { get; set; }
        public int ProductosInsertados { get; set; }
        public int FilasProcesadas { get; set; }
        public int FilasOmitidas { get; set; }
        public int ImagenesDescargadas { get; set; }
        public bool ModoRapido { get; set; }
        public List<string> Advertencias { get; } = new List<string>();
        public List<string> Errores { get; } = new List<string>();

        public bool Exito => Errores.Count == 0;

        public string ResumenHtml()
        {
            var sb = new StringBuilder();
            sb.Append("<ul class=\"mb-0\">");
            sb.Append("<li><strong>Categorías:</strong> ").Append(CategoriasInsertadas).Append("</li>");
            sb.Append("<li><strong>Proveedores:</strong> ").Append(ProveedoresInsertados).Append("</li>");
            sb.Append("<li><strong>Productos:</strong> ").Append(ProductosInsertados).Append("</li>");
            if (FilasProcesadas > 0)
                sb.Append("<li><strong>Filas procesadas:</strong> ").Append(FilasProcesadas).Append("</li>");
            if (FilasOmitidas > 0)
                sb.Append("<li><strong>Filas omitidas:</strong> ").Append(FilasOmitidas).Append("</li>");
            if (ModoRapido)
                sb.Append("<li><strong>Modo:</strong> importación rápida (lotes, sin imágenes)</li>");
            sb.Append("<li><strong>Imágenes guardadas:</strong> ").Append(ImagenesDescargadas).Append("</li>");
            sb.Append("</ul>");

            if (Advertencias.Count > 0)
            {
                sb.Append("<p class=\"mt-2 mb-1\"><strong>Advertencias:</strong></p><ul>");
                foreach (var adv in Advertencias)
                    sb.Append("<li>").Append(adv).Append("</li>");
                sb.Append("</ul>");
            }

            if (Errores.Count > 0)
            {
                sb.Append("<p class=\"mt-2 mb-1 text-danger\"><strong>Errores:</strong></p><ul class=\"text-danger\">");
                foreach (var err in Errores)
                    sb.Append("<li>").Append(err).Append("</li>");
                sb.Append("</ul>");
            }

            return sb.ToString();
        }
    }
}
