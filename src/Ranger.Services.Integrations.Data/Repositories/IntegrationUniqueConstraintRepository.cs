using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Data
{
    public class IntegrationUniqueConstraintRepository : IIntegrationUniqueContraintRepository
    {
        private readonly ILogger<IntegrationUniqueConstraintRepository> logger;
        private readonly IntegrationsDbContext context;

        public IntegrationUniqueConstraintRepository(ContextTenant contextTenant, IntegrationsDbContext context, ILogger<IntegrationUniqueConstraintRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<bool> GetIntegrationNameAvailableByProjectAsync(Guid projectId, string name)
        {
            return await this.context.IntegrationUniqueConstraints.AnyAsync(_ => _.ProjectId == projectId && _.Name == name);
        }
    }
}