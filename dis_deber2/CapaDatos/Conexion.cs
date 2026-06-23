using System.Configuration;
using System.Data.SqlClient;

namespace dis_deber2.CapaDatos
{
    public static class Conexion
    {
        public static SqlConnection ObtenerConexion()
        {
            var cadena = ConfigurationManager.ConnectionStrings["cnDisDeber2"].ConnectionString;
            return new SqlConnection(cadena);
        }
    }
}
