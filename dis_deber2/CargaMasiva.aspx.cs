using System;

using System.IO;

using System.Linq;

using System.Web.UI;

using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;



namespace dis_deber2

{

    public partial class CargaMasiva : Page

    {

        protected void Page_Load(object sender, EventArgs e)

        {

            if (!SesionHelper.RequerirAdmin()) return;

        }



        protected void btnImportar_Click(object sender, EventArgs e)

        {

            if (!fuArchivo.HasFile)

            {

                Mostrar("Seleccione un archivo Excel o CSV.", false);

                return;

            }



            var ext = Path.GetExtension(fuArchivo.FileName);

            var error = carga_masiva.ValidarArchivo(fuArchivo.PostedFile.ContentLength, ext);

            if (error != null)

            {

                Mostrar(error, false);

                return;

            }



            var temp = GuardarTemporal(fuArchivo, ext);



            try

            {

                var opciones = new CargaMasivaOpciones

                {

                    OmitirImagenes = chkOmitirImagenes.Checked,

                    ForzarModoRapido = chkModoRapido.Checked

                };



                var servicio = new carga_masiva();

                var resultado = servicio.ImportarAutomatico(temp, ext, ddlTipo.SelectedValue, opciones);



                litResumen.Visible = true;

                litResumen.Text = "<div class=\"alert " + (resultado.Errores.Count == 0 ? "alert-success" : "alert-warning") + "\">" +

                                  "<strong>Importación finalizada</strong>" + resultado.ResumenHtml() + "</div>";



                var msg = "Importación completada.";

                if (resultado.Errores.Count > 0)

                    msg += " Revise los errores en el detalle.";

                Mostrar(msg, resultado.Errores.Count == 0);



                gvPreview.Visible = false;

                pnlHojas.Visible = false;

            }

            catch (Exception ex)

            {

                Mostrar("Error al importar: " + ex.Message, false);

            }

            finally

            {

                if (File.Exists(temp))

                    File.Delete(temp);

            }

        }



        protected void btnPrevisualizar_Click(object sender, EventArgs e)

        {

            if (!fuArchivo.HasFile)

            {

                Mostrar("Seleccione un archivo.", false);

                return;

            }



            var ext = Path.GetExtension(fuArchivo.FileName);

            var error = carga_masiva.ValidarArchivo(fuArchivo.PostedFile.ContentLength, ext);

            if (error != null)

            {

                Mostrar(error, false);

                return;

            }



            var temp = GuardarTemporal(fuArchivo, ext);



            try

            {

                var servicio = new carga_masiva();

                var vista = servicio.LeerVistaPrevia(temp, ext, ddlTipo.SelectedValue);



                gvPreview.DataSource = vista.Muestra;

                gvPreview.DataBind();

                gvPreview.Visible = vista.Muestra != null && vista.Muestra.Rows.Count > 0;



                rptHojas.DataSource = vista.ResumenHojas.Select(p => new { Key = p.Key, RowCount = p.Value }).ToList();

                rptHojas.DataBind();

                pnlHojas.Visible = vista.ResumenHojas.Count > 0;



                var msg = "Previsualización lista (máx. " + carga_masiva.FilasPreviewMax + " filas de muestra). Total estimado: "

                          + vista.TotalFilasEstimadas + " fila(s).";

                if (vista.ModoRapidoSugerido)

                    msg += " Se recomienda activar importación rápida sin imágenes.";

                Mostrar(msg, true);

            }

            catch (Exception ex)

            {

                Mostrar("Error al leer archivo: " + ex.Message, false);

            }

            finally

            {

                if (File.Exists(temp))

                    File.Delete(temp);

            }

        }



        private static string GuardarTemporal(System.Web.UI.WebControls.FileUpload upload, string ext)

        {

            var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ext);

            upload.SaveAs(temp);

            return temp;

        }



        private void Mostrar(string msg, bool ok)

        {

            lblMsg.Text = msg;

            lblMsg.CssClass = ok ? "alert alert-success d-block" : "alert alert-danger d-block";

            lblMsg.Visible = true;

        }

    }

}


