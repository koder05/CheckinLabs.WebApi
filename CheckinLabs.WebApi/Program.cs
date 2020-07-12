using CheckinLabs.AppBase;
using CheckinLabs.BL.Models;
using CheckinLabs.Data.EF;
using CheckinLabs.Data.EF.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace CheckinLabs.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                var host = CreateHostBuilder(args);
                //host
                //    .UseContentRoot(System.IO.Directory.GetCurrentDirectory())
                //    .UseKestrel()
                //    .UseUrls("http://+:54321");
                host.Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .AddStartupModule<DbModule>()
                .AddStartupModule<MigrationsModule>()
                .AddService<AppDbContext, AppSqlServerContext>(ServiceLifetime.Scoped)
                .AddService<CheckinLabs.BL.Repo.IAccountRepo, CheckinLabs.Data.EF.Repo.AccountRepo>()
                .AddService<CheckinLabs.BL.Svc.INotifier<UserCheckin>, CheckinLabs.BL.Svc.UserCheckinNotifier>()
                .AddService<CheckinLabs.BL.Svc.INotifyChannel, CheckinLabs.WebApi.Notifications.ConsoleNotifyChannel>()
                .AddService<CheckinLabs.BL.Svc.INotifyChannel, CheckinLabs.WebApi.Notifications.EmailNotifyChannel>()
                ;
    }
}
