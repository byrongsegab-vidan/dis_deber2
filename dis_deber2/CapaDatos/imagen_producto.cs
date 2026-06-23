using System;
using System.Data;
using System.Linq;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class imagen_producto
    {
        private static IMongoCollection<ImagenProductoDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<ImagenProductoDoc>("imagenes_producto");

        public int ipro_id { get; set; }
        public int pro_id { get; set; }
        public string ipro_ruta { get; set; }
        public bool ipro_es_principal { get; set; }

        public DataTable ListarPorProducto(int productoId)
        {
            var dt = MongoTabla.Crear("ipro_id", "pro_id", "ipro_ruta", "ipro_es_principal");
            var lista = Coleccion.Find(i => i.pro_id == productoId && i.ipro_estado == EstadoRegistro.Activo)
                .ToList()
                .OrderByDescending(i => i.ipro_es_principal)
                .ThenBy(i => i.ipro_id);

            foreach (var doc in lista)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "ipro_id", doc.ipro_id);
                MongoTabla.Asignar(row, "pro_id", doc.pro_id);
                MongoTabla.Asignar(row, "ipro_ruta", doc.ipro_ruta);
                MongoTabla.Asignar(row, "ipro_es_principal", doc.ipro_es_principal);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public int ContarActivas(int productoId)
        {
            return (int)Coleccion.CountDocuments(i => i.pro_id == productoId && i.ipro_estado == EstadoRegistro.Activo);
        }

        public bool Insertar()
        {
            ipro_id = MongoSecuencia.Siguiente("ipro_id");
            Coleccion.InsertOne(new ImagenProductoDoc
            {
                ipro_id = ipro_id,
                pro_id = pro_id,
                ipro_ruta = ipro_ruta,
                ipro_es_principal = ipro_es_principal,
                ipro_estado = EstadoRegistro.Activo
            });
            return true;
        }

        public bool EliminarLogico(int iproId)
        {
            var result = Coleccion.UpdateOne(i => i.ipro_id == iproId && i.ipro_estado == EstadoRegistro.Activo,
                Builders<ImagenProductoDoc>.Update
                    .Set(i => i.ipro_estado, EstadoRegistro.Inactivo)
                    .Set(i => i.ipro_fecha_baja, DateTime.Now));
            return result.ModifiedCount > 0;
        }
    }
}
