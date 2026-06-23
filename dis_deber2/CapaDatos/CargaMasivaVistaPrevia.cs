using System.Collections.Generic;
using System.Data;

namespace dis_deber2.CapaDatos
{
    public class CargaMasivaVistaPrevia
    {
        public DataTable Muestra { get; set; }
        public List<KeyValuePair<string, int>> ResumenHojas { get; set; } = new List<KeyValuePair<string, int>>();
        public int TotalFilasEstimadas { get; set; }
        public bool ModoRapidoSugerido { get; set; }
    }
}
