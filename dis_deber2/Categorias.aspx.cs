using System;

namespace dis_deber2
{
    public partial class Categorias : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Proveedores.aspx#categorias");
        }
    }
}