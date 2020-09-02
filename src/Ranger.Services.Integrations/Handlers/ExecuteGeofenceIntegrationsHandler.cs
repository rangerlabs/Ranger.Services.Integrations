using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Handlers
{
    public class ExecuteIntegrationsHandler : ICommandHandler<ExecuteGeofenceIntegrations>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<ExecuteIntegrationsHandler> logger;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly IntegrationStrategyExecutor integrationExecutor;

        public ExecuteIntegrationsHandler(IBusPublisher busPublisher, ILogger<ExecuteIntegrationsHandler> logger, Func<string, IntegrationsRepository> integrationsRepository, IntegrationStrategyExecutor integrationExecutor)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
            this.integrationsRepository = integrationsRepository;
            this.integrationExecutor = integrationExecutor;
        }

        public async Task HandleAsync(ExecuteGeofenceIntegrations message, ICorrelationContext context)
        {
            logger.LogInformation("Executing integrations. {@Message}", message);
            var repo = integrationsRepository.Invoke(message.TenantId);

            var defaultIntegrationIds = (await repo.GetAllNotDeletedDefaultIntegrationsForProject(message.ProjectId, message.Environment)).Where(i => i.Environment == message.Environment).Select(i => i.Id);
            logger.LogDebug("Determined default integrations to be {DefaultIntegrationIds}", defaultIntegrationIds);

            var distinctIntegrationIds = message.GeofenceIntegrationResults.SelectMany(_ => _.IntegrationIds).Distinct();
            logger.LogDebug("Determined distinct integrations to be {DistinctIntegrationIds}", distinctIntegrationIds);

            var integrationIdsToExecute = defaultIntegrationIds.Union(distinctIntegrationIds);

            var integrationsToExecute = await repo.GetAllNotDeletedIntegrationsByIdsForProject(message.ProjectId, integrationIdsToExecute);

            foreach (var integration in integrationsToExecute)
            {
                logger.LogDebug("Executing integration {IntegrationId}", integration.Id);
                if (integration.IsDefault)
                {
                    logger.LogDebug("{IntegrationId} is a default integration, executing for all GeofenceIntegrationResults");
                    await integrationExecutor.Execute(message.TenantId, message.ProjectName, integration, message.GeofenceIntegrationResults, message.Breadcrumb, message.Environment);
                }
                else
                {
                    logger.LogDebug("{IntegrationId} is not a default integration, executing select GeofenceIntegrationResults");
                    var geofenceResults = message.GeofenceIntegrationResults.Where(gri => gri.IntegrationIds.Contains(integration.Id));
                    await integrationExecutor.Execute(message.TenantId, message.ProjectName, integration, geofenceResults, message.Breadcrumb, message.Environment);
                }
            }
        }
    }
}