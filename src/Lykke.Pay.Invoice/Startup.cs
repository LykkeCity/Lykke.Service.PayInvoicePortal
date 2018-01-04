using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Common.Log;
using Lykke.AzureQueueIntegration;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Logs;
using Lykke.Pay.Invoice.AppCode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.Rest;

namespace Lykke.Pay.Invoice
{
    public class Startup
    {
        private ILog Log { get; set; }

        public AppSettings Settings { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            var appSettings = Configuration.LoadSettings<AppSettings>();
            Settings = appSettings.CurrentValue;

            services.AddMvc();

            services.AddAuthentication(opts => {
                    opts.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>

                {
                    o.LoginPath = new PathString("/Home/Welcome");
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(Settings.PayInvoice.UserLoginTime);
                });

            services.AddSingleton(Settings);
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            });
            services.AddSingleton<IInvoicesservice>(new Invoicesservice(new Uri(Settings.PayInvoice.InvoicesService)));

            Log = CreateLogWithSlack(services, appSettings);
            services.AddSingleton(Log);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Invoice}/{action=Index}");
            });

            //app.UseLykkeMiddleware("LykkePayInvoceWeb", ex =>
            //{
            //    string errorMessage;

            //    switch (ex)
            //    {
            //        case InvalidOperationException ioe:
            //            errorMessage = $"Invalid operation: {ioe.Message}";
            //            break;
            //        case ValidationException ve:
            //            errorMessage = $"Validation error: {ve.Message}";
            //            break;
            //        default:
            //            errorMessage = "Technical problem";
            //            break;
            //    }

            //    return Error.Create(Constants.ComponentName, errorMessage);
            //});

            app.UseMiddleware<ApiTraceMiddleware>();
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.PayInvoice.Logs.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                const string appName = "Lykke.Pay.Invoice.Web";

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager
                (
                    appName,
                    slackService,
                    consoleLogger
                );

                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager
                (
                    appName,
                    AzureTableStorage<LogEntity>.Create(settings.ConnectionString(x => x.PayInvoice.Logs.LogsConnString), "LykkePayInvoiceWebLog", consoleLogger),
                    consoleLogger
                );



                var azureStorageLogger = new LykkeLogToAzureStorage
                (
                    appName,
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger
                );

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}
