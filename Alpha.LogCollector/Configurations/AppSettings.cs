namespace Alpha.LogCollector.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public static class AppSettings
    {
        static AppSettings()
        {
            var elasticsearchUriList = ConfigurationManager.AppSettings["ElasticsearchUriList"] ?? "https://localhost:9200/";

            UriList = elasticsearchUriList.Split(',').Select(uriString => new Uri(uriString));

            IsSSL = elasticsearchUriList.ToLowerInvariant().Contains("https");

            ElasticsearchIndex = ConfigurationManager.AppSettings["ElasticsearchIndex"] ?? "tracking_activity";

            ElasticsearchAuthInfo = ConfigurationManager.AppSettings["ElasticsearchAuthInfo"] ?? "admin:admin";

            ElasticsearchIndexType = ConfigurationManager.AppSettings["ElasticsearchIndexType"] ?? "user_activity";

            StoredProcedurePath = ConfigurationManager.AppSettings["StoredProcedurePath"];
        }

        public static IEnumerable<Uri> UriList { get; internal set; }

        public static bool IsSSL { get; internal set; }

        public static string ElasticsearchIndex { get; internal set; }

        public static string ElasticsearchAuthInfo { get; internal set; }

        public static string ElasticsearchIndexType { get; internal set; }

        public static string StoredProcedurePath { get; internal set; }
    }
}
