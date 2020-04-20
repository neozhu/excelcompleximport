using System;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using WebApp.App_Helpers.third_party.api;

namespace WebApp
{
  public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
  {
    public bool Authorize([NotNull] DashboardContext context)
    {
      // In case you need an OWIN context, use the next line, `OwinContext` class
      // is the part of the `Microsoft.Owin` package.
      var owinContext = new OwinContext(context.GetOwinEnvironment());

      // Allow all authenticated users to see the Dashboard (potentially dangerous).
      return owinContext.Authentication.User.Identity.IsAuthenticated;
    }
  }

  public partial class Startup
  {
    // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
    public void ConfigureHangfire(IAppBuilder app)
    {
      GlobalConfiguration.Configuration
         .UseSimpleAssemblyNameTypeSerializer()
         .UseRecommendedSerializerSettings()
         .UseColouredConsoleLogProvider()
         .UseSqlServerStorage(
              "DefaultConnection",
              new SqlServerStorageOptions
              {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(1),
                UseRecommendedIsolationLevel = true,
                UsePageLocksOnDequeue = true,
                DisableGlobalLocks = true
              });



      app.UseHangfireDashboard("/hangfire", new DashboardOptions()
      {
        Authorization = new[] { new HangFireAuthorizationFilter() }
      });
      app.UseHangfireServer();
      //每10分钟执行一个方法
      RecurringJob.AddOrUpdate(
               () => covid19api.DownloadData(),
           Cron.Daily(5,0));
    }

    //public void ExecuteProcess() => Console.WriteLine("{ DateTime.Now }:do something......");
  }
}