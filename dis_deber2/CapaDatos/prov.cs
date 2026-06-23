using System;
using System.Data;
using System.Linq;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class prov
    {
        private static IMongoCollection<ProveedorDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<ProveedorDoc>("proveedores");

        private static IMongoCollection<ProductoDoc> Productos =>
            MongoConexion.ObtenerColeccion<ProductoDoc>("productos");

        public int prov_id { get; set; }
        public string prov_nombre { get; set; }
        public string prov_ruc { get; set; }
        public string prov_telefono { get; set; }
        public string prov_correo { get; set; }
        public string prov_direccion { get; set; }
        public string prov_estado { get; set; }

        public DataTable Listar(string filtro = "", bool incluirInactivos = false, bool ordenAlfabetico = false)
        {
            var lista = Coleccion.Find(_ => true).ToList()
                .Where(p => incluirInactivos || p.prov_estado == EstadoRegistro.Activo)
                .Where(p => Contiene(p.prov_nombre, filtro) || Contiene(p.prov_ruc, filtro));

            var ordenada = ordenAlfabetico
                ? lista.OrderBy(p => p.prov_nombre)
                : lista.OrderBy(p => p.prov_id);

            var dt = MongoTabla.Crear("prov_id", "prov_nombre", "prov_ruc", "prov_telefono", "prov_correo", "prov_direccion", "prov_estado", "prov_fecha_baja");
            foreach (var doc in ordenada)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "prov_id", doc.prov_id);
                MongoTabla.Asignar(row, "prov_nombre", doc.prov_nombre);
                MongoTabla.Asignar(row, "prov_ruc", doc.prov_ruc);
                MongoTabla.Asignar(row, "prov_telefono", doc.prov_telefono);
                MongoTabla.Asignar(row, "prov_correo", doc.prov_correo);
                MongoTabla.Asignar(row, "prov_direccion", doc.prov_direccion);
                MongoTabla.Asignar(row, "prov_estado", doc.prov_estado);
                MongoTabla.Asignar(row, "prov_fecha_baja", doc.prov_fecha_baja);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public DataTable ListarActivos()
        {
            return Listar(string.Empty, false);
        }

        public bool Insertar()
        {
            prov_id = MongoSecuencia.Siguiente("prov_id");
            Coleccion.InsertOne(new ProveedorDoc
            {
                prov_id = prov_id,
                prov_nombre = prov_nombre,
                prov_ruc = prov_ruc,
                prov_telefono = prov_telefono,
                prov_correo = prov_correo,
                prov_direccion = prov_direccion,
                prov_estado = EstadoRegistro.Activo
            });
            return true;
        }

        public bool Actualizar()
        {
            var result = Coleccion.UpdateOne(p => p.prov_id == prov_id,
                Builders<ProveedorDoc>.Update
                    .Set(p => p.prov_nombre, prov_nombre)
                    .Set(p => p.prov_ruc, prov_ruc)
                    .Set(p => p.prov_telefono, prov_telefono)
                    .Set(p => p.prov_correo, prov_correo)
                    .Set(p => p.prov_direccion, prov_direccion));
            return result.ModifiedCount > 0;
        }

        public bool EliminarLogico()
        {
            var result = Coleccion.UpdateOne(p => p.prov_id == prov_id,
                Builders<ProveedorDoc>.Update
                    .Set(p => p.prov_estado, EstadoRegistro.Inactivo)
                    .Set(p => p.prov_fecha_baja, DateTime.Now));
            return result.ModifiedCount > 0;
        }

        public bool EliminarFisico()
        {
            Productos.UpdateMany(p => p.prov_id == prov_id,
                Builders<ProductoDoc>.Update
                    .Set(p => p.prov_id_respaldo, prov_id)
                    .Set(p => p.prov_id, null));
            return Coleccion.DeleteOne(p => p.prov_id == prov_id).DeletedCount > 0;
        }

        public bool Reactivar()
        {
            var result = Coleccion.UpdateOne(p => p.prov_id == prov_id,
                Builders<ProveedorDoc>.Update
                    .Set(p => p.prov_estado, EstadoRegistro.Activo)
                    .Set(p => p.prov_fecha_baja, null));
            return result.ModifiedCount > 0;
        }

        public bool RestaurarProductosHijos()
        {
            var result = Productos.UpdateMany(p => p.prov_id_respaldo == prov_id,
                Builders<ProductoDoc>.Update
                    .Set(p => p.prov_id, prov_id)
                    .Set(p => p.prov_id_respaldo, null));
            return result.ModifiedCount >= 0;
        }

        public int? ObtenerIdPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            var doc = Coleccion.Find(p =>
                p.prov_estado == EstadoRegistro.Activo &&
                p.prov_nombre.ToLower() == nombre.Trim().ToLower()).FirstOrDefault();
            return doc?.prov_id;
        }

        public int BuscarOCrear(string nombre)
        {
            var existente = ObtenerIdPorNombre(nombre);
            if (existente.HasValue) return existente.Value;

            prov_nombre = nombre.Trim();
            Insertar();
            return prov_id;
        }

        private static bool Contiene(string texto, string filtro)
        {
            if (string.IsNullOrEmpty(filtro)) return true;
            return (texto ?? string.Empty).IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
