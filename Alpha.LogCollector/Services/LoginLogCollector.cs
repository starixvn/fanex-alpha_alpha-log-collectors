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
        private readonly DateTime currentDate;
        private readonly Nest.IElasticClient elasticClient;
        private readonly ILogger logger;
        private readonly DateTime currentYear;

        public LoginLogCollector(
            IDynamicRepository dynamicRepository = null,
            DateTime? currentDate = null,
            Nest.IElasticClient elasticClient = null,
            ILogger logger = null)
        {
            this.dynamicRepository = dynamicRepository ?? new DynamicRepository();
            this.currentDate = currentDate ?? DateTime.Now;
            this.currentYear = new DateTime(this.currentDate.Year, 1, 1);
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
            this.logger = logger ?? Logger.Log;
        }

        public void Collect()
        {
            var latestLoginLog = GetLatestLoginLog();
            var lastTransformLogTime = ComputeWhatDateToGetLogs(latestLoginLog);

            TransformLogs(lastTransformLogTime);
        }

        internal void TransformLogs(DateTime lastTransformLogTime)
        {
            while (lastTransformLogTime < currentDate)
            {
                var loginLogs = GetLoginLogs(lastTransformLogTime);
                InsertLogs(loginLogs);

                logger.Info($"Done for {lastTransformLogTime}");

                lastTransformLogTime = lastTransformLogTime.AddDays(1);
            }
        }

        private IEnumerable<LoginLogDto> GetLoginLogs(DateTime logDate)
            => dynamicRepository.Fetch<LoginLogDto>(new GetLoginLogByDateCriteria(logDate));

        private DateTime ComputeWhatDateToGetLogs(LoginLogDto latestLoginLog)
            => latestLoginLog != null
                            ? GetLastestDate(latestLoginLog)
                            : currentYear;

        private DateTime GetLastestDate(LoginLogDto latestLoginLog)
            => currentYear > latestLoginLog.LogDate
                                            ? currentYear
                                            : latestLoginLog.LogDate;

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