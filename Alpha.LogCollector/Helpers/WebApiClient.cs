namespace Alpha.LogCollector.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Configurations;
    using Fanex.Logging;

    public interface WebApiClient
    {
        IEnumerable<T> GetLogs<T>(string relativeUrl);
    }

    public class WebApiClientImpl : WebApiClient
    {
        private static HttpClient httpClient = new HttpClient();
        private readonly string baseUrl;
        private readonly ILogger logger;

        public WebApiClientImpl(string baseUrl = null, ILogger logger = null)
        {
            this.baseUrl = baseUrl ?? AppSettings.LogCollectingApiBaseUrl;
            this.logger = logger ?? Logger.Log;
        }

        public IEnumerable<T> GetLogs<T>(string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
            {
                return Enumerable.Empty<T>();
            }

            var response = httpClient.GetAsync($"{baseUrl}/{relativeUrl}").Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<IEnumerable<T>>().Result;
            }
            else
            {
                logger.Error(response.Content.ReadAsStringAsync().Result);
            }

            return Enumerable.Empty<T>();
        }
    }
}