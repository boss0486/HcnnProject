using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();
            //
            routes.MapRoute(
              name: "Default",
              url: "{controller}/{action}/{id}",
              defaults: new  { area = "", controller = "Home", action = "Index", id = UrlParameter.Optional },
              namespaces: new string[] { "WebApplication.HomePage.Controllers" } // namespace of Controller
           );
            // login
            // routes.MapRoute(
            //    name: "Login",
            //    url: "authen",
            //    defaults: new { controller = "Authen", action = "Login", id = UrlParameter.Optional },
            //    new string[] { "WebApplication.Authentication.Controllers" } // namespace of Controller
            // );
            // //
            // routes.MapRoute(
            //   name: "Default",
            //   url: "",
            //   defaults: new { area = "Authentication", controller = "Authen", action = "Login", id = UrlParameter.Optional },
            //   namespaces: new string[] { "WebApplication.Authentication.Controllers" } // namespace of Controller,
            //   //Areas.Management.ManagementAreaRegistration
            //).DataTokens.Add("area", "Authentication");
        }
    }
}
