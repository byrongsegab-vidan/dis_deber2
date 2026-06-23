using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using dis_deber2.CapaDatos;
using dis_deber2.CapaPresentacion;

namespace dis_deber2
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Evita errores de bundles minificados en hosting compartido (SmarterASP)
            BundleTable.EnableOptimizations = false;

            try
            {
                MongoSeed.InicializarSiVacio();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("MongoDB seed: " + ex.Message);
            }
        }
    }
}