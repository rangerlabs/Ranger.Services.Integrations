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
        private readonly IIntegrationsRepository integrationsRepository;

        public ExecuteIntegrationsHandler(IBusPublisher busPublisher, ILogger<ExecuteIntegrationsHandler> logger, IIntegrationsRepository integrationsRepository)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
            this.integrationsRepository = integrationsRepository;
        }

        public Task HandleAsync(ExecuteGeofenceIntegrations message, ICorrelationContext context)
        {
            logger.LogInformation($"Executing integrations. {JsonConvert.SerializeObject(message)}");
            return Task.CompletedTask;
        }
    }
}