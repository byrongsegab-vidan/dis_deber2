using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MongoDB.Driver;

namespace dis_deber2.CapaDatos
{
    public class pro
    {
        private static IMongoCollection<ProductoDoc> Coleccion =>
            MongoConexion.ObtenerColeccion<ProductoDoc>("productos");

        private static IMongoCollection<CategoriaDoc> Categorias =>
            MongoConexion.ObtenerColeccion<CategoriaDoc>("categorias");

        private static IMongoCollection<ProveedorDoc> Proveedores =>
            MongoConexion.ObtenerColeccion<ProveedorDoc>("proveedores");

        private static IMongoCollection<ImagenProductoDoc> Imagenes =>
            MongoConexion.ObtenerColeccion<ImagenProductoDoc>("imagenes_producto");

        public const int MaxNombreLength = 500;
        public const int MaxDescripcionLength = 500;

        public int pro_id { get; set; }
        public string pro_nombre { get; set; }
        public string pro_descripcion { get; set; }
        public decimal pro_precio { get; set; }
        public int pro_stock { get; set; }
        public string pro_imagen_ruta { get; set; }
        public string pro_estado { get; set; }
        public int? cat_id { get; set; }
        public int? prov_id { get; set; }
        public int? prov_id_respaldo { get; set; }

        public DataTable ListarPaginado(string filtroNombre, int? categoriaId, int? proveedorId, decimal? precioMin, decimal? precioMax, int pagina, int tamano, out int total, bool incluirInactivos = true, bool ordenAlfabetico = false)
        {
            var cats = Categorias.Find(_ => true).ToList().ToDictionary(c => c.cat_id, c => c.cat_nombre);
            var provs = Proveedores.Find(_ => true).ToList().ToDictionary(p => p.prov_id, p => p.prov_nombre);

            var filtrados = FiltrarProductos(filtroNombre, categoriaId, proveedorId, precioMin, precioMax, incluirInactivos);
            total = filtrados.Count;

            var paginados = (ordenAlfabetico
                    ? filtrados.OrderBy(p => p.pro_nombre)
                    : filtrados.OrderBy(p => p.pro_id))
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList();

            return AProductoDataTable(paginados, cats, provs);
        }

        public bool ActualizarImagenPrincipal(int productoId)
        {
            var img = Imagenes.Find(i => i.pro_id == productoId && i.ipro_estado == EstadoRegistro.Activo)
                .ToList()
                .OrderByDescending(i => i.ipro_es_principal)
                .ThenBy(i => i.ipro_id)
                .FirstOrDefault();

            var ruta = img?.ipro_ruta;
            var result = Coleccion.UpdateOne(p => p.pro_id == productoId,
                Builders<ProductoDoc>.Update.Set(p => p.pro_imagen_ruta, ruta));
            return result.ModifiedCount > 0;
        }

        public bool CambiarEstado(string nuevoEstado)
        {
            if (nuevoEstado == EstadoRegistro.Activo) return Reactivar();
            if (nuevoEstado == EstadoRegistro.Inactivo) return EliminarLogico();
            return false;
        }

        public DataTable BusquedaRapida(string texto, int? categoriaId, decimal? precioMin, decimal? precioMax, bool ordenAlfabetico = false)
        {
            var cats = Categorias.Find(_ => true).ToList().ToDictionary(c => c.cat_id, c => c.cat_nombre);
            var provs = Proveedores.Find(_ => true).ToList().ToDictionary(p => p.prov_id, p => p.prov_nombre);

            var filtrados = FiltrarProductos(texto, categoriaId, null, precioMin, precioMax, false);
            var lista = (ordenAlfabetico ? filtrados.OrderBy(p => p.pro_nombre) : filtrados.OrderBy(p => p.pro_id))
                .Take(50)
                .ToList();

            var dt = MongoTabla.Crear("pro_id", "pro_nombre", "pro_precio", "pro_stock", "pro_imagen_ruta", "cat_nombre", "prov_nombre");
            foreach (var p in lista)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "pro_id", p.pro_id);
                MongoTabla.Asignar(row, "pro_nombre", p.pro_nombre);
                MongoTabla.Asignar(row, "pro_precio", p.pro_precio);
                MongoTabla.Asignar(row, "pro_stock", p.pro_stock);
                MongoTabla.Asignar(row, "pro_imagen_ruta", p.pro_imagen_ruta);
                MongoTabla.Asignar(row, "cat_nombre", p.cat_id.HasValue && cats.ContainsKey(p.cat_id.Value) ? cats[p.cat_id.Value] : null);
                MongoTabla.Asignar(row, "prov_nombre", p.prov_id.HasValue && provs.ContainsKey(p.prov_id.Value) ? provs[p.prov_id.Value] : null);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public DataTable EstadisticasPorCategoria()
        {
            var cats = Categorias.Find(_ => true).ToList().ToDictionary(c => c.cat_id, c => c.cat_nombre);
            var activos = Coleccion.Find(p => p.pro_estado == EstadoRegistro.Activo).ToList();

            var dt = MongoTabla.Crear("categoria", "total_productos", "stock_total", "precio_promedio");
            var grupos = activos.GroupBy(p => p.cat_id);
            foreach (var g in grupos)
            {
                var nombre = g.Key.HasValue && cats.ContainsKey(g.Key.Value) ? cats[g.Key.Value] : "Sin categoría";
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "categoria", nombre);
                MongoTabla.Asignar(row, "total_productos", g.Count());
                MongoTabla.Asignar(row, "stock_total", g.Sum(x => x.pro_stock));
                MongoTabla.Asignar(row, "precio_promedio", g.Average(x => x.pro_precio));
                dt.Rows.Add(row);
            }
            return dt;
        }

