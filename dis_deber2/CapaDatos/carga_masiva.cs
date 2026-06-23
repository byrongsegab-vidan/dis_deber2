using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;

namespace dis_deber2.CapaDatos
{
    public partial class carga_masiva
    {
        private const int MinimoImagenesProducto = 3;
        private const int MaximoImagenesProducto = 11;
        private const decimal FactorInrAUsd = 83m;

        static carga_masiva()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public Dictionary<string, DataTable> LeerLibro(string rutaFisica, string extension)
        {
            extension = (extension ?? string.Empty).ToLowerInvariant();
            if (extension == ".csv")
            {
                var dt = LeerCsv(rutaFisica);
                NormalizarColumnas(dt);
                return new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase)
                {
                    { "datos", dt }
                };
            }

            if (extension == ".xls" || extension == ".xlsx")
                return LeerExcelLibro(rutaFisica);

            throw new InvalidOperationException("Formato no soportado. Use CSV, XLS o XLSX.");
        }

        public DataTable LeerArchivo(string rutaFisica, string extension)
        {
            var libro = LeerLibro(rutaFisica, extension);
            return libro.Values.FirstOrDefault() ?? new DataTable();
        }

        public CargaMasivaResultado ImportarAutomatico(string rutaFisica, string extension, string modo, CargaMasivaOpciones opciones = null)
        {
            opciones = opciones ?? CargaMasivaOpciones.PorDefecto();
            var resultado = new CargaMasivaResultado();
            var errorArchivo = ValidarArchivo(new FileInfo(rutaFisica).Length, extension);
            if (errorArchivo != null)
            {
                resultado.Errores.Add(errorArchivo);
                return resultado;
            }

            extension = (extension ?? string.Empty).ToLowerInvariant();
            var vista = LeerVistaPrevia(rutaFisica, extension, modo, 1);

            if (modo == "prov")
            {
                if (vista.TotalFilasEstimadas > MaxFilasPermitidas)
                {
                    resultado.Errores.Add("Supera el máximo de " + MaxFilasPermitidas + " filas.");
                    return resultado;
                }

                var libroProv = LeerLibro(rutaFisica, extension);
                var dt = ObtenerPrimeraHojaUtil(libroProv, "proveedores", "prov", "proveedor") ?? libroProv.Values.FirstOrDefault();
                if (dt == null || dt.Rows.Count == 0)
                    resultado.Errores.Add("No se encontraron filas de proveedores.");
                else
                    resultado.ProveedoresInsertados = GuardarProveedores(dt, resultado);
                return resultado;
            }

            if (DebeUsarModoRapido(rutaFisica, vista.TotalFilasEstimadas, opciones) || vista.TotalFilasEstimadas > UmbralModoRapido)
            {
                resultado.ModoRapido = true;
                if (vista.TotalFilasEstimadas > UmbralModoRapido)
                    resultado.Advertencias.Add("Archivo grande (" + vista.TotalFilasEstimadas + " filas): importación rápida por lotes.");
                if (opciones.OmitirImagenes || vista.TotalFilasEstimadas > UmbralModoRapido)
                    opciones.OmitirImagenes = true;

                if (modo == "auto")
                {
                    var libro = LeerLibro(rutaFisica, extension);
                    ImportarLibroAuxiliarPequeno(libro, resultado);
                }

                resultado.ProductosInsertados = ImportarProductosStreaming(rutaFisica, extension, modo, resultado, opciones);
                return resultado;
            }

            var libroCompleto = LeerLibro(rutaFisica, extension);

            if (modo == "auto")
                return ImportarLibroCompleto(libroCompleto, resultado);

            var productos = ObtenerPrimeraHojaUtil(libroCompleto, "productos", "pro", "producto")
                ?? DetectarHojaAmazon(libroCompleto)
                ?? DetectarHojaPorColumnas(libroCompleto, "pro_nombre", "pro_precio", "precio")
                ?? libroCompleto.Values.FirstOrDefault();
            if (productos == null || productos.Rows.Count == 0)
            {
                resultado.Errores.Add("No se encontraron filas de productos.");
                return resultado;
            }

            if (productos.Rows.Count > MaxFilasPermitidas)
            {
                resultado.Errores.Add("Supera el máximo de " + MaxFilasPermitidas + " filas.");
                return resultado;
            }

            if (EsFormatoAmazon(productos))
                resultado.Advertencias.Add("Formato Amazon detectado: precios INR se convierten a USD (÷83). Filas sin precio se omiten.");

            resultado.ProductosInsertados = GuardarProductos(productos, resultado);
            return resultado;
        }

