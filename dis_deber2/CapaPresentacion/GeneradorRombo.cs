using System;
using System.Collections.Generic;
using System.Text;

namespace dis_deber2.CapaPresentacion
{
    public class GeneradorRombo
    {
        private struct Punto
        {
            public int Fila;
            public int Col;

            public Punto(int fila, int col)
            {
                Fila = fila;
                Col = col;
            }
        }

        public const int Minimo = 1;
        public const int Maximo = 20;

        public class Resultado
        {
            public bool Exito { get; set; }
            public string MensajeError { get; set; }
            public string Patron { get; set; }
            public int Numero { get; set; }
            public bool EsPar { get; set; }
            public string Orientacion { get; set; }
        }

        /// <summary>Valida el texto ingresado. Devuelve null si es válido.</summary>
        public static string ValidarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "Ingrese un número entero del " + Minimo + " al " + Maximo + ".";

            var limpio = texto.Trim();
            int n;
            if (!int.TryParse(limpio, out n))
                return "Solo se permiten números enteros (sin letras ni símbolos).";

            return ValidarNumero(n);
        }

        /// <summary>Valida el valor numérico. Devuelve null si es válido.</summary>
        public static string ValidarNumero(int n)
        {
            if (n < 0)
                return "No se permiten números negativos.";
            if (n < Minimo)
                return "El tamaño mínimo permitido es " + Minimo + ".";
            if (n > Maximo)
                return "El tamaño máximo permitido es " + Maximo + ".";
            return null;
        }

        public static Resultado Generar(int n)
        {
            var error = ValidarNumero(n);
            if (error != null)
            {
                return new Resultado
                {
                    Exito = false,
                    MensajeError = error
                };
            }

            var esPar = n % 2 == 0;
            var orientacion = esPar ? "izquierda" : "derecha";
            var filas = DibujarRombo(n);
            var ancho = 2 * n - 1;
            var anchoInterior = ancho + 2;
            var bordeHorizontal = new string('═', anchoInterior);

            var sb = new StringBuilder();
            sb.AppendLine("╔" + bordeHorizontal + "╗");
            sb.AppendLine("║ " + new string(' ', ancho) + " ║");
            foreach (var linea in filas)
                sb.AppendLine("║ " + linea.PadRight(ancho) + " ║");
            sb.AppendLine("║ " + new string(' ', ancho) + " ║");
            sb.AppendLine("╚" + bordeHorizontal + "╝");

            return new Resultado
            {
                Exito = true,
                Patron = sb.ToString().TrimEnd('\r', '\n'),
                Numero = n,
                EsPar = esPar,
                Orientacion = orientacion
            };
        }

        private static List<string> DibujarRombo(int n)
        {
            var tam = 2 * n - 1;
            var grilla = CrearGrilla(tam);

            DibujarDiagonalSuperior(n, grilla);
            DibujarCuartaDiagonalSuperior(n, grilla);
            DibujarVEnGrilla(n, grilla);
            DibujarCapasInterior(n, grilla);

            var filas = new List<string>();
            for (var r = 0; r < tam; r++)
            {
                var sb = new StringBuilder(tam);
                for (var c = 0; c < tam; c++)
                    sb.Append(grilla[r][c]);
                filas.Add(sb.ToString());
            }
            return filas;
        }

        private static char[][] CrearGrilla(int tam)
        {
            var grilla = new char[tam][];
            for (var r = 0; r < tam; r++)
            {
                grilla[r] = new char[tam];
                for (var c = 0; c < tam; c++)
                    grilla[r][c] = ' ';
            }
            return grilla;
        }

        private static void DibujarDiagonalSuperior(int n, char[][] grilla)
        {
            var centro = n - 1;
            var esPar = n % 2 == 0;

            for (var k = 0; k < n; k++)
            {
                var col = esPar ? centro + k : centro - k;
                grilla[k][col] = '*';
            }
        }

        private static void DibujarCuartaDiagonalSuperior(int n, char[][] grilla)
        {
            var centro = n - 1;
            var esPar = n % 2 == 0;
            var filaAntesMedio = n - 2;

            for (var k = 0; k < n; k++)
            {
                if (k == filaAntesMedio)
                    continue;
                var col = esPar ? centro - k : centro + k;
                grilla[k][col] = '*';
            }
        }

