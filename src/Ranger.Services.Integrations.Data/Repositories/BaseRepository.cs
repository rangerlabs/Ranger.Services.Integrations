using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data
{
    public class BaseRepository<TRepository>
        where TRepository : IRepository
    {
        public delegate TRepository Factory(ContextTenant contextTenant);
        protected readonly IntegrationsDbContext Context;
        private readonly ILogger<BaseRepository<TRepository>> logger;

        public BaseRepository(ContextTenant contextTenant, IntegrationsDbContext.Factory context, CloudSqlOptions cloudSqlOptions, ILogger<BaseRepository<TRepository>> logger)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (contextTenant is null)
            {
                throw new System.ArgumentNullException(nameof(contextTenant));
            }

            if (cloudSqlOptions is null)
            {
                throw new System.ArgumentNullException(nameof(cloudSqlOptions));
            }

            NpgsqlConnectionStringBuilder connectionBuilder = new NpgsqlConnectionStringBuilder(cloudSqlOptions.ConnectionString);
            connectionBuilder.Username = contextTenant.DatabaseUsername;
            connectionBuilder.Password = contextTenant.DatabasePassword;

            var options = new DbContextOptionsBuilder<IntegrationsDbContext>();
            options.UseNpgsql(connectionBuilder.ToString());
            this.Context = context.Invoke(options.Options);
            this.logger = logger;
        }
    }
}