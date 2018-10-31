using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Logs;
using Lykke.Service.PayInvoicePortal.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.PayInvoicePortal.Core.Services;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using System.Net;
using Common;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Common.Log;
using Lykke.Service.PayInvoicePortal.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.SignalRHubs;

namespace Lykke.Service.PayInvoicePortal
{
    public class Startup
    {
        private string _monitoringServiceUrl;

        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }
        private IHealthNotifier HealthNotifier { get; set; }

        internal static string BlockchainExplorerUrl;
        internal static string EthereumBlockchainExplorerUrl;
        internal static string ApiaryDocsDomain;
        public static string PortalTestnetUrl;
        public static DeploymentEnvironment DeploymentEnvironment;
        public static bool EnableSignup;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                var appSettings = Configuration.LoadSettings<AppSettings>();
                
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    });

                services.AddSignalR();

                services.AddAuthentication(opts =>
                    {
                        opts.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        opts.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>

                    {
                        o.LoginPath = new PathString("/auth/signin");
                        o.ExpireTimeSpan = appSettings.CurrentValue.PayInvoicePortal.UserLoginTime;
                        o.Events.OnRedirectToLogin = (context) =>
                        {
                            if (context.Request.Path.HasValue &&
                                context.Request.Path.Value.StartsWith("/api/") &&
                                context.Response.StatusCode == (int) HttpStatusCode.OK)
                            {
                                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                            }
                            else
                            {
                                /*
                                because sandbox external url configured on prod nginx to redirect
                                to another nginx url (as sandbox located in another kube)
                                the context.RedirectUri is not right in this case  
                                and has to be changed to external prod url
                                */
                                if (DeploymentEnvironment == DeploymentEnvironment.Sandbox &&
                                    !string.IsNullOrEmpty(PortalTestnetUrl))
                                {
                                    var oldRedirectUri = context.RedirectUri;

                                    var hostUrl = context.RedirectUri.Substring(0,
                                        context.RedirectUri.IndexOf(o.LoginPath.ToString(), StringComparison.Ordinal));

                                    if (hostUrl != PortalTestnetUrl.TrimEnd('/'))
                                    {
                                        context.RedirectUri =
                                            context.RedirectUri.Replace(hostUrl, PortalTestnetUrl.TrimEnd('/'));

                                        Log?.Info(
                                            $"OnRedirectToLogin, old RedirectUri: {oldRedirectUri}, " +
                                            $"new RedirectUri: {context.RedirectUri}, " +
                                            $@"context: {
                                                new
                                                {
                                                    context.Request.Host,
                                                    context.Request.Path,
                                                    context.Request.QueryString,
                                                    context.Request.Scheme
                                                }}");
                                    }
                                }

                                context.Response.Redirect(context.RedirectUri);
                            }

                            return Task.FromResult(0);
                        };
                    });
                
                services.AddAuthorization(options =>
                    {
                        options.DefaultPolicy =
                            new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                                .RequireAuthenticatedUser().Build();
                    });

                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfiles(typeof(AutoMapperProfile));
                    cfg.AddProfiles(typeof(Services.AutoMapperProfile));
                });

                Mapper.AssertConfigurationIsValid();

                var builder = new ContainerBuilder();

                services.AddLykkeLogging(
                    appSettings.ConnectionString(x => x.PayInvoicePortal.Db.LogsConnectionString),
                    "PayInvoicePortalLog",
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.QueueName);

                BlockchainExplorerUrl = appSettings.CurrentValue.PayInvoicePortal.BlockchainExplorerUrl;
                EthereumBlockchainExplorerUrl = appSettings.CurrentValue.PayInvoicePortal.EthereumBlockchainExplorerUrl;
                PortalTestnetUrl = appSettings.CurrentValue.PayInvoicePortal.PortalTestnetUrl;
                DeploymentEnvironment = appSettings.CurrentValue.PayInvoicePortal.DeploymentEnvironment.ParseEnum(DeploymentEnvironment.Prod);
                EnableSignup = appSettings.CurrentValue.PayInvoicePortal.EnableSignup;
                ApiaryDocsDomain = appSettings.CurrentValue.PayInvoicePortal.ApiaryDocsDomain;
                _monitoringServiceUrl = appSettings.CurrentValue.MonitoringServiceClient?.MonitoringServiceUrl;

                builder.RegisterModule(new Repositories.AutofacModule(
                    appSettings.Nested(o => o.PayInvoicePortal.Db.SubscriptionConnectionString)));
                builder.RegisterModule(new Services.AutofacModule(
                    appSettings.CurrentValue.PayInvoicePortal.CacheExpirationPeriods,
                    appSettings.CurrentValue.PayInvoicePortal.EnableSignup,
                    appSettings.CurrentValue.AssetsMap));
                builder.RegisterModule(new AutofacModule(appSettings));
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                Log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);

                HealthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseBrowserLink();
                    app.UseCors(corsBuilder =>
                    {
                        corsBuilder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                            .AllowCredentials();
                    });
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                }
                
                app.UseLykkeMiddleware( ex => new { Message = "Technical problem" });
                
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseStaticFiles();
                app.UseAuthentication();
                app.UseSignalR(routes =>
                {
                    routes.MapHub<InvoiceUpdateHub>("/ws/invoiceUpdateHub");
                });
                app.UseMvc(routes =>
                {
                    routes.MapRoute("default", "{controller=Welcome}/{action=Welcome}/{id?}");
                    routes.MapRoute("testng2", "ng2/dist/lykkepay/index.html");
                });

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here

                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();

                HealthNotifier?.Notify("Started");

                #if (!DEBUG)

                await AutoRegistrationInMonitoring.RegisterAsync(Configuration, _monitoringServiceUrl, Log);

                #endif
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.

                await ApplicationContainer.Resolve<IShutdownManager>().StopAsync();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

                HealthNotifier?.Notify("Terminating");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    Log.Critical(ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }
    }
}
