using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Peacock.ZXEval.WebSite
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            //使用特性路由
            config.MapHttpAttributeRoutes();

            //使用json媒体类型格式
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    Converters = new JsonConverter[] 
                    { 
                        new StringEnumConverter()
                    }
                }
            });


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi1",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

        }
    }
}