        private void ImportarLibroAuxiliarPequeno(Dictionary<string, DataTable> libro, CargaMasivaResultado resultado)
        {
            var dtCategorias = ObtenerPrimeraHojaUtil(libro, "categorias", "categoria", "cat");
            if (dtCategorias != null && dtCategorias.Rows.Count <= UmbralModoRapido)
                resultado.CategoriasInsertadas = GuardarCategorias(dtCategorias, resultado);

            var dtProveedores = ObtenerPrimeraHojaUtil(libro, "proveedores", "proveedor", "prov");
            if (dtProveedores != null && dtProveedores.Rows.Count <= UmbralModoRapido)
                resultado.ProveedoresInsertados = GuardarProveedores(dtProveedores, resultado);
        }

        public CargaMasivaResultado ImportarAutomatico(string rutaFisica, string extension, string modo)
        {
            return ImportarAutomatico(rutaFisica, extension, modo, null);
        }

        public CargaMasivaResultado ImportarLibroCompleto(Dictionary<string, DataTable> libro, CargaMasivaResultado resultado = null)
        {
            resultado = resultado ?? new CargaMasivaResultado();

            var dtCategorias = ObtenerPrimeraHojaUtil(libro, "categorias", "categoria", "cat");
            if (dtCategorias != null)
                resultado.CategoriasInsertadas = GuardarCategorias(dtCategorias, resultado);

            var dtProveedores = ObtenerPrimeraHojaUtil(libro, "proveedores", "proveedor", "prov");
            if (dtProveedores != null)
                resultado.ProveedoresInsertados = GuardarProveedores(dtProveedores, resultado);

            var dtProductos = ObtenerPrimeraHojaUtil(libro, "productos", "producto", "pro")
                ?? DetectarHojaPorColumnas(libro, "pro_nombre", "pro_precio", "precio")
                ?? DetectarHojaAmazon(libro);
            if (dtProductos != null)
            {
                if (EsFormatoAmazon(dtProductos))
                    resultado.Advertencias.Add("Formato Amazon detectado: precios INR se convierten a USD (÷83). Filas sin precio se omiten.");
                resultado.ProductosInsertados = GuardarProductos(dtProductos, resultado);
            }
            else if (libro.Count == 1 && libro.ContainsKey("datos"))
            {
                var unica = libro["datos"];
                if (ContieneColumna(unica, "pro_nombre", "pro_precio", "precio"))
                    resultado.ProductosInsertados = GuardarProductos(unica, resultado);
                else if (EsFormatoAmazon(unica))
                {
                    resultado.Advertencias.Add("Formato Amazon detectado: precios INR se convierten a USD (÷83). Filas sin precio se omiten.");
                    resultado.ProductosInsertados = GuardarProductos(unica, resultado);
                }
                else if (ContieneColumna(unica, "prov_nombre", "prov_ruc"))
                    resultado.ProveedoresInsertados = GuardarProveedores(unica, resultado);
                else if (ContieneColumna(unica, "cat_nombre", "categoria"))
                    resultado.CategoriasInsertadas = GuardarCategorias(unica, resultado);
                else
                    resultado.Errores.Add("No se reconoció el contenido del archivo. Use las columnas de la plantilla.");
            }
            else if (dtCategorias == null && dtProveedores == null && dtProductos == null)
                resultado.Errores.Add("No se encontró una hoja con datos de categorías, proveedores o productos.");

            return resultado;
        }

        public int GuardarCategorias(DataTable dt, CargaMasivaResultado resultado)
        {
            var insertados = 0;
            var svc = new categoria();
            foreach (DataRow row in dt.Rows)
            {
                var nombre = Texto(row, "cat_nombre", "nombre", "categoria");
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;

                if (svc.ObtenerIdPorNombre(nombre).HasValue)
                    continue;

                svc.cat_nombre = nombre;
                if (svc.Insertar())
                    insertados++;
            }

            return insertados;
        }

