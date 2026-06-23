using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using dis_deber2.CapaDatos;

namespace dis_deber2.CapaPresentacion
{
    [Serializable]
    public class CarritoItem
    {
        public int ProId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }

    public static class CarritoHelper
    {
        private const string SessionKey = "Carrito";

        public static List<CarritoItem> Obtener()
        {
            var items = HttpContext.Current.Session[SessionKey] as List<CarritoItem>;
            if (items == null)
            {
                items = new List<CarritoItem>();
                HttpContext.Current.Session[SessionKey] = items;
            }

            return items;
        }

        public static int CantidadProductos()
        {
            return Obtener().Sum(i => i.Cantidad);
        }

        public static decimal Total()
        {
            return Obtener().Sum(i => i.Subtotal);
        }

        public static string Agregar(int proId, int cantidad)
        {
            if (cantidad <= 0)
                return "Ingrese una cantidad válida (mínimo 1).";

            var row = new pro().ObtenerPorId(proId);
            if (row == null)
                return "El producto no existe o no está disponible.";

            if (row["pro_estado"].ToString() != "A")
                return "El producto no está disponible.";

            var stock = Convert.ToInt32(row["pro_stock"]);
            var items = Obtener();
            var existente = items.FirstOrDefault(i => i.ProId == proId);
            var totalSolicitado = cantidad + (existente?.Cantidad ?? 0);

            if (totalSolicitado > stock)
                return "Stock insuficiente. Disponible: " + stock + ".";

            if (existente != null)
            {
                existente.Cantidad += cantidad;
            }
            else
            {
                items.Add(new CarritoItem
                {
                    ProId = proId,
                    Nombre = row["pro_nombre"].ToString(),
                    Precio = Convert.ToDecimal(row["pro_precio"]),
                    Cantidad = cantidad
                });
            }

            return "¡Agregado al carrito! (" + row["pro_nombre"] + " x" + cantidad + ")";
        }

        public static void ActualizarCantidad(int proId, int cantidad)
        {
            var items = Obtener();
            var item = items.FirstOrDefault(i => i.ProId == proId);
            if (item == null)
                return;

            if (cantidad <= 0)
            {
                items.Remove(item);
                return;
            }

            var row = new pro().ObtenerPorId(proId);
            if (row == null)
            {
                items.Remove(item);
                return;
            }

            var stock = Convert.ToInt32(row["pro_stock"]);
            item.Cantidad = Math.Min(cantidad, stock);
        }

        public static void Eliminar(int proId)
        {
            var items = Obtener();
            items.RemoveAll(i => i.ProId == proId);
        }

        public static void Limpiar()
        {
            HttpContext.Current.Session.Remove(SessionKey);
        }
    }
}
