using WebApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Helper.User;

namespace Webapplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        { 
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
            //Helper.User.UserSession.SetSession(new UserSessionModel());
            // 
        }
        // remove X-AspNet-Version và Server header
        protected void Application_PreSendRequestHeaders()
        {
            // HttpContext.Current.Response.Headers.Remove("X-Powered-By");
            //HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
            //HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
            //HttpContext.Current.Response.Headers.Remove("Server"); 
        }
    }
}
