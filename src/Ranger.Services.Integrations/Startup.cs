using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;
        private IBusSubscriber busSubscriber;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddAutoWrapper();
            services.AddSwaggerGen("Integrations API", "v1");
            services.AddApiVersioning(o => o.ApiVersionReader = new HeaderApiVersionReader("api-version"));

            services.AddDbContext<IntegrationsDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );

            var identityAuthority = configuration["httpClient:identityAuthority"];
            services.AddPollyPolicyRegistry();
            services.AddTenantsHttpClient("http://tenants:8082", identityAuthority, "tenantsApi", "cKprgh9wYKWcsm");
            services.AddProjectsHttpClient("http://projects:8086", identityAuthority, "projectsApi", "usGwT8Qsp4La2");
            services.AddSubscriptionsHttpClient("http://subscriptions:8089", identityAuthority, "subscriptionsApi", "4T3SXqXaD6GyGHn4RY");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIntegrationsDbContextInitializer, IntegrationsDbContextInitializer>();
            services.AddTransient<ILoginRoleRepository<IntegrationsDbContext>, LoginRoleRepository<IntegrationsDbContext>>();
            services.AddTransient<IIntegrationsRepository, IntegrationsRepository>();

            //Typed Integration HttpClients
            services.AddHttpClient<WebhookService>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "integrationsApi";

                    options.RequireHttpsMetadata = false;
                });

            services.AddDataProtection()
                .SetApplicationName("Integrations")
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<IntegrationsDbContext>();

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<IntegrationsDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance<CloudSqlOptions>(configuration.GetOptions<CloudSqlOptions>("cloudSql"));
            builder.RegisterType<IntegrationsDbContext>().InstancePerDependency();
            builder.RegisterType<TenantServiceDbContext>();
            builder.RegisterType<WebhookIntegrationStrategy>().InstancePerDependency();
            builder.RegisterType<IntegrationStrategyExecutor>().InstancePerDependency();
            builder.Register((c, p) =>
            {
                var provider = c.Resolve<TenantServiceDbContext>();
                var (dbContextOptions, model) = provider.GetDbContextOptions<IntegrationsDbContext>(p.TypedAs<string>());
                return new IntegrationUniqueConstraintRepository(model, new IntegrationsDbContext(dbContextOptions), c.Resolve<ILogger<IntegrationUniqueConstraintRepository>>());
            });
            builder.Register((c, p) =>
            {
                var provider = c.Resolve<TenantServiceDbContext>();
                var (dbContextOptions, model) = provider.GetDbContextOptions<IntegrationsDbContext>(p.TypedAs<string>());
                return new IntegrationsRepository(model, new IntegrationsDbContext(dbContextOptions), c.Resolve<ILogger<IntegrationsRepository>>());
            });
            builder.AddRabbitMq();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseSwagger("v1", "Integrations API");
            app.UseAutoWrapper();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
            });

            this.busSubscriber = app.UseRabbitMQ()
                .SubscribeCommand<InitializeTenant>((c, e) =>
                   new InitializeTenantRejected(e.Message, "")
                )
                .SubscribeCommand<CreateIntegration>((c, e) =>
                    new CreateIntegrationRejected(e.Message, "")
                )
                .SubscribeCommand<UpdateIntegration>((c, e) =>
                    new UpdateIntegrationRejected(e.Message, "")
                )
                .SubscribeCommand<DeleteIntegration>((c, e) =>
                    new DeleteIntegrationRejected(e.Message, "")
                )
                .SubscribeCommand<ExecuteGeofenceIntegrations>()
                .SubscribeCommand<EnforceIntegrationResourceLimits>();
        }

        private void OnShutdown()
        {
            this.busSubscriber.Dispose();
        }
    }
}