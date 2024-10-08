using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
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
                var repo = integrationsRepoFactory(tenantLimit.tenantId);
                var integrations = await repo.GetAllNotDeletedIntegrationsForProjectIds(tenantLimit.remainingProjectIds);
                if (integrations.Count() > tenantLimit.limit)
                {
                    var exceededByCount = integrations.Count() - tenantLimit.limit;
                    var integrationsToRemove = integrations.OrderByDescending(p => p.CreatedOn).Take(exceededByCount);
                    foreach (var projectToRemove in integrationsToRemove)
                    {
                        await repo.SoftDeleteAsync(projectToRemove.ProjectId, "SubscriptionEnforcer", projectToRemove.Name);
                        busPublisher.Send(new PurgeIntegrationFromGeofences(tenantLimit.tenantId, projectToRemove.ProjectId, projectToRemove.Id), context);
                    }
                }
            }
        }
    }
}
