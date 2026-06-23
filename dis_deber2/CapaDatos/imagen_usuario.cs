using System.Data;
using System.Linq;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class imagen_usuario
    {
        private static IMongoCollection<ImagenUsuarioDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<ImagenUsuarioDoc>("imagenes_usuario");

        public int iusu_id { get; set; }
        public int usu_id { get; set; }
        public string iusu_ruta { get; set; }
        public bool iusu_es_principal { get; set; }

        public DataTable ListarPorUsuario(int usuarioId)
        {
            var dt = MongoTabla.Crear("iusu_id", "usu_id", "iusu_ruta", "iusu_es_principal");
            var lista = Coleccion.Find(i => i.usu_id == usuarioId && i.iusu_estado == EstadoRegistro.Activo)
                .ToList()
                .OrderByDescending(i => i.iusu_es_principal);

            foreach (var doc in lista)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "iusu_id", doc.iusu_id);
                MongoTabla.Asignar(row, "usu_id", doc.usu_id);
                MongoTabla.Asignar(row, "iusu_ruta", doc.iusu_ruta);
                MongoTabla.Asignar(row, "iusu_es_principal", doc.iusu_es_principal);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public string ObtenerRutaPrincipal(int usuarioId)
        {
            var doc = Coleccion.Find(i => i.usu_id == usuarioId && i.iusu_estado == EstadoRegistro.Activo && i.iusu_es_principal)
                .ToList()
                .OrderByDescending(i => i.iusu_id)
                .FirstOrDefault();
            return doc?.iusu_ruta;
        }

        public bool GuardarPrincipal(int usuarioId, string ruta)
        {
            Coleccion.UpdateMany(i => i.usu_id == usuarioId && i.iusu_estado == EstadoRegistro.Activo,
                Builders<ImagenUsuarioDoc>.Update.Set(i => i.iusu_es_principal, false));

            iusu_id = MongoSecuencia.Siguiente("iusu_id");
            Coleccion.InsertOne(new ImagenUsuarioDoc
            {
                iusu_id = iusu_id,
                usu_id = usuarioId,
                iusu_ruta = ruta,
                iusu_es_principal = true,
                iusu_estado = EstadoRegistro.Activo
            });
            return true;
        }

        public bool Insertar()
        {
            iusu_id = MongoSecuencia.Siguiente("iusu_id");
            Coleccion.InsertOne(new ImagenUsuarioDoc
            {
                iusu_id = iusu_id,
                usu_id = usu_id,
                iusu_ruta = iusu_ruta,
                iusu_es_principal = iusu_es_principal,
                iusu_estado = EstadoRegistro.Activo
            });
            return true;
        }
    }
}
