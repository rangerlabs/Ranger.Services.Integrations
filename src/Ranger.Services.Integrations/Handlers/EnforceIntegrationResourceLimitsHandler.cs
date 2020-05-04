using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Handlers
{
    public class EnforceIntegrationResourceLimitsHandler : ICommandHandler<EnforceIntegrationResourceLimits>
    {
        private readonly IBusPublisher busPublisher;
        private readonly Func<string, IntegrationsRepository> integrationsRepoFactory;
        private readonly ILogger<DeleteIntegrationHandler> logger;

        public EnforceIntegrationResourceLimitsHandler(
            IBusPublisher busPublisher,
            Func<string, IntegrationsRepository> integrationsRepository,
            ILogger<DeleteIntegrationHandler> logger
        )
        {
            this.busPublisher = busPublisher;
            this.integrationsRepoFactory = integrationsRepository;
            this.logger = logger;
        }

        public async Task HandleAsync(EnforceIntegrationResourceLimits message, ICorrelationContext context)
        {
            foreach (var tenantLimit in message.TenantLimits)
            {
                var repo = integrationsRepoFactory(tenantLimit.Item1);
                var integrations = await repo.GetAllIntegrationsForProjectIds(tenantLimit.remainingProjectIds);
                if (integrations.Count() > tenantLimit.Item2)
                {
                    var exceededByCount = integrations.Count() - tenantLimit.Item2;
                    var projectsToRemove = integrations.OrderByDescending(p => p.integration.CreatedOn).Take(exceededByCount);
                    foreach (var projectToRemove in projectsToRemove)
                    {
                        await repo.SoftDeleteAsync(projectToRemove.integration.ProjectId, "SubscriptionEnforcer", projectToRemove.integration.Name);
                    }
                }
            }
        }
    }
}
