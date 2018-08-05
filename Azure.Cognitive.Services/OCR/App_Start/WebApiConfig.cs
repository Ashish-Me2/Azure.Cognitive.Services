using OCR.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace OCR
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //Add a static object as Cache
            Utility util = new Utility();
            string lunchMenuUri = ConfigurationManager.AppSettings.Get("lunchMenuUri");
            byte[] imageData = util.DownloadImage(lunchMenuUri).Result;
            CacheManager cache = CacheManager.GetInstance();
            cache.SetItem("MENU_IMAGE", imageData);
            cache.SetItem("MENU_REFRESH_TIME", DateTime.Now);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