        public bool Insertar()
        {
            NormalizarCampos();
            pro_id = MongoSecuencia.Siguiente("pro_id");
            Coleccion.InsertOne(new ProductoDoc
            {
                pro_id = pro_id,
                pro_nombre = pro_nombre,
                pro_descripcion = pro_descripcion,
                pro_precio = pro_precio,
                pro_stock = pro_stock,
                pro_imagen_ruta = pro_imagen_ruta,
                cat_id = cat_id,
                prov_id = prov_id,
                pro_estado = EstadoRegistro.Activo
            });
            return pro_id > 0;
        }

        public bool Actualizar()
        {
            NormalizarCampos();
            var result = Coleccion.UpdateOne(p => p.pro_id == pro_id,
                Builders<ProductoDoc>.Update
                    .Set(p => p.pro_nombre, pro_nombre)
                    .Set(p => p.pro_descripcion, pro_descripcion)
                    .Set(p => p.pro_precio, pro_precio)
                    .Set(p => p.pro_stock, pro_stock)
                    .Set(p => p.pro_imagen_ruta, pro_imagen_ruta)
                    .Set(p => p.cat_id, cat_id)
                    .Set(p => p.prov_id, prov_id));
            return result.ModifiedCount > 0;
        }

        public bool EliminarLogico()
        {
            var result = Coleccion.UpdateOne(p => p.pro_id == pro_id,
                Builders<ProductoDoc>.Update
                    .Set(p => p.pro_estado, EstadoRegistro.Inactivo)
                    .Set(p => p.pro_fecha_baja, DateTime.Now));

            Imagenes.UpdateMany(i => i.pro_id == pro_id,
                Builders<ImagenProductoDoc>.Update
                    .Set(i => i.ipro_estado, EstadoRegistro.Inactivo)
                    .Set(i => i.ipro_fecha_baja, DateTime.Now));

            return result.ModifiedCount > 0;
        }

        public bool EliminarFisico()
        {
            Imagenes.DeleteMany(i => i.pro_id == pro_id);
            return Coleccion.DeleteOne(p => p.pro_id == pro_id).DeletedCount > 0;
        }

        public bool Reactivar()
        {
            var result = Coleccion.UpdateOne(p => p.pro_id == pro_id,
                Builders<ProductoDoc>.Update
                    .Set(p => p.pro_estado, EstadoRegistro.Activo)
                    .Set(p => p.pro_fecha_baja, null));
            return result.ModifiedCount > 0;
        }

