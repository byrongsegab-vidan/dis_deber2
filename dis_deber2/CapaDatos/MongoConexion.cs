using System.Configuration;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public static class MongoConexion
    {
        private static IMongoDatabase _baseDatos;

        public static IMongoClient ObtenerCliente()
        {
            var cadena = ConfigurationManager.ConnectionStrings["cnMongo"].ConnectionString;
            return new MongoClient(cadena);
        }

        public static IMongoDatabase ObtenerBaseDatos()
        {
            if (_baseDatos != null)
                return _baseDatos;

            var nombreBd = ConfigurationManager.AppSettings["MongoDatabase"] ?? "dis_deber2";
            _baseDatos = ObtenerCliente().GetDatabase(nombreBd);
            return _baseDatos;
        }

        public static IMongoCollection<T> ObtenerColeccion<T>(string nombre)
        {
            return ObtenerBaseDatos().GetCollection<T>(nombre);
        }
    }
}
