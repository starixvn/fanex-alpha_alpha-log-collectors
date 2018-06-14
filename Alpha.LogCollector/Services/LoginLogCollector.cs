namespace Alpha.LogCollector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Alpha.LogCollector.Helpers;
    using Alpha.LogCollector.Models;
    using Elasticsearch.Net;
    using Fanex.Logging;
    using Fanex.Common;

    public class LoginLogCollector : ILogCollector
    {
        private const string elasticSearchType = "user_login_log";
        private readonly Nest.IElasticClient elasticClient;
        private readonly ILogger logger;
        private readonly WebApiClient webApiClient;

        public LoginLogCollector(
            Nest.IElasticClient elasticClient = null,
            WebApiClient webApiClient = null,
            ILogger logger = null)
        {
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
            this.logger = logger ?? Logger.Log;
            this.webApiClient = webApiClient ?? new WebApiClientImpl();
        }

        public void Collect(DateTime currentTime)
        {
            var lastLoginLog = GetLastLoginLogFromElastic();
            lastLoginLog = ComputeWhatDateToGetLogs(lastLoginLog);

            TransformLogs(lastLoginLog, currentTime);
        }

        internal void TransformLogs(LoginLogDto lastTransformLogDto, DateTime currentTime)
        {
            while (lastTransformLogDto.LogDate.Date < currentTime.Date)
            {
                var loginLogs = GetLoginLogs(lastTransformLogDto);

                if (loginLogs.Any())
                {
                    InsertLogs(loginLogs);

                    var logMessage = $"Done from {lastTransformLogDto.LogDate} to {loginLogs.Last().LogDate}, items: {loginLogs.Count()}";
                    logger.Info(logMessage);
                    Console.WriteLine(logMessage);
                    lastTransformLogDto = loginLogs.Last();
                }
                else
                {
                    lastTransformLogDto.LogDate = new DateTime(lastTransformLogDto.LogDate.Year, lastTransformLogDto.LogDate.Month, lastTransformLogDto.LogDate.Day).AddDays(1);
                }
            }
        }

        private IEnumerable<LoginLogDto> GetLoginLogs(LoginLogDto latestLogin)
            => webApiClient.GetLogs<LoginLogDto>($"LogCollecting/LoginLog?date={latestLogin.LogDate.Date}&logId={latestLogin.LogId}").ToList();

        private LoginLogDto ComputeWhatDateToGetLogs(LoginLogDto latestLoginLog)
            => latestLoginLog != null && latestLoginLog.LogDate > DateTime.MinValue
                            ? latestLoginLog
                            : new LoginLogDto { LogId = 0, LogDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date };


        internal void InsertLogs(IEnumerable<LoginLogDto> loginLogs)
        {
            var errorResults = new List<Result>();

            foreach (var loginLog in loginLogs)
            {
                if (!string.IsNullOrWhiteSpace(loginLog.DomainName))
                {
                    loginLog.DomainName = Cryptography.CfsEnCrypt(loginLog.DomainName);
                }

                if (!string.IsNullOrWhiteSpace(loginLog.DomainName) && loginLog.CountryCode.Equals("Viet Nam", StringComparison.InvariantCultureIgnoreCase))
                {
                    loginLog.CountryCode = "N/A";
                }

                if (!string.IsNullOrWhiteSpace(loginLog.Username2))
                {
                    loginLog.Username2 = Cryptography.CfsEnCrypt(loginLog.Username2);
                }

                if (!string.IsNullOrWhiteSpace(loginLog.Doing))
                {
                    var colonIndex = loginLog.Doing.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 1;
                    var spaceIndex = loginLog.Doing.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase);


                    if (colonIndex > 0 && spaceIndex > colonIndex)
                    {
                        var username = loginLog.Doing.Substring(colonIndex, spaceIndex - colonIndex);
                        loginLog.Doing = loginLog.Doing.Replace(username, Cryptography.CfsEnCrypt(username));
                    }
                    else
                    {
                        loginLog.Doing = new string(loginLog.Doing.ToCharArray().Reverse().ToArray());
                    }
                }

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

        internal LoginLogDto GetLastLoginLogFromElastic()
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