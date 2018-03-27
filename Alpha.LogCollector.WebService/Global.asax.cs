namespace Alpha.LogCollector.WebService
{
    using System.Web.Http;
    using Alpha.LogCollector.WebService.Configurations;
    using Fanex.Data;
    using Fanex.Data.OldConnectionStringProvider;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            DbSettingProviderManager
                .StartNewSession()
                .Use(new OldConnectionStringProvider())
                .UseDefaultDbSettingProvider(resourcePath: AppSettings.StoredProcedurePath, enableWatching: false)
                .Run();
        }
    }
}