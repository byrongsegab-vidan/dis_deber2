namespace dis_deber2.CapaDatos
{
    public class CargaMasivaOpciones
    {
        /// <summary>Sin descargar imágenes (recomendado para archivos grandes).</summary>
        public bool OmitirImagenes { get; set; }

        /// <summary>Forzar inserción por lotes aunque el archivo sea pequeño.</summary>
        public bool ForzarModoRapido { get; set; }

        public static CargaMasivaOpciones PorDefecto()
        {
            return new CargaMasivaOpciones { OmitirImagenes = false };
        }
    }
}
