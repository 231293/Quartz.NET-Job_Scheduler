using log4net;
using System;

namespace Quartz.NET_Job_Scheduler
{
    public class HelloJob : IJob
    {
        private ILog _logger;
        public HelloJob(ILog log)
        {
            _logger = log;
        }

        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Initiating HelloJob." + DateTime.Now);
            _logger.Info("Initiating HelloJob.");
        }
    }
}
