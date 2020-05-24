using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Handlers
{
    public class DeleteIntegrationHandler : ICommandHandler<DeleteIntegration>
    {
        private readonly IBusPublisher busPublisher;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly ILogger<DeleteIntegrationHandler> logger;

        public DeleteIntegrationHandler(IBusPublisher busPublisher, Func<string, IntegrationsRepository> integrationsRepository, ILogger<DeleteIntegrationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.logger = logger;
        }

        public async Task HandleAsync(DeleteIntegration command, ICorrelationContext context)
        {
            var repo = integrationsRepository.Invoke(command.TenantId);

            try
            {
                var integrationId = await repo.SoftDeleteAsync(command.ProjectId, command.CommandingUserEmail, command.Name);
                busPublisher.Publish(new IntegrationDeleted(command.TenantId, integrationId, command.Name), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (ConcurrencyException ex)
            {
                logger.LogWarning(ex.Message);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete integration");
                throw new RangerException("Failed to delete the integration. No additional data could be provided");
            }
        }
    }
}