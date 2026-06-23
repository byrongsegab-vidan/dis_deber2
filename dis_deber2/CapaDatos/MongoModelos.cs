using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dis_deber2.CapaDatos
{
    public class UsuarioDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int usu_id { get; set; }
        public string usu_correo { get; set; }
        public string usu_nombres { get; set; }
        public string usu_apellidos { get; set; }
        public string usu_nick { get; set; }
        public int tusu_id { get; set; }
        public string usu_cedula { get; set; }
        public string usu_contrasena { get; set; }
        public string usu_direccion { get; set; }
        public string usu_celular { get; set; }
        public DateTime? usu_fecha_cumple { get; set; }
        public string usu_estado { get; set; } = EstadoRegistro.Activo;
        public int usu_intentos { get; set; }
        public string usu_codigo_OTP { get; set; }
        public string usu_otp_tipo { get; set; }
        public DateTime? usu_otp_expira { get; set; }
    }

    public class CategoriaDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int cat_id { get; set; }
        public string cat_nombre { get; set; }
        public string cat_estado { get; set; } = EstadoRegistro.Activo;
        public DateTime? cat_fecha_baja { get; set; }
    }

    public class ProveedorDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int prov_id { get; set; }
        public string prov_nombre { get; set; }
        public string prov_ruc { get; set; }
        public string prov_telefono { get; set; }
        public string prov_correo { get; set; }
        public string prov_direccion { get; set; }
        public string prov_estado { get; set; } = EstadoRegistro.Activo;
        public DateTime? prov_fecha_baja { get; set; }
    }

    public class ProductoDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int pro_id { get; set; }
        public string pro_nombre { get; set; }
        public string pro_descripcion { get; set; }
        public decimal pro_precio { get; set; }
        public int pro_stock { get; set; }
        public string pro_imagen_ruta { get; set; }
        public string pro_estado { get; set; } = EstadoRegistro.Activo;
        public int? cat_id { get; set; }
        public int? prov_id { get; set; }
        public int? prov_id_respaldo { get; set; }
        public DateTime? pro_fecha_baja { get; set; }
    }

    public class ImagenUsuarioDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int iusu_id { get; set; }
        public int usu_id { get; set; }
        public string iusu_ruta { get; set; }
        public bool iusu_es_principal { get; set; }
        public string iusu_estado { get; set; } = EstadoRegistro.Activo;
    }

    public class ImagenProductoDoc
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int ipro_id { get; set; }
        public int pro_id { get; set; }
        public string ipro_ruta { get; set; }
        public bool ipro_es_principal { get; set; }
        public string ipro_estado { get; set; } = EstadoRegistro.Activo;
        public DateTime? ipro_fecha_baja { get; set; }
    }

    public static class MongoRoles
    {
        public const int Administrador = 1;
        public const int Cliente = 2;

        public static string Nombre(int tusuId)
        {
            return tusuId == Administrador ? "Administrador" : "Cliente";
        }
    }
}
