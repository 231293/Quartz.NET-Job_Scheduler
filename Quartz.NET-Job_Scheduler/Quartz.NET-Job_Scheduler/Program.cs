using Autofac;
using Autofac.Extras.Quartz;
using log4net;
using log4net.Config;
using Quartz.Impl;
using Quartz.Spi;
using System;
using Topshelf;
using Topshelf.Autofac;
using Topshelf.Quartz;
using Topshelf.ServiceConfigurators;

namespace Quartz.NET_Job_Scheduler
{
    internal class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        private static IContainer _container;

        public static void Main()
        {
            try
            {
                _container = ConfigureContainer(new ContainerBuilder()).Build();
                var assemblyName = typeof(Program).Assembly.GetName().Name;
                ConfigureEndpoint();

                HostFactory.Run(conf =>
                {
                    log4net.Config.XmlConfigurator.Configure();

                    conf.SetServiceName(assemblyName);
                    conf.SetDisplayName(assemblyName);
                    conf.UseLog4Net();
                    conf.UseAutofacContainer(_container);

                    conf.Service<SampleJobService>(svc =>
                    {
                        svc.ConstructUsingAutofacContainer();
                        svc.WhenStarted(o => o.Start());
                        svc.WhenStopped(o =>
                        {
                            o.Stop();
                            _container.Dispose();
                        });
                        ConfigureJob(svc);
                    });
                });
                LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.Error("Exception during startup", ex);
                LogManager.Shutdown();
            }
            Console.ReadKey();
        }

        private static void ConfigureEndpoint()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler();
        }

        private static ContainerBuilder ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new QuartzAutofacFactoryModule());
            RegisterJobs(builder);
            builder.RegisterType<SampleJobService>().AsSelf();
            XmlConfigurator.Configure(); //configure logger
            builder.Register(c => log4net.LogManager.GetLogger(typeof(Object))).As<log4net.ILog>();
            return builder;
        }

        private static void RegisterJobs(ContainerBuilder builder)
        {
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(HelloJob).Assembly));
        }

        private static void ConfigureJob(ServiceConfigurator<SampleJobService> svc)
        {
            svc.UsingQuartzJobFactory(() => _container.Resolve<IJobFactory>());

            var groupKey = typeof(SampleJobService).Name;

            var jobKeyBidClosedEU = new JobKey(typeof(HelloJob).Name, groupKey);
            //var utcHour = 1;
            //var utcHourMin = 2;
            svc.ScheduleQuartzJob(q =>
            {
                q.WithJob(JobBuilder.Create<HelloJob>()
                    .WithIdentity(jobKeyBidClosedEU)
                    .Build);
                q.AddTrigger(() => TriggerBuilder.Create().WithSimpleSchedule().StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever())
                    .Build()
                        //.WithSchedule(CronScheduleBuilder
                        //    .DailyAtHourAndMinute(utcHour, utcHourMin)
                        //    .InTimeZone(TimeZoneInfo.Utc)
                        //    .WithMisfireHandlingInstructionFireAndProceed())
                        //    .Build()
                        );
            });
        }
    }
}