        public DataRow ObtenerPorId(int id)
        {
            var doc = Coleccion.Find(p => p.pro_id == id).FirstOrDefault();
            if (doc == null) return null;

            var catNombre = doc.cat_id.HasValue
                ? Categorias.Find(c => c.cat_id == doc.cat_id.Value).FirstOrDefault()?.cat_nombre
                : null;
            var provNombre = doc.prov_id.HasValue
                ? Proveedores.Find(p => p.prov_id == doc.prov_id.Value).FirstOrDefault()?.prov_nombre
                : null;

            var dt = MongoTabla.Crear("pro_id", "pro_nombre", "pro_descripcion", "pro_precio", "pro_stock",
                "pro_imagen_ruta", "pro_estado", "cat_id", "prov_id", "prov_id_respaldo", "cat_nombre", "prov_nombre");
            var row = dt.NewRow();
            MongoTabla.Asignar(row, "pro_id", doc.pro_id);
            MongoTabla.Asignar(row, "pro_nombre", doc.pro_nombre);
            MongoTabla.Asignar(row, "pro_descripcion", doc.pro_descripcion);
            MongoTabla.Asignar(row, "pro_precio", doc.pro_precio);
            MongoTabla.Asignar(row, "pro_stock", doc.pro_stock);
            MongoTabla.Asignar(row, "pro_imagen_ruta", doc.pro_imagen_ruta);
            MongoTabla.Asignar(row, "pro_estado", doc.pro_estado);
            MongoTabla.Asignar(row, "cat_id", doc.cat_id);
            MongoTabla.Asignar(row, "prov_id", doc.prov_id);
            MongoTabla.Asignar(row, "prov_id_respaldo", doc.prov_id_respaldo);
            MongoTabla.Asignar(row, "cat_nombre", catNombre);
            MongoTabla.Asignar(row, "prov_nombre", provNombre);
            dt.Rows.Add(row);
            return row;
        }

        private List<ProductoDoc> FiltrarProductos(string filtroNombre, int? categoriaId, int? proveedorId, decimal? precioMin, decimal? precioMax, bool incluirInactivos)
        {
            return Coleccion.Find(_ => true).ToList()
                .Where(p => incluirInactivos || p.pro_estado == EstadoRegistro.Activo)
                .Where(p => Contiene(p.pro_nombre, filtroNombre) || Contiene(p.pro_descripcion, filtroNombre))
                .Where(p => !categoriaId.HasValue || p.cat_id == categoriaId)
                .Where(p => !proveedorId.HasValue || p.prov_id == proveedorId)
                .Where(p => !precioMin.HasValue || Math.Round(p.pro_precio, 2) >= Math.Round(precioMin.Value, 2))
                .Where(p => !precioMax.HasValue || Math.Round(p.pro_precio, 2) <= Math.Round(precioMax.Value, 2))
                .ToList();
        }

        private static DataTable AProductoDataTable(List<ProductoDoc> productos, Dictionary<int, string> cats, Dictionary<int, string> provs)
        {
            var dt = MongoTabla.Crear("pro_id", "pro_nombre", "pro_descripcion", "pro_precio", "pro_stock",
                "pro_imagen_ruta", "pro_estado", "cat_id", "cat_nombre", "prov_id", "prov_nombre", "prov_id_respaldo");
            foreach (var p in productos)
            {
                var row = dt.NewRow();
                MongoTabla.Asignar(row, "pro_id", p.pro_id);
                MongoTabla.Asignar(row, "pro_nombre", p.pro_nombre);
                MongoTabla.Asignar(row, "pro_descripcion", p.pro_descripcion);
                MongoTabla.Asignar(row, "pro_precio", p.pro_precio);
                MongoTabla.Asignar(row, "pro_stock", p.pro_stock);
                MongoTabla.Asignar(row, "pro_imagen_ruta", p.pro_imagen_ruta);
                MongoTabla.Asignar(row, "pro_estado", p.pro_estado);
                MongoTabla.Asignar(row, "cat_id", p.cat_id);
                MongoTabla.Asignar(row, "cat_nombre", p.cat_id.HasValue && cats.ContainsKey(p.cat_id.Value) ? cats[p.cat_id.Value] : null);
                MongoTabla.Asignar(row, "prov_id", p.prov_id);
                MongoTabla.Asignar(row, "prov_nombre", p.prov_id.HasValue && provs.ContainsKey(p.prov_id.Value) ? provs[p.prov_id.Value] : null);
                MongoTabla.Asignar(row, "prov_id_respaldo", p.prov_id_respaldo);
                dt.Rows.Add(row);
            }
            return dt;
        }

        private void NormalizarCampos()
        {
            pro_nombre = TruncarTexto(pro_nombre, MaxNombreLength);
            pro_descripcion = TruncarTexto(pro_descripcion, MaxDescripcionLength);
        }

        public static string TruncarTexto(string texto, int maxLength)
        {
            if (string.IsNullOrEmpty(texto) || texto.Length <= maxLength)
                return texto;
            if (maxLength <= 3)
                return texto.Substring(0, maxLength);
            return texto.Substring(0, maxLength - 3) + "...";
        }

        public static bool FueTruncado(string original, string normalizado)
        {
            return !string.IsNullOrEmpty(original)
                && !string.Equals(original, normalizado, StringComparison.Ordinal);
        }

        private static bool Contiene(string texto, string filtro)
        {
            if (string.IsNullOrEmpty(filtro)) return true;
            return (texto ?? string.Empty).IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
