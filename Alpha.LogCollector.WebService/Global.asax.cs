namespace Alpha.LogCollector.WebService
{
    using System.Web.Http;
    using Alpha.LogCollector.WebService.Configurations;
    using Fanex.Data;
    using Fanex.Data.OldConnectionStringProvider;
    using Fanex.Logging.Extensions.Web;

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

            ExceptionHandler
                // To use custom config
                .UseConfig(new LoggingConfiguration()
                // To ignore exceptions
                //.IgnoreException<DivideByZeroException>()
                // To hide sensitive information
                //.HideSensitiveInfo(
                //    new List<string>
                //    {
                //        "(?:password=)(?<paramValue>[^&|^\\s]+)",
                //        "(\"password\":)(?<paramValue>[^,|^\r]+)"
                //    })
                // To Enable Log Http Header
                .GetHttpHeaderInfo()
                // To Enable Log Http Session
                .GetHttpSessionInfo());
        }
    }
}