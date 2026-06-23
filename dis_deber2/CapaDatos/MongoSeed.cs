using System;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public static class MongoSeed
    {
        public static void InicializarSiVacio()
        {
            var usuarios = MongoConexion.ObtenerColeccion<UsuarioDoc>("usuarios");
            if (usuarios.CountDocuments(FilterDefinition<UsuarioDoc>.Empty) > 0)
                return;

            InsertarUsuarios(usuarios);
            InsertarCategorias();
            InsertarProveedores();
            InsertarProductos();

            MongoSecuencia.FijarMinimo("usu_id", 2);
            MongoSecuencia.FijarMinimo("cat_id", 5);
            MongoSecuencia.FijarMinimo("prov_id", 2);
            MongoSecuencia.FijarMinimo("pro_id", 5);
        }

        private static void InsertarUsuarios(IMongoCollection<UsuarioDoc> coll)
        {
            coll.InsertMany(new[]
            {
                new UsuarioDoc
                {
                    usu_id = 1,
                    usu_cedula = "1720000001",
                    usu_nombres = "Byron",
                    usu_apellidos = "Segab",
                    usu_correo = "byrong.segab@gmail.com",
                    usu_nick = "byron.admin",
                    usu_contrasena = "Admin123",
                    tusu_id = MongoRoles.Administrador,
                    usu_estado = EstadoRegistro.Activo
                },
                new UsuarioDoc
                {
                    usu_id = 2,
                    usu_cedula = "1720000002",
                    usu_nombres = "Jadan",
                    usu_apellidos = "Vinicio",
                    usu_correo = "jadan.vinicio77@gmail.com",
                    usu_nick = "jadan.user",
                    usu_contrasena = "User123",
                    tusu_id = MongoRoles.Cliente,
                    usu_estado = EstadoRegistro.Activo
                }
            });
        }

        private static void InsertarCategorias()
        {
            var coll = MongoConexion.ObtenerColeccion<CategoriaDoc>("categorias");
            var nombres = new[] { "Electrónica", "Ropa", "Alimentos", "Hogar", "Deportes" };
            for (var i = 0; i < nombres.Length; i++)
            {
                coll.InsertOne(new CategoriaDoc
                {
                    cat_id = i + 1,
                    cat_nombre = nombres[i],
                    cat_estado = EstadoRegistro.Activo
                });
            }
        }

        private static void InsertarProveedores()
        {
            var coll = MongoConexion.ObtenerColeccion<ProveedorDoc>("proveedores");
            coll.InsertMany(new[]
            {
                new ProveedorDoc
                {
                    prov_id = 1,
                    prov_nombre = "TechSupply SA",
                    prov_ruc = "1790123456001",
                    prov_telefono = "0991234567",
                    prov_correo = "ventas@techsupply.com",
                    prov_direccion = "Quito",
                    prov_estado = EstadoRegistro.Activo
                },
                new ProveedorDoc
                {
                    prov_id = 2,
                    prov_nombre = "ModaExpress",
                    prov_ruc = "1790987654001",
                    prov_telefono = "0987654321",
                    prov_correo = "info@modaexpress.com",
                    prov_direccion = "Guayaquil",
                    prov_estado = EstadoRegistro.Activo
                }
            });
        }

        private static void InsertarProductos()
        {
            var coll = MongoConexion.ObtenerColeccion<ProductoDoc>("productos");
            coll.InsertMany(new[]
            {
                new ProductoDoc { pro_id = 1, pro_nombre = "Laptop HP 15", pro_descripcion = "Laptop 8GB RAM 256GB SSD", pro_precio = 650.00m, pro_stock = 25, cat_id = 1, prov_id = 1 },
                new ProductoDoc { pro_id = 2, pro_nombre = "Camiseta Nike", pro_descripcion = "Camiseta deportiva talla M", pro_precio = 35.50m, pro_stock = 100, cat_id = 2, prov_id = 2 },
                new ProductoDoc { pro_id = 3, pro_nombre = "Arroz Premium 5kg", pro_descripcion = "Arroz de grano largo", pro_precio = 4.80m, pro_stock = 500, cat_id = 3, prov_id = 1 },
                new ProductoDoc { pro_id = 4, pro_nombre = "Silla Ergonómica", pro_descripcion = "Silla de oficina ajustable", pro_precio = 120.00m, pro_stock = 40, cat_id = 4, prov_id = 1 },
                new ProductoDoc { pro_id = 5, pro_nombre = "Balón Fútbol", pro_descripcion = "Balón oficial talla 5", pro_precio = 28.00m, pro_stock = 80, cat_id = 5, prov_id = 2 }
            });
        }
    }
}
