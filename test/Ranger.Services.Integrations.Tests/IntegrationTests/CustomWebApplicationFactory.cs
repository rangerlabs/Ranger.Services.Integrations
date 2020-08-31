using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Production);

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                services.AddDbContext<IntegrationsDbContext>(options =>
                    {
                        options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
                    });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<CustomWebApplicationFactory>>();
                    var context = scope.ServiceProvider.GetRequiredService<IntegrationsDbContext>();
                    context.Database.Migrate();
                }
            });
        }
    }
}