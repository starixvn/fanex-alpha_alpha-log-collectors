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

    public partial class LogCollectorService : ServiceBase
    {
        private readonly IEnumerable<ILogCollector> logCollectors = new List<ILogCollector>
        {
            new LoginLogCollector()
        };
        private const int IntervalToGetLogsInHours = 2 * 60 * 60 * 1000;
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

            OnCollectingLogs();

            timer = new Timer(IntervalToGetLogsInHours);
            timer.Elapsed += (sender, e) => OnCollectingLogs();
            timer.Start();
        }

        private void OnCollectingLogs()
        {
            foreach (var collector in logCollectors)
            {
                collector.Collect();
            }
        }

        protected override void OnStop()
        {
            timer.Dispose();
        }
    }
}