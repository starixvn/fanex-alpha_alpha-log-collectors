namespace Alpha.LogCollector
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using System.Timers;
    using Alpha.LogCollector.Configurations;
    using Alpha.LogCollector.Services;
    using Fanex.Data;
    using Fanex.Data.OldConnectionStringProvider;
    using Fanex.Logging;

    public partial class LogCollectorService : ServiceBase
    {
        private readonly IEnumerable<ILogCollector> logCollectors = new List<ILogCollector>
        {
            new LoginLogCollector()
        };
        private const int IntervalToGetLogsInMinutes = 12 * 60 * 60 * 1000;
        private Timer timer;

        public LogCollectorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            DbSettingProviderManager
                .StartNewSession()
                .Use(new OldConnectionStringProvider())
                .UseDefaultDbSettingProvider(resourcePath: AppSettings.StoredProcedurePath, enableWatching: false)
                .Run();

            LogService.Info($"Start Alpha.LogCollector at {DateTime.Now}");

            OnCollectingLogs();

            timer = new Timer(IntervalToGetLogsInMinutes);
            timer.Elapsed += (sender, e) => OnCollectingLogs();
            timer.Start();
        }

        private void OnCollectingLogs()
        {
            foreach (var collector in logCollectors)
            {
                collector.Collect(DateTime.Now);
            }
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
            LogService.Info($"End Alpha.LogCollector at {DateTime.Now}");
        }
    }
}