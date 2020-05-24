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

        public ExecuteIntegrationsHandler(IBusPublisher busPublisher, ILogger<ExecuteIntegrationsHandler> logger, Func<string, IntegrationsRepository> integrationsRepository)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
            this.integrationsRepository = integrationsRepository;
        }

        public async Task HandleAsync(ExecuteGeofenceIntegrations message, ICorrelationContext context)
        {
            logger.LogInformation($"Executing integrations. {JsonConvert.SerializeObject(message)}");
            var repo = integrationsRepository.Invoke(message.TenantId);

            var projectIntegrations = await repo.GetAllIntegrationsByIdForProject(message.ProjectId, message.GeofenceIntegrationResults.SelectMany(_ => _.IntegrationIds));
            return;
        }
    }
}