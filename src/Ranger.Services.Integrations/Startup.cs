using System.Security.Cryptography.X509Certificates;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Redis;
using Ranger.Services.Integrations.Data;
using Ranger.Services.Integrations.IntegrationStrategies;

namespace Ranger.Services.Integrations
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;

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
                    options.Filters.Add<OperationCanceledExceptionFilter>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddRangerApiVersioning();
            services.ConfigureAutoWrapperModelStateResponseFactory();

            services.AddWebhookIntegrationHttpClient();
            services.AddSwaggerGen("Integrations API", "v1");

            services.AddDbContext<IntegrationsDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            });

            services.AddRedis(configuration["redis:ConnectionString"]);

            var identityAuthority = configuration["httpClient:identityAuthority"];
            services.AddPollyPolicyRegistry();
            services.AddTenantsHttpClient("http://tenants:8082", identityAuthority, "tenantsApi", "cKprgh9wYKWcsm");
            services.AddProjectsHttpClient("http://projects:8086", identityAuthority, "projectsApi", "usGwT8Qsp4La2");
            services.AddSubscriptionsHttpClient("http://subscriptions:8089", identityAuthority, "subscriptionsApi", "4T3SXqXaD6GyGHn4RY");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIntegrationsDbContextInitializer, IntegrationsDbContextInitializer>();
            services.AddTransient<ILoginRoleRepository<IntegrationsDbContext>, LoginRoleRepository<IntegrationsDbContext>>();
            services.AddTransient<IIntegrationsRepository, IntegrationsRepository>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "integrationsApi";

                    options.RequireHttpsMetadata = false;
                });
            // Workaround for MAC validation issues on MacOS
            if (configuration.IsIntegrationTesting())
            {
                services.AddDataProtection()
                   .SetApplicationName("Integrations")
                   .PersistKeysToDbContext<IntegrationsDbContext>();
            }
            else
            {
                services.AddDataProtection()
                    .SetApplicationName("Integrations")
                    .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .PersistKeysToDbContext<IntegrationsDbContext>();
            }

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<IntegrationsDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance<CloudSqlOptions>(configuration.GetOptions<CloudSqlOptions>("cloudSql"));
            builder.RegisterType<IntegrationsDbContext>().InstancePerDependency();
            builder.RegisterType<TenantServiceDbContextProvider>();
            builder.RegisterType<WebhookIntegrationStrategy>().InstancePerDependency();
            builder.RegisterType<IntegrationStrategyExecutor>().InstancePerDependency();
            builder.Register((c, p) =>
            {
                var provider = c.Resolve<TenantServiceDbContextProvider>();
                var (dbContextOptions, model) = provider.GetDbContextOptions<IntegrationsDbContext>(p.TypedAs<string>());
                return new IntegrationUniqueConstraintRepository(model, new IntegrationsDbContext(dbContextOptions), c.Resolve<ILogger<IntegrationUniqueConstraintRepository>>());
            });
            builder.Register((c, p) =>
            {
                var provider = c.Resolve<TenantServiceDbContextProvider>();
                var (dbContextOptions, model) = provider.GetDbContextOptions<IntegrationsDbContext>(p.TypedAs<string>());
                return new IntegrationsRepository(model, new IntegrationsDbContext(dbContextOptions), c.Resolve<ILogger<IntegrationsRepository>>(), c.Resolve<IDataProtectionProvider>());
            });
            builder.AddRabbitMq();
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            app.UseSwagger("v1", "Integrations API");
            app.UseAutoWrapper();
            app.UseUnhandedExceptionLogger();
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
            app.UseRabbitMQ()
                .SubscribeCommandWithHandler<InitializeTenant>((c, e) =>
                   new InitializeTenantRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<CreateIntegration>((c, e) =>
                    new CreateIntegrationRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<UpdateIntegration>((c, e) =>
                    new UpdateIntegrationRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<DeleteIntegration>((c, e) =>
                    new DeleteIntegrationRejected(e.Message, "")
                )
                .SubscribeCommandWithHandler<ExecuteGeofenceIntegrations>()
                .SubscribeCommandWithHandler<EnforceIntegrationResourceLimits>();
        }
    }
}