using System;
using System.Data;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class usuario
    {
        private static IMongoCollection<UsuarioDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<UsuarioDoc>("usuarios");

        public int usu_id { get; set; }
        public string usu_correo { get; set; }
        public string usu_nombres { get; set; }
        public string usu_apellidos { get; set; }
        public string usu_nick { get; set; }
        public int? tusu_id { get; set; }
        public string usu_cedula { get; set; }
        public string usu_contrasena { get; set; }
        public string usu_direccion { get; set; }
        public string usu_celular { get; set; }
        public DateTime? usu_fecha_cumple { get; set; }

        public bool CorreoExiste(string correo)
        {
            return Coleccion.CountDocuments(u => u.usu_correo == correo) > 0;
        }

        public int? RegistrarCliente()
        {
            if (CorreoExiste(usu_correo))
                return null;

            usu_id = MongoSecuencia.Siguiente("usu_id");
            var doc = new UsuarioDoc
            {
                usu_id = usu_id,
                usu_cedula = usu_cedula ?? string.Empty,
                usu_nombres = usu_nombres,
                usu_apellidos = usu_apellidos,
                usu_correo = usu_correo,
                usu_nick = usu_nick ?? usu_correo.Split('@')[0],
                usu_contrasena = usu_contrasena,
                tusu_id = MongoRoles.Cliente,
                usu_estado = EstadoRegistro.Activo
            };
            Coleccion.InsertOne(doc);
            return usu_id;
        }

        public DataRow ObtenerPorId(int id)
        {
            var doc = Coleccion.Find(u => u.usu_id == id && u.usu_estado == EstadoRegistro.Activo).FirstOrDefault();
            if (doc == null) return null;

            var dt = MongoTabla.Crear("usu_id", "usu_cedula", "usu_nombres", "usu_apellidos", "usu_direccion",
                "usu_celular", "usu_correo", "usu_fecha_cumple", "usu_nick", "tusu_id", "tusu_nombre");
            var row = dt.NewRow();
            MongoTabla.Asignar(row, "usu_id", doc.usu_id);
            MongoTabla.Asignar(row, "usu_cedula", doc.usu_cedula);
            MongoTabla.Asignar(row, "usu_nombres", doc.usu_nombres);
            MongoTabla.Asignar(row, "usu_apellidos", doc.usu_apellidos);
            MongoTabla.Asignar(row, "usu_direccion", doc.usu_direccion);
            MongoTabla.Asignar(row, "usu_celular", doc.usu_celular);
            MongoTabla.Asignar(row, "usu_correo", doc.usu_correo);
            MongoTabla.Asignar(row, "usu_fecha_cumple", doc.usu_fecha_cumple);
            MongoTabla.Asignar(row, "usu_nick", doc.usu_nick);
            MongoTabla.Asignar(row, "tusu_id", doc.tusu_id);
            MongoTabla.Asignar(row, "tusu_nombre", MongoRoles.Nombre(doc.tusu_id));
            dt.Rows.Add(row);
            return row;
        }

        public bool ActualizarPerfil()
        {
            var update = Builders<UsuarioDoc>.Update
                .Set(u => u.usu_cedula, usu_cedula ?? string.Empty)
                .Set(u => u.usu_nombres, usu_nombres)
                .Set(u => u.usu_apellidos, usu_apellidos)
                .Set(u => u.usu_nick, usu_nick)
                .Set(u => u.usu_direccion, usu_direccion)
                .Set(u => u.usu_celular, usu_celular)
                .Set(u => u.usu_fecha_cumple, usu_fecha_cumple);

            var result = Coleccion.UpdateOne(u => u.usu_id == usu_id && u.usu_estado == EstadoRegistro.Activo, update);
            return result.ModifiedCount > 0;
        }

        public int? ValidarCedulaCorreo(string cedula, string correo)
        {
            var doc = Coleccion.Find(u =>
                u.usu_cedula == cedula &&
                u.usu_correo == correo &&
                (u.usu_estado == EstadoRegistro.Activo || u.usu_estado == EstadoRegistro.Bloqueado))
                .FirstOrDefault();
            return doc?.usu_id;
        }

        public string GenerarOtp(string correo, string tipo)
        {
            var doc = Coleccion.Find(u =>
                u.usu_correo == correo &&
                (u.usu_estado == EstadoRegistro.Activo || u.usu_estado == EstadoRegistro.Bloqueado))
                .FirstOrDefault();
            if (doc == null) return null;

            var otp = new Random().Next(0, 1000000).ToString("D6");
            var update = Builders<UsuarioDoc>.Update
                .Set(u => u.usu_codigo_OTP, otp)
                .Set(u => u.usu_otp_tipo, tipo)
                .Set(u => u.usu_otp_expira, DateTime.Now.AddMinutes(10));

            Coleccion.UpdateOne(u => u.usu_id == doc.usu_id, update);
            return otp;
        }

        public int? ValidarOtp(string correo, string otp, string tipo)
        {
            var doc = Coleccion.Find(u =>
                u.usu_correo == correo &&
                u.usu_codigo_OTP == otp &&
                u.usu_otp_tipo == tipo &&
                u.usu_otp_expira >= DateTime.Now &&
                (u.usu_estado == EstadoRegistro.Activo || u.usu_estado == EstadoRegistro.Bloqueado))
                .FirstOrDefault();
            if (doc == null) return null;

            Coleccion.UpdateOne(u => u.usu_id == doc.usu_id,
                Builders<UsuarioDoc>.Update
                    .Set(u => u.usu_codigo_OTP, null)
                    .Set(u => u.usu_otp_tipo, null)
                    .Set(u => u.usu_otp_expira, null));

            return doc.usu_id;
        }

        public bool CambiarContrasena(int usuId, string nuevaClave)
        {
            var update = Builders<UsuarioDoc>.Update
                .Set(u => u.usu_contrasena, nuevaClave)
                .Set(u => u.usu_codigo_OTP, null)
                .Set(u => u.usu_otp_tipo, null)
                .Set(u => u.usu_otp_expira, null)
                .Set(u => u.usu_intentos, 0)
                .Set(u => u.usu_estado, EstadoRegistro.Activo);

            var result = Coleccion.UpdateOne(u =>
                u.usu_id == usuId &&
                (u.usu_estado == EstadoRegistro.Activo || u.usu_estado == EstadoRegistro.Bloqueado),
                update);
            return result.ModifiedCount > 0;
        }

        public class EstadoLogin
        {
            public int? UsuId { get; set; }
            public string Estado { get; set; }
            public int Intentos { get; set; }
            public bool Existe => UsuId.HasValue;
            public bool Bloqueada => Estado == EstadoRegistro.Bloqueado;
        }

        public EstadoLogin ObtenerEstadoLogin(string correo)
        {
            var doc = Coleccion.Find(u =>
                u.usu_correo == correo &&
                (u.usu_estado == EstadoRegistro.Activo || u.usu_estado == EstadoRegistro.Bloqueado))
                .FirstOrDefault();

            if (doc == null) return new EstadoLogin();

            return new EstadoLogin
            {
                UsuId = doc.usu_id,
                Estado = doc.usu_estado,
                Intentos = doc.usu_intentos
            };
        }

        public bool RegistrarIntentoFallido(string correo, out int intentos, out bool bloqueada)
        {
            var doc = Coleccion.Find(u => u.usu_correo == correo).FirstOrDefault();
            if (doc == null)
            {
                intentos = 0;
                bloqueada = false;
                return false;
            }

            intentos = doc.usu_intentos + 1;
            bloqueada = intentos >= 3;
            var update = Builders<UsuarioDoc>.Update.Set(u => u.usu_intentos, intentos);
            if (bloqueada)
                update = update.Set(u => u.usu_estado, EstadoRegistro.Bloqueado);

            Coleccion.UpdateOne(u => u.usu_id == doc.usu_id, update);
            return true;
        }

        public void LoginExitoso(string correo)
        {
            Coleccion.UpdateOne(u => u.usu_correo == correo && u.usu_estado == EstadoRegistro.Activo,
                Builders<UsuarioDoc>.Update.Set(u => u.usu_intentos, 0));
        }

        public DataRow ValidarLogin(string correo, string contrasena)
        {
            var estado = ObtenerEstadoLogin(correo);
            if (!estado.Existe || estado.Bloqueada)
                return null;

            var doc = Coleccion.Find(u => u.usu_correo == correo && u.usu_estado == EstadoRegistro.Activo).FirstOrDefault();
            if (doc == null || doc.usu_contrasena != contrasena)
                return null;

            LoginExitoso(correo);

            var dt = MongoTabla.Crear("usu_id", "usu_correo", "usu_nombres", "usu_apellidos", "usu_nick",
                "tusu_id", "tusu_nombre", "clave");
            var row = dt.NewRow();
            MongoTabla.Asignar(row, "usu_id", doc.usu_id);
            MongoTabla.Asignar(row, "usu_correo", doc.usu_correo);
            MongoTabla.Asignar(row, "usu_nombres", doc.usu_nombres);
            MongoTabla.Asignar(row, "usu_apellidos", doc.usu_apellidos);
            MongoTabla.Asignar(row, "usu_nick", doc.usu_nick);
            MongoTabla.Asignar(row, "tusu_id", doc.tusu_id);
            MongoTabla.Asignar(row, "tusu_nombre", MongoRoles.Nombre(doc.tusu_id));
            MongoTabla.Asignar(row, "clave", doc.usu_contrasena);
            dt.Rows.Add(row);
            return row;
        }

        public DataTable Listar()
        {
            var dt = MongoTabla.Crear("usu_id", "usu_correo", "usu_nombres", "usu_apellidos", "usu_nick", "tusu_nombre");
            foreach (var doc in Coleccion.Find(u => u.usu_estado == EstadoRegistro.Activo).ToList())
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "usu_id", doc.usu_id);
                MongoTabla.Asignar(row, "usu_correo", doc.usu_correo);
                MongoTabla.Asignar(row, "usu_nombres", doc.usu_nombres);
                MongoTabla.Asignar(row, "usu_apellidos", doc.usu_apellidos);
                MongoTabla.Asignar(row, "usu_nick", doc.usu_nick);
                MongoTabla.Asignar(row, "tusu_nombre", MongoRoles.Nombre(doc.tusu_id));
                dt.Rows.Add(row);
            }
            return dt;
        }

        public bool EliminarLogico()
        {
            var result = Coleccion.UpdateOne(u => u.usu_id == usu_id,
                Builders<UsuarioDoc>.Update.Set(u => u.usu_estado, EstadoRegistro.Inactivo));
            return result.ModifiedCount > 0;
        }

        public bool EliminarFisico()
        {
            var result = Coleccion.DeleteOne(u => u.usu_id == usu_id);
            return result.DeletedCount > 0;
        }

        public bool Reactivar()
        {
            var result = Coleccion.UpdateOne(u => u.usu_id == usu_id,
                Builders<UsuarioDoc>.Update
                    .Set(u => u.usu_estado, EstadoRegistro.Activo)
                    .Set(u => u.usu_intentos, 0));
            return result.ModifiedCount > 0;
        }
    }
}
