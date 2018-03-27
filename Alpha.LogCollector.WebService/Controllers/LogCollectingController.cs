namespace Alpha.LogCollector.WebService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using Models;
    using Services;

    public class LogCollectingController : ApiController
    {
        private readonly ILogCollector<LoginLogDto> loginLogCollector;

        public LogCollectingController() : this(null)
        { }

        public LogCollectingController(ILogCollector<LoginLogDto> loginLogCollector)
        {
            this.loginLogCollector = loginLogCollector ?? new LoginLogCollector();
        }

        [HttpGet]
        public IEnumerable<LoginLogDto> Index(DateTime date)
            => loginLogCollector.Collect(date);
    }
}