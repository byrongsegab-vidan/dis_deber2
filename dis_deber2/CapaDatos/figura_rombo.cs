using System;
using System.Collections.Generic;
using System.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class FiguraRomboDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int fig_id { get; set; }
        public int usu_id { get; set; }
        public int numero { get; set; }
        public bool esPar { get; set; }
        public string orientacion { get; set; }
        public string patron { get; set; }
        public DateTime fecha { get; set; }
    }

    public class figura_rombo
    {
        private static IMongoCollection<FiguraRomboDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<FiguraRomboDoc>("figuras");

        public int Guardar(int usuId, int numero, bool esPar, string orientacion, string patron)
        {
            var doc = new FiguraRomboDoc
            {
                fig_id = MongoSecuencia.Siguiente("fig_id"),
                usu_id = usuId,
                numero = numero,
                esPar = esPar,
                orientacion = orientacion,
                patron = patron,
                fecha = DateTime.Now
            };
            Coleccion.InsertOne(doc);
            return doc.fig_id;
        }

        public DataTable ListarPorUsuario(int usuId)
        {
            var dt = MongoTabla.Crear("fig_id", "numero", "orientacion", "fecha", "patron");
            var lista = Coleccion.Find(f => f.usu_id == usuId)
                .SortByDescending(f => f.fecha)
                .Limit(20)
                .ToList();

            foreach (var doc in lista)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "fig_id", doc.fig_id);
                MongoTabla.Asignar(row, "numero", doc.numero);
                MongoTabla.Asignar(row, "orientacion", doc.orientacion);
                MongoTabla.Asignar(row, "fecha", doc.fecha);
                MongoTabla.Asignar(row, "patron", doc.patron);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public string ObtenerPatron(int figId, int usuId)
        {
            var doc = Coleccion.Find(f => f.fig_id == figId && f.usu_id == usuId).FirstOrDefault();
            return doc?.patron;
        }
    }
}
