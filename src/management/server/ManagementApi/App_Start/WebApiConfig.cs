namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi
{
    using System.Web.Http;

    /// <summary>
    /// Configure the ASP.NET HTTP configuration settings
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register the required HTTP configuration
        /// </summary>
        /// <param name="config">The HTTP configuration.</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