        private static bool SeparadaEnFila(int fila, int col, int centro, int tam, bool esPar)
        {
            if (col < 0 || col >= tam)
                return false;

            if (fila < centro)
                return (centro - fila) + 2 <= col && col <= (centro + fila) - 2;

            var k = fila - centro;
            return col >= k + 2 && col <= (tam - 1 - k) - 2;
        }

        private static bool PuedeColocar(char[][] grilla, int fila, int col)
        {
            if (col < 0 || col >= grilla[0].Length || grilla[fila][col] == '*')
                return false;

            for (var oc = 0; oc < grilla[fila].Length; oc++)
            {
                if (grilla[fila][oc] == '*' && Math.Abs(oc - col) == 1)
                    return false;
            }
            return true;
        }

        private static bool OmitirPorToque(int fila, int col, char[][] grilla, bool esPar)
        {
            for (var oc = 0; oc < grilla[fila].Length; oc++)
            {
                if (grilla[fila][oc] != '*' || oc == col)
                    continue;
                if (Math.Abs(col - oc) != 2)
                    continue;
                if (esPar && col < oc)
                    return true;
                if (!esPar && col > oc)
                    return true;
            }
            return false;
        }

        private static int NumCapasInterior(int n)
        {
            if (n < 4) return 0;
            if (n < 8) return 1;
            return 1 + (n - 6) / 2;
        }

        private static bool LadoQuinta(bool esPar, int capa)
        {
            return capa % 2 == 1;
        }

        private static int LimiteFilaBajada(int n, int capa)
        {
            return (2 * n - 3) - (capa - 1);
        }

        private static int LimiteFilaBajadaSuperior(int n, int capa)
        {
            var finInf = FinBajadaInferior(n, capa);
            var centro = n - 1;
            return Math.Max(0, 2 * centro - finInf.Fila);
        }

        private static Punto FinBajadaInferior(int n, int capa)
        {
            var tam = 2 * n - 1;
            var centro = n - 1;
            var esPar = n % 2 == 0;
            var p = ParamsCapaInterior(n, capa, true);
            var filaAncla = p.FilaAncla;
            var colAncla = p.ColAncla;
            var ladoQuinta = LadoQuinta(esPar, capa);
            var filaDesde = filaAncla + 1;
            var filaHasta = LimiteFilaBajada(n, capa);
            var ultimo = new Punto(filaAncla, colAncla);

            for (var fila = filaDesde; fila <= filaHasta; fila++)
            {
                var col = ColBajada(fila, filaAncla, colAncla, esPar, ladoQuinta);
                if (!SeparadaEnFila(fila, col, centro, tam, esPar))
                    break;
                ultimo = new Punto(fila, col);
            }

            return ultimo;
        }

        private struct ParamsCapa
        {
            public int FilaTope;
            public int FilaAncla;
            public int ColAncla;
        }

        private static ParamsCapa ParamsCapaInterior(int n, int capa, bool abajo)
        {
            var centro = n - 1;
            var esPar = n % 2 == 0;
            int filaTope;
            int filaAncla;
            int colAncla;

            if (abajo)
                filaTope = centro + capa - 1;
            else
                filaTope = centro - (capa - 1);

            if (capa == 1)
            {
                filaAncla = n - 3;
                colAncla = esPar ? centro - (n - 3) : centro + (n - 3);
            }
            else
            {
                filaAncla = filaTope;
                var offset = (capa - 1) * 2;
                if (LadoQuinta(esPar, capa))
                {
                    colAncla = esPar
                        ? (centro - (n - 3)) + offset
                        : (centro + (n - 3)) - offset;
                }
                else
                {
                    colAncla = esPar
                        ? (centro + (n - 3)) - offset
                        : (centro - (n - 3)) + offset;
                }
            }

            return new ParamsCapa { FilaTope = filaTope, FilaAncla = filaAncla, ColAncla = colAncla };
        }

        private static int ColBajada(int fila, int filaAncla, int colAncla, bool esPar, bool ladoQuinta)
        {
            var paso = Math.Abs(fila - filaAncla);
            if (ladoQuinta)
                return esPar ? colAncla + paso : colAncla - paso;
            return esPar ? colAncla - paso : colAncla + paso;
        }

        private static int ColSubida(int fila, int filaFin, int colFin, bool esPar, bool ladoQuinta)
        {
            var paso = Math.Abs(filaFin - fila);
            return esPar ? colFin + paso : colFin - paso;
        }