        public int GuardarProveedores(DataTable dt, CargaMasivaResultado resultado)
        {
            var insertados = 0;
            foreach (DataRow row in dt.Rows)
            {
                var nombre = Texto(row, "prov_nombre", "nombre", "proveedor");
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;

                var existente = new prov().ObtenerIdPorNombre(nombre);
                if (existente.HasValue)
                    continue;

                var svc = new prov
                {
                    prov_nombre = nombre,
                    prov_ruc = Valor(row, "prov_ruc", "ruc")?.ToString(),
                    prov_telefono = Valor(row, "prov_telefono", "telefono", "tel")?.ToString(),
                    prov_correo = Valor(row, "prov_correo", "correo", "email")?.ToString(),
                    prov_direccion = Valor(row, "prov_direccion", "direccion", "dir")?.ToString()
                };
                if (svc.Insertar())
                    insertados++;
            }

            return insertados;
        }

        public int GuardarProductos(DataTable dt, CargaMasivaResultado resultado)
        {
            var insertados = 0;
            var fila = 1;
            var esAmazon = EsFormatoAmazon(dt);

            foreach (DataRow row in dt.Rows)
            {
                fila++;
                try
                {
                    var nombre = Texto(row, "pro_nombre", "nombre", "producto", "title");
                    if (string.IsNullOrWhiteSpace(nombre) || EsFilaEncabezado(nombre))
                        continue;

                    var nombreOriginal = nombre;
                    nombre = pro.TruncarTexto(nombre, pro.MaxNombreLength);
                    if (pro.FueTruncado(nombreOriginal, nombre))
                        resultado.Advertencias.Add("Fila " + fila + ": el nombre superaba " + pro.MaxNombreLength + " caracteres y fue acortado.");

                    if (!TryDecimal(row, out var precio, "pro_precio", "precio", "price"))
                    {
                        if (esAmazon)
                        {
                            resultado.Advertencias.Add("Fila " + fila + ": '" + nombre + "' omitido (sin precio).");
                            continue;
                        }

                        resultado.Errores.Add("Fila " + fila + ": precio inválido para '" + nombre + "'.");
                        continue;
                    }

                    if (!TryEntero(row, out var stock, "pro_stock", "stock", "cantidad"))
                        stock = 0;

                    if (esAmazon)
                    {
                        var moneda = Texto(row, "currency");
                        if (string.Equals(moneda, "INR", StringComparison.OrdinalIgnoreCase))
                            precio = Math.Round(precio / FactorInrAUsd, 2);
                        if (stock <= 0)
                            stock = 25;
                    }

                    var catId = ResolverCategoria(row);
                    var provId = ResolverProveedor(row);

                    var rutasImagen = ObtenerRutasImagen(row, resultado, fila, nombre);
                    if (rutasImagen.Count == 0)
                    {
                        resultado.Advertencias.Add("Fila " + fila + ": '" + nombre + "' se importó sin imágenes.");
                    }
                    else if (rutasImagen.Count < MinimoImagenesProducto)
                    {
                        var primera = rutasImagen[0];
                        while (rutasImagen.Count < MinimoImagenesProducto)
                            rutasImagen.Add(primera);
                        resultado.Advertencias.Add("Fila " + fila + ": '" + nombre + "' tenía pocas imágenes; se completó hasta " + MinimoImagenesProducto + ".");
                    }

                    var descripcion = esAmazon
                        ? ObtenerDescripcionAmazon(row)
                        : Texto(row, "pro_descripcion", "descripcion", "description");

                    if (pro.FueTruncado(nombreOriginal, nombre) && !string.IsNullOrEmpty(nombreOriginal))
                    {
                        if (string.IsNullOrEmpty(descripcion))
                            descripcion = nombreOriginal;
                        else if (descripcion.IndexOf(nombreOriginal, StringComparison.Ordinal) < 0)
                            descripcion = nombreOriginal + " | " + descripcion;
                        descripcion = pro.TruncarTexto(descripcion, pro.MaxDescripcionLength);
                    }

                    var producto = new pro
                    {
                        pro_nombre = nombre,
                        pro_descripcion = descripcion,
                        pro_precio = precio,
                        pro_stock = stock,
                        cat_id = catId,
                        prov_id = provId,
                        pro_imagen_ruta = rutasImagen.FirstOrDefault()
                    };

                    if (!producto.Insertar())
                    {
                        resultado.Errores.Add("Fila " + fila + ": no se pudo insertar '" + nombre + "'.");
                        continue;
                    }

                    for (var i = 0; i < rutasImagen.Count && i < MaximoImagenesProducto; i++)
                    {
                        new imagen_producto
                        {
                            pro_id = producto.pro_id,
                            ipro_ruta = rutasImagen[i],
                            ipro_es_principal = i == 0
                        }.Insertar();
                        resultado.ImagenesDescargadas++;
                    }

                    producto.ActualizarImagenPrincipal(producto.pro_id);
                    insertados++;
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add("Fila " + fila + ": " + ex.Message);
                }
            }

            return insertados;
        }

