using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Chat
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SetLanguage",
                url: "SetLanguage/{culture_}",
                defaults: new { controller = "Home", action = "Language", culture_ = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LocalizeJson",
                url: "localize.json",
                defaults: new { controller = "Home", action = "LocalizeJSON" }
            );

			routes.MapRoute(
			 name: "Context",
			 url: "{controller}/{roomId}/{action}",
			 defaults: new { controller = "Home", action = "Index", roomId = Guid.Empty }
		 );

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}