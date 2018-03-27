namespace Alpha.LogCollector.WebService.Services
{
    using System;
    using System.Collections.Generic;
    using Criteria;
    using Fanex.Data.Repository;
    using Models;

    public class LoginLogCollector : ILogCollector<LoginLogDto>
    {
        private readonly IDynamicRepository dynamicRepository;

        public LoginLogCollector(IDynamicRepository dynamicRepository = null)
        {
            this.dynamicRepository = dynamicRepository ?? new DynamicRepository();
        }

        public IEnumerable<LoginLogDto> Collect(DateTime date)
         => dynamicRepository.Fetch<LoginLogDto>(new GetLoginLogByDateCriteria(date));
    }
}