        private List<string> ObtenerRutasImagen(DataRow row, CargaMasivaResultado resultado, int fila, string nombreProducto)
        {
            var urls = new List<string>();

            var amazonImgs = Texto(row, "images");
            if (!string.IsNullOrWhiteSpace(amazonImgs))
            {
                urls.AddRange(amazonImgs.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Trim())
                    .Where(u => !string.IsNullOrWhiteSpace(u)));
            }

            var combinadas = Texto(row, "pro_imagenes", "imagenes", "imagen_urls", "image_urls");
            if (!string.IsNullOrWhiteSpace(combinadas))
            {
                urls.AddRange(combinadas.Split(new[] { '|', ';', ',', '~' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Trim())
                    .Where(u => !string.IsNullOrWhiteSpace(u)));
            }

            for (var i = 1; i <= MaximoImagenesProducto; i++)
            {
                var valor = Texto(row,
                    "pro_imagen_" + i,
                    "imagen_" + i,
                    "foto_" + i,
                    "image_" + i,
                    "img_" + i);
                if (!string.IsNullOrWhiteSpace(valor))
                    urls.Add(valor.Trim());
            }

            urls = urls.Distinct(StringComparer.OrdinalIgnoreCase).Take(MaximoImagenesProducto).ToList();

            var rutas = new List<string>();
            foreach (var url in urls)
            {
                var ruta = RutasImagen.GuardarDesdeUrl(url, RutasImagen.CarpetaProductos, out var error);
                if (ruta != null)
                    rutas.Add(ruta);
                else if (!string.IsNullOrWhiteSpace(error))
                    resultado.Advertencias.Add("Fila " + fila + " (" + nombreProducto + "): no se pudo obtener imagen '" + url + "' — " + error);
            }

            return rutas;
        }

        private static int? ResolverCategoria(DataRow row)
        {
            if (int.TryParse(Texto(row, "cat_id", "categoria_id"), out var id))
                return id;

            var nombre = Texto(row, "cat_nombre", "categoria", "category");
            if (string.IsNullOrWhiteSpace(nombre))
                nombre = ExtraerCategoriaDeBreadcrumbs(Texto(row, "breadcrumbs"));
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return new categoria().BuscarOCrear(nombre);
        }

        private static int? ResolverProveedor(DataRow row)
        {
            if (int.TryParse(Texto(row, "prov_id", "proveedor_id"), out var id))
                return id;

            var nombre = Texto(row, "prov_nombre", "proveedor", "provider");
            if (string.IsNullOrWhiteSpace(nombre))
                nombre = ExtraerMarcaDeBrand(Texto(row, "brand"));
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return new prov().BuscarOCrear(nombre);
        }

        private static bool EsFormatoAmazon(DataTable dt)
        {
            return dt != null
                && dt.Columns.Contains("title")
                && dt.Columns.Contains("asin")
                && dt.Columns.Contains("images");
        }

        private static DataTable DetectarHojaAmazon(Dictionary<string, DataTable> libro)
        {
            foreach (var par in libro)
            {
                if (EsFormatoAmazon(par.Value))
                    return par.Value;
            }

            return null;
        }

