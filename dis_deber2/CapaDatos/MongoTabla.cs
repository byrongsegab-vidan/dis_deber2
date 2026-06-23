using System;
using System.Data;

namespace dis_deber2.CapaDatos
{
    public static class MongoTabla
    {
        public static DataTable Crear(params string[] columnas)
        {
            var dt = new DataTable();
            foreach (var col in columnas)
                dt.Columns.Add(col);
            return dt;
        }

        public static void Asignar(DataRow row, string columna, object valor)
        {
            row[columna] = valor ?? DBNull.Value;
        }
    }
}
