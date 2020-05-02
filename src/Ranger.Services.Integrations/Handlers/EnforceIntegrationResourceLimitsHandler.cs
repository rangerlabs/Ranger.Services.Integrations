using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Handlers
{
    public class EnforceIntegrationResourceLimitsHandler : ICommandHandler<EnforceIntegrationResourceLimits>
    {
        private readonly IBusPublisher busPublisher;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly ILogger<DeleteIntegrationHandler> logger;

        public EnforceIntegrationResourceLimitsHandler(
            IBusPublisher busPublisher,
            Func<string, IntegrationsRepository> integrationsRepository,
            ILogger<DeleteIntegrationHandler> logger
        )
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.logger = logger;
        }

        public Task HandleAsync(EnforceIntegrationResourceLimits message, ICorrelationContext context)
        {
            logger.LogInformation("Enforcing integration resource limits");
            return Task.CompletedTask;
            // var repo = integrationsRepository(message.TenantId);
        }
    }
}