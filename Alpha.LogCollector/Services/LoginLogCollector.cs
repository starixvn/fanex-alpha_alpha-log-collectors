namespace Alpha.LogCollector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Alpha.LogCollector.Criteria;
    using Alpha.LogCollector.Helpers;
    using Alpha.LogCollector.Models;
    using Elasticsearch.Net;
    using Fanex.Data.Repository;
    using Fanex.Logging;

    public class LoginLogCollector : ILogCollector
    {
        private const string elasticSearchType = "user_login_log";
        private readonly IDynamicRepository dynamicRepository;
        private readonly Nest.IElasticClient elasticClient;
        private readonly ILogger logger;

        public LoginLogCollector(
            IDynamicRepository dynamicRepository = null,
            Nest.IElasticClient elasticClient = null,
            ILogger logger = null)
        {
            this.dynamicRepository = dynamicRepository ?? new DynamicRepository();
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
            this.logger = logger ?? Logger.Log;
        }

        public void Collect(DateTime currentTime)
        {
            var latestLoginLog = GetLatestLoginLog();
            var lastTransformLogDate = ComputeWhatDateToGetLogs(latestLoginLog);

            TransformLogs(lastTransformLogDate, currentTime);
        }

        internal void TransformLogs(DateTime lastTransformLogDate, DateTime currentTime)
        {
            while (lastTransformLogDate.Date < currentTime.Date)
            {
                var loginLogs = GetLoginLogs(lastTransformLogDate);

                if (loginLogs.Any())
                {
                    InsertLogs(loginLogs);
                    
                    logger.Info($"Done from {lastTransformLogDate} to {loginLogs.Last().LogDate}, items: {loginLogs.Count()}");
                }

                lastTransformLogDate = lastTransformLogDate.AddDays(1);
            }
        }

        private IEnumerable<LoginLogDto> GetLoginLogs(DateTime logDate)
            => dynamicRepository.Fetch<LoginLogDto>(new GetLoginLogByDateCriteria(logDate));

        private DateTime ComputeWhatDateToGetLogs(LoginLogDto latestLoginLog)
            => latestLoginLog != null && latestLoginLog.LogDate > DateTime.MinValue
                            ? latestLoginLog.LogDate.Date
                            : new DateTime(DateTime.Now.Year, 1, 1).Date;

        internal void InsertLogs(IEnumerable<LoginLogDto> loginLogs)
        {
            var errorResults = new List<Result>();

            foreach (var loginLog in loginLogs)
            {
                var response = elasticClient.Index(
                    loginLog,
                    i => i
                    .Type(elasticSearchType)
                    .Refresh(Refresh.True));

                if (!response.IsValid)
                {
                    errorResults.Add(Result.CreateErrorResult(response.ToString()));
                }
            }

            if (errorResults.Any())
            {
                logger.Error(string.Join("\r\n", errorResults.Select(result => result.Message)));
            }
        }

        internal LoginLogDto GetLatestLoginLog()
        {
            var searchResponse = elasticClient.Search<LoginLogDto>(
                request => request
                            .Type(elasticSearchType)
                            .Sort(item => item.Descending(f => f.LogDate))
                            .From(0)
                            .Take(1));

            return searchResponse.Documents.FirstOrDefault();
        }
    }
}