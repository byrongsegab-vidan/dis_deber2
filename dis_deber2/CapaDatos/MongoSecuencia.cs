using MongoDB.Bson;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public static class MongoSecuencia
    {
        public static int Siguiente(string nombre)
        {
            var coll = MongoConexion.ObtenerColeccion<BsonDocument>("secuencias");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", nombre);
            var update = Builders<BsonDocument>.Update.Inc("valor", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            var doc = coll.FindOneAndUpdate(filter, update, options);
            return doc["valor"].AsInt32;
        }

        public static void FijarMinimo(string nombre, int minimo)
        {
            var coll = MongoConexion.ObtenerColeccion<BsonDocument>("secuencias");
            var doc = coll.Find(Builders<BsonDocument>.Filter.Eq("_id", nombre)).FirstOrDefault();
            if (doc == null || doc["valor"].AsInt32 < minimo)
            {
                coll.UpdateOne(
                    Builders<BsonDocument>.Filter.Eq("_id", nombre),
                    Builders<BsonDocument>.Update.Set("valor", minimo),
                    new UpdateOptions { IsUpsert = true });
            }
        }
    }
}