        private static string ObtenerDescripcionAmazon(DataRow row)
        {
            var partes = new List<string>();
            foreach (var campo in new[] { "description", "about_item", "overview", "availability" })
            {
                var t = Texto(row, campo);
                if (!string.IsNullOrWhiteSpace(t) && t != "[]")
                    partes.Add(t);
            }

            var desc = string.Join(" | ", partes.Distinct());
            if (desc.Length > 500)
                desc = desc.Substring(0, 497) + "...";
            return desc;
        }

        private static string ExtraerCategoriaDeBreadcrumbs(string breadcrumbs)
        {
            if (string.IsNullOrWhiteSpace(breadcrumbs))
                return null;

            var partes = breadcrumbs.Split('|')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
            if (partes.Count == 0)
                return null;

            return partes.Count >= 2 ? partes[1] : partes[0];
        }

        private static string ExtraerMarcaDeBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return null;

            brand = brand.Trim();
            const string prefix = "Brand:";
            if (brand.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return brand.Substring(prefix.Length).Trim();

            const string visit = "Visit the ";
            const string store = " Store";
            if (brand.StartsWith(visit, StringComparison.OrdinalIgnoreCase)
                && brand.EndsWith(store, StringComparison.OrdinalIgnoreCase))
            {
                return brand.Substring(visit.Length, brand.Length - visit.Length - store.Length).Trim();
            }

            return brand;
        }

        private static string NormalizarNumeroPrecio(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            texto = texto.Trim().Replace("\"", "").Replace("$", "").Replace("₹", "").Trim();

            if (texto.Contains(",") && texto.Contains("."))
            {
                var lastDot = texto.LastIndexOf('.');
                var lastComma = texto.LastIndexOf(',');
                if (lastDot > lastComma)
                    return texto.Replace(",", "");
                return texto.Replace(".", "").Replace(",", ".");
            }

            if (texto.Contains(",") && !texto.Contains("."))
            {
                var parts = texto.Split(',');
                if (parts.Length == 2 && parts[1].Length <= 2)
                    return parts[0].Replace(".", "") + "." + parts[1];
                return texto.Replace(",", "");
            }

            return texto;
        }

        private static DataTable DetectarHojaPorColumnas(Dictionary<string, DataTable> libro, params string[] columnas)
        {
            foreach (var par in libro)
            {
                if (ContieneColumna(par.Value, columnas))
                    return par.Value;
            }

            return null;
        }

        private static bool ContieneColumna(DataTable dt, params string[] columnas)
        {
            if (dt == null)
                return false;

            return columnas.Any(c => dt.Columns.Contains(c));
        }

        private static DataTable ObtenerPrimeraHojaUtil(Dictionary<string, DataTable> libro, params string[] alias)
        {
            foreach (var nombre in alias)
            {
                foreach (var par in libro)
                {
                    if (string.Equals(NormalizarNombreHoja(par.Key), nombre, StringComparison.OrdinalIgnoreCase))
                        return par.Value;
                }
            }

            return null;
        }

        private static string NormalizarNombreHoja(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return string.Empty;

            return nombre.Trim().TrimEnd('$').Replace("'", string.Empty).ToLowerInvariant();
        }

        private static void NormalizarColumnas(DataTable dt)
        {
            foreach (DataColumn col in dt.Columns)
                col.ColumnName = col.ColumnName.Trim().ToLowerInvariant();
        }

        private static object Valor(DataRow row, params string[] columnas)
        {
            foreach (var col in columnas)
            {
                if (!row.Table.Columns.Contains(col))
                    continue;

                if (row[col] != DBNull.Value && !string.IsNullOrWhiteSpace(row[col].ToString()))
                    return row[col];
            }

            return DBNull.Value;
        }

        private static string Texto(DataRow row, params string[] columnas)
        {
            foreach (var col in columnas)
            {
                if (!row.Table.Columns.Contains(col))
                    continue;

                var texto = row[col]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                    return texto;
            }

            return string.Empty;
        }

