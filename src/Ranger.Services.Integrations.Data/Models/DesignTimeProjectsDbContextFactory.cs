using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ranger.Services.Integrations.Data
{
    public class DesignTimeIntegrationsDbContextFactory : IDesignTimeDbContextFactory<IntegrationsDbContext>
    {
        public IntegrationsDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<IntegrationsDbContext>();
            options.UseNpgsql(config["cloudSql:ConnectionString"]);

            return new IntegrationsDbContext(options.Options);
        }
    }
}