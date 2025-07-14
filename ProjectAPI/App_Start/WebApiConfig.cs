using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ProjectAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            EnableCorsAttribute enableCors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(enableCors);

            var appXMLType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(x => x.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXMLType);
        }
    }
}
