namespace Quartz.NET_Job_Scheduler
{
    using log4net;
    using Quartz;

    public class SampleJobService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SampleJobService));
        private readonly IScheduler _scheduler;

        public SampleJobService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public bool Start()
        {
            _logger.Info("SampleJobService starting.");
            if (!_scheduler.IsStarted)
            {
                _scheduler.Start();
            }
            _logger.Info("SampleJobService started.");
            return true;
        }

        public bool Stop()
        {
            _logger.Info("SampleJobService stopping.");
            _scheduler.Shutdown(true);
            _logger.Info("SampleJobService stopped.");
            return true;
        }
    }
}