        private static Punto? DibujarDiagonalBajadaCapa(int n, char[][] grilla, int capa)
        {
            var tam = grilla.Length;
            var centro = n - 1;
            var esPar = n % 2 == 0;
            var p = ParamsCapaInterior(n, capa, true);
            var filaAncla = p.FilaAncla;
            var colAncla = p.ColAncla;
            var ladoQuinta = LadoQuinta(esPar, capa);
            var filaDesde = filaAncla + 1;
            var filaHasta = LimiteFilaBajada(n, capa);
            var yaOmitioToque = false;
            Punto? ultimo = null;

            for (var fila = filaDesde; fila <= filaHasta; fila++)
            {
                var col = ColBajada(fila, filaAncla, colAncla, esPar, ladoQuinta);
                if (!SeparadaEnFila(fila, col, centro, tam, esPar))
                    break;
                if (!PuedeColocar(grilla, fila, col))
                {
                    if (capa == 1)
                        break;
                    continue;
                }
                if (!yaOmitioToque && OmitirPorToque(fila, col, grilla, esPar))
                {
                    yaOmitioToque = true;
                    continue;
                }
                grilla[fila][col] = '*';
                ultimo = new Punto(fila, col);
            }

            return ultimo;
        }

        private static void DibujarDiagonalSubidaCapa(int n, char[][] grilla, int capa, Punto finBajada)
        {
            var esPar = n % 2 == 0;
            var p = ParamsCapaInterior(n, capa, true);
            var filaTope = p.FilaTope;
            var filaFin = finBajada.Fila;
            var colFin = finBajada.Col;

            if (filaFin <= filaTope)
                return;

            var ladoQuinta = LadoQuinta(esPar, capa);
            var tam = grilla.Length;

            for (var fila = filaFin - 1; fila >= filaTope; fila--)
            {
                var col = ColSubida(fila, filaFin, colFin, esPar, ladoQuinta);
                if (col >= 0 && col < tam && PuedeColocar(grilla, fila, col))
                    grilla[fila][col] = '*';
            }
        }

        private static Punto? DibujarDiagonalBajadaCapaSuperior(int n, char[][] grilla, int capa)
        {
            var tam = grilla.Length;
            var centro = n - 1;
            var esPar = n % 2 == 0;
            var p = ParamsCapaInterior(n, capa, false);
            var filaAncla = p.FilaAncla;
            var colAncla = p.ColAncla;
            var ladoQuinta = LadoQuinta(esPar, capa);
            var filaDesde = filaAncla - 1;
            var filaHasta = LimiteFilaBajadaSuperior(n, capa);
            var yaOmitioToque = false;
            Punto? ultimo = null;

            for (var fila = filaDesde; fila >= filaHasta; fila--)
            {
                var col = ColBajada(fila, filaAncla, colAncla, esPar, ladoQuinta);
                if (!SeparadaEnFila(fila, col, centro, tam, esPar))
                    break;
                if (!PuedeColocar(grilla, fila, col))
                {
                    if (capa == 1)
                        break;
                    continue;
                }
                if (!yaOmitioToque && OmitirPorToque(fila, col, grilla, esPar))
                {
                    yaOmitioToque = true;
                    continue;
                }
                grilla[fila][col] = '*';
                ultimo = new Punto(fila, col);
            }

            return ultimo;
        }

        private static void DibujarDiagonalSubidaCapaSuperior(int n, char[][] grilla, int capa, Punto finBajada)
        {
            var esPar = n % 2 == 0;
            var p = ParamsCapaInterior(n, capa, false);
            var filaTope = p.FilaTope;
            var filaFin = finBajada.Fila;
            var colFin = finBajada.Col;

            if (filaFin >= filaTope)
                return;

            var tam = grilla.Length;

            for (var fila = filaFin + 1; fila <= filaTope; fila++)
            {
                var col = ColSubida(fila, filaFin, colFin, esPar, LadoQuinta(esPar, capa));
                if (col >= 0 && col < tam && PuedeColocar(grilla, fila, col))
                    grilla[fila][col] = '*';
            }
        }

        private static void ColocarSiValido(int n, char[][] grilla, int fila, int col)
        {
            var centro = n - 1;
            var tam = 2 * n - 1;
            var esPar = n % 2 == 0;
            if (SeparadaEnFila(fila, col, centro, tam, esPar) && PuedeColocar(grilla, fila, col))
                grilla[fila][col] = '*';
        }

