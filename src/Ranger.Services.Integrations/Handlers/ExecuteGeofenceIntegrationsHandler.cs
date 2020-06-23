using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.RabbitMQ;
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

            var distinctIntegrationIds = message.GeofenceIntegrationResults.SelectMany(_ => _.IntegrationIds).Distinct();
            logger.LogDebug("Determined distinct integrations to be {DistinctIntegrationIds}", distinctIntegrationIds);

            var distinctIntegrations = await repo.GetAllNotDeletedIntegrationsByIdsForProject(message.ProjectId, distinctIntegrationIds);

            foreach (var integration in distinctIntegrations)
            {
                var geofenceResults = message.GeofenceIntegrationResults.Where(gri => gri.IntegrationIds.Contains(integration.IntegrationId));
                await integrationExecutor.Execute(message.TenantId, message.ProjectName, integration, geofenceResults, message.Breadcrumb, message.Environment);
            }
        }
    }
}