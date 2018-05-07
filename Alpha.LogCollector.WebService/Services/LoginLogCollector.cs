namespace Alpha.LogCollector.WebService.Services
{
    using System;
    using System.Collections.Generic;
    using Criteria;
    using Fanex.Data.Repository;
    using Models;
    using System.Linq;

    public class LoginLogCollector : ILogCollector<LoginLogDto>
    {
        private static KeyValuePair<DateTime, IEnumerable<LoginLogDto>> cacheByDate;
        private readonly IDynamicRepository dynamicRepository;
        private const int NumOfBatchingItems = 5000;

        public LoginLogCollector(IDynamicRepository dynamicRepository = null)
        {
            this.dynamicRepository = dynamicRepository ?? new DynamicRepository();
        }

        public IEnumerable<LoginLogDto> Collect(DateTime date)
        {
            if (date.Date != cacheByDate.Key.Date)
            {
                cacheByDate = new KeyValuePair<DateTime, IEnumerable<LoginLogDto>>(
                    date.Date,
                    dynamicRepository
                        .Fetch<LoginLogDto>(new GetLoginLogByDateCriteria(date))
                        .OrderBy(loginLog => loginLog.LogDate)
                        .Select(loginLog =>
                        {
                            loginLog.LogDate = loginLog.LogDate.AddMilliseconds(loginLog.LogDate.Millisecond);
                            return loginLog;
                        })
                    );
            }

            return cacheByDate.Value.Where(loginLog => loginLog.LogDate > date).Take(NumOfBatchingItems);
        }
    }
}