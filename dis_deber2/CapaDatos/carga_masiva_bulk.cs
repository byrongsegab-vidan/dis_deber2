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
        public const int MaxFilasPermitidas = 50000;
        public const int TamanoLoteBulk = 2000;
        public const int FilasPreviewMax = 100;
        public const int UmbralModoRapido = 500;
        public const int MaxErroresReportados = 50;
        public const long TamanoMaximoArchivoBytes = 104857600L; // 100 MB

        public static string ValidarArchivo(long bytes, string extension)
        {
            if (bytes <= 0)
                return "El archivo está vacío.";

            if (bytes > TamanoMaximoArchivoBytes)
                return "El archivo supera el máximo de " + (TamanoMaximoArchivoBytes / 1024 / 1024) + " MB.";

            extension = (extension ?? string.Empty).ToLowerInvariant();
            if (extension != ".csv" && extension != ".xls" && extension != ".xlsx")
                return "Formato no soportado. Use CSV, XLS o XLSX.";

            return null;
        }

        public CargaMasivaVistaPrevia LeerVistaPrevia(string rutaFisica, string extension, string modo, int maxFilasMuestra = FilasPreviewMax)
        {
            extension = (extension ?? string.Empty).ToLowerInvariant();
            var vista = new CargaMasivaVistaPrevia();

            if (extension == ".csv")
                LeerVistaPreviaCsv(rutaFisica, vista, maxFilasMuestra);
            else if (extension == ".xls" || extension == ".xlsx")
                LeerVistaPreviaExcel(rutaFisica, modo, vista, maxFilasMuestra);
            else
                throw new InvalidOperationException("Formato no soportado. Use CSV, XLS o XLSX.");

            vista.ModoRapidoSugerido = vista.TotalFilasEstimadas > UmbralModoRapido;
            return vista;
        }

        private bool DebeUsarModoRapido(string rutaFisica, int filasEstimadas, CargaMasivaOpciones opciones)
        {
            if (opciones.ForzarModoRapido || opciones.OmitirImagenes)
                return true;

            if (filasEstimadas > UmbralModoRapido)
                return true;

            var info = new FileInfo(rutaFisica);
            return info.Exists && info.Length > 5 * 1024 * 1024;
        }

        public int ImportarProductosStreaming(string rutaFisica, string extension, string modo, CargaMasivaResultado resultado, CargaMasivaOpciones opciones)
        {
            extension = (extension ?? string.Empty).ToLowerInvariant();
            if (extension == ".csv")
                return ImportarProductosCsvStreaming(rutaFisica, resultado, opciones);

            return ImportarProductosExcelStreaming(rutaFisica, modo, resultado, opciones);
        }

        private int ImportarProductosCsvStreaming(string rutaFisica, CargaMasivaResultado resultado, CargaMasivaOpciones opciones)
        {
            var insertados = 0;
            using (var cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                var cacheCat = PrecargarCategorias(cn);
                var cacheProv = PrecargarProveedores(cn);
                var lote = CrearTablaBulkProductos();
                var omitirImagenes = opciones.OmitirImagenes;
                var esAmazon = false;
                string[] headers = null;
                var filaNum = 0;

                using (var reader = new StreamReader(rutaFisica, Encoding.UTF8))
                {
                    string linea;
                    while ((linea = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(linea))
                            continue;

                        if (headers == null)
                        {
                            headers = ParsearLineaCsv(linea).Select(NormalizarNombreColumna).ToArray();
                            esAmazon = EsFormatoAmazon(headers);
                            continue;
                        }

                        filaNum++;
                        if (filaNum > MaxFilasPermitidas)
                        {
                            AgregarError(resultado, "Se superó el máximo de " + MaxFilasPermitidas + " filas.");
                            break;
                        }

                        resultado.FilasProcesadas++;
                        var campos = ParsearLineaCsv(linea);
                        var fila = FilaDesdeArray(headers, campos);
                        if (!TryConstruirFilaProducto(fila, filaNum, esAmazon, omitirImagenes, cacheCat, cacheProv, cn, resultado, out var row))
                        {
                            resultado.FilasOmitidas++;
                            continue;
                        }

                        lote.Rows.Add(row);
                        if (lote.Rows.Count >= TamanoLoteBulk)
                        {
                            insertados += InsertarLoteBulk(cn, lote);
                            lote.Clear();
                        }
                    }
                }

                if (lote.Rows.Count > 0)
                    insertados += InsertarLoteBulk(cn, lote);
            }

            return insertados;
        }

        private int ImportarProductosExcelStreaming(string rutaFisica, string modo, CargaMasivaResultado resultado, CargaMasivaOpciones opciones)
        {
            var insertados = 0;
            var hojaObjetivo = ObtenerNombreHojaProductos(rutaFisica, modo);

            using (var cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                var cacheCat = PrecargarCategorias(cn);
                var cacheProv = PrecargarProveedores(cn);
                var lote = CrearTablaBulkProductos();
                var omitirImagenes = opciones.OmitirImagenes;

                using (var stream = File.Open(rutaFisica, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    if (!string.IsNullOrEmpty(hojaObjetivo))
                    {
                        while (reader.Name != hojaObjetivo && reader.NextResult()) { }
                    }

                    if (!reader.Read())
                        return 0;

                    var headers = LeerEncabezadosExcel(reader);
                    var esAmazon = EsFormatoAmazon(headers);
                    var filaNum = 0;

                    while (reader.Read())
                    {
                        if (FilaVacia(reader))
                            continue;

                        filaNum++;
                        if (filaNum > MaxFilasPermitidas)
                        {
                            AgregarError(resultado, "Se superó el máximo de " + MaxFilasPermitidas + " filas.");
                            break;
                        }

                        resultado.FilasProcesadas++;
                        var fila = FilaDesdeReader(reader, headers);
                        if (!TryConstruirFilaProducto(fila, filaNum, esAmazon, omitirImagenes, cacheCat, cacheProv, cn, resultado, out var row))
                        {
                            resultado.FilasOmitidas++;
                            continue;
                        }

                        lote.Rows.Add(row);
                        if (lote.Rows.Count >= TamanoLoteBulk)
                        {
                            insertados += InsertarLoteBulk(cn, lote);
                            lote.Clear();
                        }
                    }
                }

                if (lote.Rows.Count > 0)
                    insertados += InsertarLoteBulk(cn, lote);
            }

            return insertados;
        }

        private static string ObtenerNombreHojaProductos(string rutaFisica, string modo)
        {
            if (modo == "prov")
                return null;

            var nombres = new List<string>();
            using (var stream = File.Open(rutaFisica, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    nombres.Add(reader.Name);
                }
                while (reader.NextResult());
            }

            foreach (var alias in new[] { "productos", "producto", "pro" })
            {
                var hoja = nombres.FirstOrDefault(n => NormalizarNombreHoja(n) == alias);
                if (hoja != null)
                    return hoja;
            }

            return nombres.FirstOrDefault();
        }

        private static string[] LeerEncabezadosExcel(IExcelDataReader reader)
        {
            var headers = new string[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var header = reader.GetValue(i)?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(header))
                    header = "columna_" + (i + 1);
                headers[i] = NormalizarNombreColumna(header);
            }

            return headers;
        }

        private static Dictionary<string, string> FilaDesdeReader(IExcelDataReader reader, string[] headers)
        {
            var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount && i < headers.Length; i++)
            {
                var val = reader.GetValue(i);
                if (val == null || val == DBNull.Value)
                    continue;
                var texto = val.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                    fila[headers[i]] = texto;
            }

            return fila;
        }

        private static Dictionary<string, string> FilaDesdeArray(string[] headers, string[] campos)
        {
            var fila = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Length && c < campos.Length; c++)
            {
                var texto = campos[c]?.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                    fila[headers[c]] = texto;
            }

            return fila;
        }

        private static string NormalizarNombreColumna(string nombre)
        {
            return (nombre ?? string.Empty).Trim().ToLowerInvariant();
        }

        private bool TryConstruirFilaProducto(
            Dictionary<string, string> fila,
            int filaNum,
            bool esAmazon,
            bool omitirImagenes,
            Dictionary<string, int> cacheCat,
            Dictionary<string, int> cacheProv,
            SqlConnection cn,
            CargaMasivaResultado resultado,
            out object[] row)
        {
            row = null;
            var nombre = TextoDict(fila, "pro_nombre", "nombre", "producto", "title");
            if (string.IsNullOrWhiteSpace(nombre) || EsFilaEncabezado(nombre))
                return false;

            var nombreOriginal = nombre;
            nombre = pro.TruncarTexto(nombre, pro.MaxNombreLength);
            if (pro.FueTruncado(nombreOriginal, nombre))
                AgregarAdvertencia(resultado, "Fila " + filaNum + ": nombre acortado a " + pro.MaxNombreLength + " caracteres.");

            if (!TryDecimalDict(fila, out var precio, "pro_precio", "precio", "price"))
            {
                if (esAmazon)
                {
                    AgregarAdvertencia(resultado, "Fila " + filaNum + ": '" + nombre + "' omitido (sin precio).");
                    return false;
                }

                AgregarError(resultado, "Fila " + filaNum + ": precio inválido para '" + nombre + "'.");
                return false;
            }

            if (!TryEnteroDict(fila, out var stock, "pro_stock", "stock", "cantidad"))
                stock = 0;

            if (esAmazon)
            {
                var moneda = TextoDict(fila, "currency");
                if (string.Equals(moneda, "INR", StringComparison.OrdinalIgnoreCase))
                    precio = Math.Round(precio / FactorInrAUsd, 2);
                if (stock <= 0)
                    stock = 25;
            }

            var catId = ResolverCategoriaCache(fila, cacheCat, cn);
            var provId = ResolverProveedorCache(fila, cacheProv, cn);

            var descripcion = esAmazon
                ? ObtenerDescripcionAmazonDict(fila)
                : TextoDict(fila, "pro_descripcion", "descripcion", "description");

            if (pro.FueTruncado(nombreOriginal, nombre) && !string.IsNullOrEmpty(nombreOriginal))
            {
                if (string.IsNullOrEmpty(descripcion))
                    descripcion = nombreOriginal;
                else if (descripcion.IndexOf(nombreOriginal, StringComparison.Ordinal) < 0)
                    descripcion = nombreOriginal + " | " + descripcion;
                descripcion = pro.TruncarTexto(descripcion, pro.MaxDescripcionLength);
            }

            string imagenRuta = null;
            if (!omitirImagenes)
            {
                var rutas = ObtenerRutasImagenDict(fila, resultado, filaNum, nombre);
                imagenRuta = rutas.FirstOrDefault();
                resultado.ImagenesDescargadas += rutas.Count;
            }

            row = new object[]
            {
                nombre,
                string.IsNullOrWhiteSpace(descripcion) ? (object)DBNull.Value : descripcion,
                precio,
                stock,
                string.IsNullOrWhiteSpace(imagenRuta) ? (object)DBNull.Value : imagenRuta,
                catId.HasValue ? (object)catId.Value : DBNull.Value,
                provId.HasValue ? (object)provId.Value : DBNull.Value,
                "A"
            };
            return true;
        }

        private static DataTable CrearTablaBulkProductos()
        {
            var dt = new DataTable();
            dt.Columns.Add("pro_nombre", typeof(string));
            dt.Columns.Add("pro_descripcion", typeof(string));
            dt.Columns.Add("pro_precio", typeof(decimal));
            dt.Columns.Add("pro_stock", typeof(int));
            dt.Columns.Add("pro_imagen_ruta", typeof(string));
            dt.Columns.Add("cat_id", typeof(int));
            dt.Columns.Add("prov_id", typeof(int));
            dt.Columns.Add("pro_estado", typeof(string));
            return dt;
        }

        private static int InsertarLoteBulk(SqlConnection cn, DataTable lote)
        {
            if (lote.Rows.Count == 0)
                return 0;

            using (var bulk = new SqlBulkCopy(cn))
            {
                bulk.DestinationTableName = "tbl_producto";
                bulk.BatchSize = TamanoLoteBulk;
                bulk.ColumnMappings.Add("pro_nombre", "pro_nombre");
                bulk.ColumnMappings.Add("pro_descripcion", "pro_descripcion");
                bulk.ColumnMappings.Add("pro_precio", "pro_precio");
                bulk.ColumnMappings.Add("pro_stock", "pro_stock");
                bulk.ColumnMappings.Add("pro_imagen_ruta", "pro_imagen_ruta");
                bulk.ColumnMappings.Add("cat_id", "cat_id");
                bulk.ColumnMappings.Add("prov_id", "prov_id");
                bulk.ColumnMappings.Add("pro_estado", "pro_estado");
                bulk.WriteToServer(lote);
            }

            return lote.Rows.Count;
        }

        private static Dictionary<string, int> PrecargarCategorias(SqlConnection cn)
        {
            var cache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT cat_id, cat_nombre FROM tbl_categoria WHERE cat_estado = 'A'", cn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    cache[reader.GetString(1).Trim()] = reader.GetInt32(0);
            }

            return cache;
        }

        private static Dictionary<string, int> PrecargarProveedores(SqlConnection cn)
        {
            var cache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT prov_id, prov_nombre FROM tbl_proveedor WHERE prov_estado = 'A'", cn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    cache[reader.GetString(1).Trim()] = reader.GetInt32(0);
            }

            return cache;
        }

        private int? ResolverCategoriaCache(Dictionary<string, string> fila, Dictionary<string, int> cache, SqlConnection cn)
        {
            if (int.TryParse(TextoDict(fila, "cat_id", "categoria_id"), out var id))
                return id;

            var nombre = TextoDict(fila, "cat_nombre", "categoria", "category");
            if (string.IsNullOrWhiteSpace(nombre))
                nombre = ExtraerCategoriaDeBreadcrumbs(TextoDict(fila, "breadcrumbs"));
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            nombre = nombre.Trim();
            if (cache.TryGetValue(nombre, out var existente))
                return existente;

            using (var cmd = new SqlCommand(@"
                INSERT INTO tbl_categoria (cat_nombre, cat_estado)
                VALUES (@nombre, 'A');
                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                var nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
                cache[nombre] = nuevoId;
                return nuevoId;
            }
        }

        private int? ResolverProveedorCache(Dictionary<string, string> fila, Dictionary<string, int> cache, SqlConnection cn)
        {
            if (int.TryParse(TextoDict(fila, "prov_id", "proveedor_id"), out var id))
                return id;

            var nombre = TextoDict(fila, "prov_nombre", "proveedor", "provider");
            if (string.IsNullOrWhiteSpace(nombre))
                nombre = ExtraerMarcaDeBrand(TextoDict(fila, "brand"));
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            nombre = nombre.Trim();
            if (cache.TryGetValue(nombre, out var existente))
                return existente;

            using (var cmd = new SqlCommand(@"
                INSERT INTO tbl_proveedor (prov_nombre, prov_estado)
                VALUES (@nombre, 'A');
                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                var nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
                cache[nombre] = nuevoId;
                return nuevoId;
            }
        }

        private static bool EsFormatoAmazon(IEnumerable<string> columnas)
        {
            var set = new HashSet<string>(columnas ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            return set.Contains("title") && set.Contains("asin") && set.Contains("images");
        }

        private static string TextoDict(Dictionary<string, string> fila, params string[] columnas)
        {
            foreach (var col in columnas)
            {
                if (fila.TryGetValue(col, out var texto) && !string.IsNullOrWhiteSpace(texto))
                    return texto.Trim();
            }

            return string.Empty;
        }

        private static bool TryDecimalDict(Dictionary<string, string> fila, out decimal valor, params string[] columnas)
        {
            valor = 0;
            foreach (var col in columnas)
            {
                if (!fila.TryGetValue(col, out var texto) || string.IsNullOrWhiteSpace(texto))
                    continue;

                texto = NormalizarNumeroPrecio(texto);
                if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor))
                    return true;
            }

            return false;
        }

        private static bool TryEnteroDict(Dictionary<string, string> fila, out int valor, params string[] columnas)
        {
            valor = 0;
            foreach (var col in columnas)
            {
                if (!fila.TryGetValue(col, out var texto) || string.IsNullOrWhiteSpace(texto))
                    continue;

                if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out valor))
                    return true;
            }

            return false;
        }

        private static string ObtenerDescripcionAmazonDict(Dictionary<string, string> fila)
        {
            var partes = new List<string>();
            foreach (var campo in new[] { "description", "about_item", "overview", "availability" })
            {
                var t = TextoDict(fila, campo);
                if (!string.IsNullOrWhiteSpace(t) && t != "[]")
                    partes.Add(t);
            }

            var desc = string.Join(" | ", partes.Distinct());
            if (desc.Length > 500)
                desc = desc.Substring(0, 497) + "...";
            return desc;
        }

        private List<string> ObtenerRutasImagenDict(Dictionary<string, string> fila, CargaMasivaResultado resultado, int filaNum, string nombreProducto)
        {
            var urls = new List<string>();
            var amazonImgs = TextoDict(fila, "images");
            if (!string.IsNullOrWhiteSpace(amazonImgs))
            {
                urls.AddRange(amazonImgs.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Trim())
                    .Where(u => !string.IsNullOrWhiteSpace(u)));
            }

            var combinadas = TextoDict(fila, "pro_imagenes", "imagenes", "imagen_urls", "image_urls");
            if (!string.IsNullOrWhiteSpace(combinadas))
            {
                urls.AddRange(combinadas.Split(new[] { '|', ';', ',', '~' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Trim())
                    .Where(u => !string.IsNullOrWhiteSpace(u)));
            }

            for (var i = 1; i <= MaximoImagenesProducto; i++)
            {
                var valor = TextoDict(fila, "pro_imagen_" + i, "imagen_" + i, "foto_" + i, "image_" + i, "img_" + i);
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
                    AgregarAdvertencia(resultado, "Fila " + filaNum + " (" + nombreProducto + "): imagen no obtenida — " + error);
            }

            return rutas;
        }

        private void LeerVistaPreviaCsv(string ruta, CargaMasivaVistaPrevia vista, int maxFilasMuestra)
        {
            var muestra = new DataTable();
            string[] headers = null;
            var total = 0;

            foreach (var linea in File.ReadLines(ruta, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                if (headers == null)
                {
                    headers = ParsearLineaCsv(linea).Select(NormalizarNombreColumna).ToArray();
                    foreach (var h in headers)
                        muestra.Columns.Add(h);
                    continue;
                }

                total++;
                if (muestra.Rows.Count >= maxFilasMuestra)
                    continue;

                var campos = ParsearLineaCsv(linea);
                var row = muestra.NewRow();
                for (var c = 0; c < headers.Length && c < campos.Length; c++)
                    row[c] = campos[c];
                muestra.Rows.Add(row);
            }

            vista.Muestra = muestra;
            vista.TotalFilasEstimadas = total;
            vista.ResumenHojas.Add(new KeyValuePair<string, int>("datos", total));
        }

        private void LeerVistaPreviaExcel(string ruta, string modo, CargaMasivaVistaPrevia vista, int maxFilasMuestra)
        {
            using (var stream = File.Open(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    var dtMuestra = new DataTable();
                    dtMuestra.TableName = reader.Name;
                    var totalHoja = 0;

                    if (!reader.Read())
                    {
                        vista.ResumenHojas.Add(new KeyValuePair<string, int>(reader.Name, 0));
                        continue;
                    }

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var header = reader.GetValue(i)?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(header))
                            header = "columna_" + (i + 1);
                        dtMuestra.Columns.Add(NormalizarNombreColumna(header));
                    }

                    while (reader.Read())
                    {
                        if (FilaVacia(reader))
                            continue;

                        totalHoja++;
                        if (dtMuestra.Rows.Count < maxFilasMuestra)
                        {
                            var fila = dtMuestra.NewRow();
                            for (var i = 0; i < reader.FieldCount && i < dtMuestra.Columns.Count; i++)
                                fila[i] = reader.GetValue(i) ?? DBNull.Value;
                            dtMuestra.Rows.Add(fila);
                        }
                    }

                    vista.ResumenHojas.Add(new KeyValuePair<string, int>(reader.Name, totalHoja));
                    vista.TotalFilasEstimadas += totalHoja;

                    if (vista.Muestra == null || EsHojaPreviewPreferida(reader.Name, modo, totalHoja, vista.Muestra.Rows.Count))
                        vista.Muestra = dtMuestra;
                }
                while (reader.NextResult());
            }
        }

        private static bool EsHojaPreviewPreferida(string nombreHoja, string modo, int filasHoja, int filasMuestraActual)
        {
            if (modo == "prov")
                return nombreHoja.IndexOf("prov", StringComparison.OrdinalIgnoreCase) >= 0 || filasHoja > filasMuestraActual;

            if (modo == "pro")
                return nombreHoja.IndexOf("pro", StringComparison.OrdinalIgnoreCase) >= 0 || filasHoja > filasMuestraActual;

            return filasHoja > filasMuestraActual;
        }

        private static void AgregarError(CargaMasivaResultado resultado, string mensaje)
        {
            if (resultado.Errores.Count < MaxErroresReportados)
                resultado.Errores.Add(mensaje);
        }

        private static void AgregarAdvertencia(CargaMasivaResultado resultado, string mensaje)
        {
            if (resultado.Advertencias.Count < MaxErroresReportados)
                resultado.Advertencias.Add(mensaje);
        }
    }
}
