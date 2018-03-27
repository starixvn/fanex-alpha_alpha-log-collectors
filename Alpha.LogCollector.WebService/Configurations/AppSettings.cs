namespace Alpha.LogCollector.WebService.Configurations
{
    using System.Configuration;

    public static class AppSettings
    {
        static AppSettings()
        {
            StoredProcedurePath = ConfigurationManager.AppSettings["StoredProcedurePath"];
        }

        public static string StoredProcedurePath { get; internal set; }
    }
}