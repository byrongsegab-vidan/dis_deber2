using System;
using System.Data;
using System.Linq;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class categoria
    {
        private static IMongoCollection<CategoriaDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<CategoriaDoc>("categorias");

        public int cat_id { get; set; }
        public string cat_nombre { get; set; }
        public string cat_estado { get; set; }

        public DataTable ListarActivas()
        {
            return Listar(string.Empty, false);
        }

        public DataTable Listar(string filtroNombre = "", bool incluirInactivos = true, bool ordenAlfabetico = false)
        {
            var lista = Coleccion.Find(_ => true).ToList()
                .Where(c => incluirInactivos || c.cat_estado == EstadoRegistro.Activo)
                .Where(c => Contiene(c.cat_nombre, filtroNombre));

            var ordenada = ordenAlfabetico
                ? lista.OrderBy(c => c.cat_nombre)
                : lista.OrderBy(c => c.cat_id);

            var dt = MongoTabla.Crear("cat_id", "cat_nombre", "cat_estado", "cat_fecha_baja");
            foreach (var doc in ordenada)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "cat_id", doc.cat_id);
                MongoTabla.Asignar(row, "cat_nombre", doc.cat_nombre);
                MongoTabla.Asignar(row, "cat_estado", doc.cat_estado);
                MongoTabla.Asignar(row, "cat_fecha_baja", doc.cat_fecha_baja);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public DataRow ObtenerPorId(int id)
        {
            var doc = Coleccion.Find(c => c.cat_id == id).FirstOrDefault();
            if (doc == null) return null;

            var dt = MongoTabla.Crear("cat_id", "cat_nombre", "cat_estado", "cat_fecha_baja");
            var row = dt.NewRow();
            MongoTabla.Asignar(row, "cat_id", doc.cat_id);
            MongoTabla.Asignar(row, "cat_nombre", doc.cat_nombre);
            MongoTabla.Asignar(row, "cat_estado", doc.cat_estado);
            MongoTabla.Asignar(row, "cat_fecha_baja", doc.cat_fecha_baja);
            dt.Rows.Add(row);
            return row;
        }

        public bool Insertar()
        {
            cat_id = MongoSecuencia.Siguiente("cat_id");
            Coleccion.InsertOne(new CategoriaDoc
            {
                cat_id = cat_id,
                cat_nombre = cat_nombre,
                cat_estado = EstadoRegistro.Activo
            });
            return true;
        }

        public bool Actualizar()
        {
            var result = Coleccion.UpdateOne(c => c.cat_id == cat_id,
                Builders<CategoriaDoc>.Update.Set(c => c.cat_nombre, cat_nombre));
            return result.ModifiedCount > 0;
        }

        public bool EliminarLogico()
        {
            var result = Coleccion.UpdateOne(c => c.cat_id == cat_id,
                Builders<CategoriaDoc>.Update
                    .Set(c => c.cat_estado, EstadoRegistro.Inactivo)
                    .Set(c => c.cat_fecha_baja, DateTime.Now));
            return result.ModifiedCount > 0;
        }

        public bool EliminarFisico()
        {
            return Coleccion.DeleteOne(c => c.cat_id == cat_id).DeletedCount > 0;
        }

        public bool Reactivar()
        {
            var result = Coleccion.UpdateOne(c => c.cat_id == cat_id,
                Builders<CategoriaDoc>.Update
                    .Set(c => c.cat_estado, EstadoRegistro.Activo)
                    .Set(c => c.cat_fecha_baja, null));
            return result.ModifiedCount > 0;
        }

        public bool CambiarEstado(string nuevoEstado)
        {
            if (nuevoEstado == EstadoRegistro.Activo) return Reactivar();
            if (nuevoEstado == EstadoRegistro.Inactivo) return EliminarLogico();
            return false;
        }

        public int? ObtenerIdPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            var doc = Coleccion.Find(c =>
                c.cat_estado == EstadoRegistro.Activo &&
                c.cat_nombre.ToLower() == nombre.Trim().ToLower()).FirstOrDefault();
            return doc?.cat_id;
        }

        public int BuscarOCrear(string nombre)
        {
            var existente = ObtenerIdPorNombre(nombre);
            if (existente.HasValue) return existente.Value;

            cat_nombre = nombre.Trim();
            Insertar();
            return cat_id;
        }

        private static bool Contiene(string texto, string filtro)
        {
            if (string.IsNullOrEmpty(filtro)) return true;
            return (texto ?? string.Empty).IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
