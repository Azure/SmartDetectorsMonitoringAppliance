namespace Microsoft.Azure.Monitoring.SmartSignals.ManagementApi
{
    using System.Web.Http;

    /// <summary>
    /// The web API application
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Sets the required configuration for this application
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
