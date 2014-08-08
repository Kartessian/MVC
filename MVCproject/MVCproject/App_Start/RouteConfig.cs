using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCproject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account",
                url: "account/{action}",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Editor",
                url: "{action}/{id}",
                defaults: new { controller = "Editor", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Embed",
                url: "embed/{action}/{id}",
                defaults: new { controller = "Embed", action = "GoogleMaps", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Tiles",
                url: "tiles/{z}/{x}/{y}/{id}/{map}",
                defaults: new { controller = "Tiles", action = "Tile", z = UrlParameter.Optional, x = UrlParameter.Optional, y = UrlParameter.Optional, id = UrlParameter.Optional, map = UrlParameter.Optional }
            );
        }
    }
}