        private static bool TryDecimal(DataRow row, out decimal valor, params string[] columnas)
        {
            valor = 0;

            foreach (var col in columnas)
            {
                if (!row.Table.Columns.Contains(col))
                    continue;

                var celda = row[col];
                if (celda == null || celda == DBNull.Value)
                    continue;

                if (celda is decimal d)
                {
                    valor = d;
                    return true;
                }

                if (celda is double dbl)
                {
                    valor = Convert.ToDecimal(dbl);
                    return true;
                }

                if (celda is float fl)
                {
                    valor = Convert.ToDecimal(fl);
                    return true;
                }

                if (celda is int ent)
                {
                    valor = ent;
                    return true;
                }

                var texto = celda.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(texto))
                    continue;

                texto = NormalizarNumeroPrecio(texto);
                if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor))
                    return true;
            }

            return false;
        }

        private static bool TryEntero(DataRow row, out int valor, params string[] columnas)
        {
            valor = 0;

            foreach (var col in columnas)
            {
                if (!row.Table.Columns.Contains(col))
                    continue;

                var celda = row[col];
                if (celda == null || celda == DBNull.Value)
                    continue;

                if (celda is int ent)
                {
                    valor = ent;
                    return true;
                }

                if (celda is double dbl)
                {
                    valor = Convert.ToInt32(dbl);
                    return true;
                }

                if (celda is decimal dec)
                {
                    valor = Convert.ToInt32(dec);
                    return true;
                }

                var texto = celda.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(texto))
                    continue;

                if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out valor))
                    return true;
            }

            return false;
        }

        private static bool EsFilaEncabezado(string nombre)
        {
            var n = nombre.Trim().ToLowerInvariant();
            return n == "pro_nombre" || n == "nombre" || n == "producto" || n == "title";
        }

        private static DataTable LeerCsv(string ruta)
        {
            var dt = new DataTable();
            var lineas = File.ReadAllLines(ruta, Encoding.UTF8);
            if (lineas.Length == 0)
                return dt;

            var headers = ParsearLineaCsv(lineas[0]);
            foreach (var h in headers)
                dt.Columns.Add(h.Trim().ToLowerInvariant());

            for (var i = 1; i < lineas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lineas[i]))
                    continue;

                var campos = ParsearLineaCsv(lineas[i]);
                var fila = dt.NewRow();
                for (var c = 0; c < dt.Columns.Count && c < campos.Length; c++)
                    fila[c] = campos[c];
                dt.Rows.Add(fila);
            }

            return dt;
        }

        private static string[] ParsearLineaCsv(string linea)
        {
            var campos = new List<string>();
            var actual = new StringBuilder();
            var enComillas = false;

            for (var i = 0; i < linea.Length; i++)
            {
                var c = linea[i];
                if (c == '"')
                {
                    if (enComillas && i + 1 < linea.Length && linea[i + 1] == '"')
                    {
                        actual.Append('"');
                        i++;
                    }
                    else
                    {
                        enComillas = !enComillas;
                    }

                    continue;
                }

                if (c == ',' && !enComillas)
                {
                    campos.Add(actual.ToString());
                    actual.Clear();
                    continue;
                }

                actual.Append(c);
            }

            campos.Add(actual.ToString());
            return campos.ToArray();
        }

        private static Dictionary<string, DataTable> LeerExcelLibro(string ruta)
        {
            var libro = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);

            using (var stream = File.Open(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    var dt = new DataTable();
                    dt.TableName = reader.Name;

                    if (!reader.Read())
                        continue;

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var header = reader.GetValue(i)?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(header))
                            header = "columna_" + (i + 1);
                        dt.Columns.Add(header.ToLowerInvariant());
                    }

                    while (reader.Read())
                    {
                        if (FilaVacia(reader))
                            continue;

                        var fila = dt.NewRow();
                        for (var i = 0; i < reader.FieldCount && i < dt.Columns.Count; i++)
                            fila[i] = reader.GetValue(i) ?? DBNull.Value;
                        dt.Rows.Add(fila);
                    }

                    if (dt.Rows.Count > 0 || dt.Columns.Count > 0)
                        libro[reader.Name] = dt;
                }
                while (reader.NextResult());
            }

            foreach (var dt in libro.Values)
                NormalizarColumnas(dt);

            return libro;
        }

        private static bool FilaVacia(IExcelDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var val = reader.GetValue(i);
                if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                    return false;
            }

            return true;
        }
    }
}