        private static void BajadaCentroIzquierda(int n, char[][] grilla, int filaInicio)
        {
            var centro = n - 1;
            ColocarSiValido(n, grilla, filaInicio, centro);
            for (var fila = filaInicio + 1; fila <= centro; fila++)
            {
                var paso = fila - filaInicio;
                ColocarSiValido(n, grilla, fila, centro - paso);
            }
        }

        private static void SubidaCentroDerecha(int n, char[][] grilla, int filaInicio)
        {
            var centro = n - 1;
            for (var fila = filaInicio + 1; fila <= centro; fila++)
            {
                var paso = fila - filaInicio;
                ColocarSiValido(n, grilla, fila, centro + paso);
            }
        }

        private static void BajadaCentroDerechaInferior(int n, char[][] grilla, int filaInicio)
        {
            var centro = n - 1;
            var filaFin = 2 * centro - filaInicio;
            for (var fila = centro + 1; fila <= filaFin; fila++)
            {
                var paso = filaFin - fila;
                ColocarSiValido(n, grilla, fila, centro + paso);
            }
        }

        private static void SubidaCentroIzquierdaInferior(int n, char[][] grilla, int filaInicio)
        {
            var centro = n - 1;
            var filaFin = 2 * centro - filaInicio;
            for (var fila = centro + 1; fila <= filaFin; fila++)
            {
                var paso = filaFin - fila;
                ColocarSiValido(n, grilla, fila, centro - paso);
            }
            ColocarSiValido(n, grilla, filaFin, centro);
        }

        private static void DibujarCapaCentroMini(int n, char[][] grilla)
        {
            var centro = n - 1;
            ColocarSiValido(n, grilla, centro, centro - 1);
            ColocarSiValido(n, grilla, centro, centro + 1);
            if (centro + 1 < grilla.Length)
                ColocarSiValido(n, grilla, centro + 1, centro);
        }

        private static void DibujarCapaCentroAngosta(int n, char[][] grilla, int filaInicio)
        {
            var centro = n - 1;
            var paso = centro - filaInicio;
            if (paso == 0)
                ColocarSiValido(n, grilla, centro, centro);
            else
            {
                BajadaCentroIzquierda(n, grilla, filaInicio);
                SubidaCentroDerecha(n, grilla, filaInicio);
            }
            if (paso >= 1 && centro + 1 < grilla.Length)
                ColocarSiValido(n, grilla, centro + 1, centro);
        }

        private static void DibujarEspiralesCentro(int n, char[][] grilla)
        {
            var centro = n - 1;
            var k = 1;
            while (true)
            {
                var filaInicio = 4 * k;
                if (filaInicio > centro)
                    break;

                if (filaInicio == centro)
                    DibujarCapaCentroMini(n, grilla);
                else if (filaInicio + 1 >= centro)
                    DibujarCapaCentroAngosta(n, grilla, filaInicio);
                else
                {
                    BajadaCentroIzquierda(n, grilla, filaInicio);
                    SubidaCentroDerecha(n, grilla, filaInicio);
                    BajadaCentroDerechaInferior(n, grilla, filaInicio);
                    SubidaCentroIzquierdaInferior(n, grilla, filaInicio);
                }
                k++;
            }
        }

        private static void DibujarCapasInterior(int n, char[][] grilla)
        {
            if (NumCapasInterior(n) >= 1)
            {
                var finInf = DibujarDiagonalBajadaCapa(n, grilla, 1);
                if (finInf.HasValue)
                    DibujarDiagonalSubidaCapa(n, grilla, 1, finInf.Value);

                var finSup = DibujarDiagonalBajadaCapaSuperior(n, grilla, 1);
                if (finSup.HasValue)
                    DibujarDiagonalSubidaCapaSuperior(n, grilla, 1, finSup.Value);
            }

            if (n >= 8)
                DibujarEspiralesCentro(n, grilla);
        }

        private static void DibujarVEnGrilla(int n, char[][] grilla)
        {
            var tam = grilla.Length;
            var centro = n - 1;

            for (var k = 0; k < n; k++)
            {
                var fila = centro + k;
                if (k == centro)
                {
                    grilla[fila][centro] = '*';
                    continue;
                }

                grilla[fila][k] = '*';
                grilla[fila][tam - 1 - k] = '*';
            }
        }
    }
}
