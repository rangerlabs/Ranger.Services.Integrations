using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Handlers
{
    public class ExecuteIntegrationsHandler : ICommandHandler<ExecuteIntegrations>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<ExecuteIntegrationsHandler> logger;
        private readonly IIntegrationsRepository geofenceRepository;

        public ExecuteIntegrationsHandler(IBusPublisher busPublisher, ILogger<ExecuteIntegrationsHandler> logger, IIntegrationsRepository geofenceRepository)
        {
            this.busPublisher = busPublisher;
            this.logger = logger;
            this.geofenceRepository = geofenceRepository;
        }

        public Task HandleAsync(ExecuteIntegrations message, ICorrelationContext context)
        {
            return Task.CompletedTask;
        }
    }
}