using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Handlers
{
    public class UpdateIntegrationHandler : ICommandHandler<UpdateIntegration>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ITenantsClient tenantsClient;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly ILogger<UpdateIntegrationHandler> logger;

        public UpdateIntegrationHandler(IBusPublisher busPublisher, Func<string, IntegrationsRepository> integrationsRepository, ITenantsClient tenantsClient, ILogger<UpdateIntegrationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateIntegration command, ICorrelationContext context)
        {
            var repo = integrationsRepository.Invoke(command.Domain);

            IIntegration entityIntegration;
            try
            {
                var domainIntegration = JsonToDomainFactory.Factory(command.IntegrationType, command.MessageJsonContent);
                entityIntegration = DomainToEntityFactory.Factory(domainIntegration);
            }
            catch (JsonSerializationException ex)
            {
                logger.LogError(ex, "Failed to instantiate the integration from the type and message content provided.");
                throw new RangerException($"Failed to update the integration. The requested integration was malformed.");
            }

            try
            {
                await repo.UpdateIntegrationAsync(command.ProjectId, command.CommandingUserEmail, "IntegrationUpdated", command.Version, entityIntegration);
                busPublisher.Publish(new IntegrationUpdated(command.Domain, entityIntegration.Name, entityIntegration.Id), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (EventStreamDataConstraintException ex)
            {
                logger.LogWarning(ex.Message);
                throw new RangerException(ex.Message);
            }
            catch (NoOpException ex)
            {
                logger.LogWarning(ex.Message);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create an integration.");
                throw new RangerException("Failed to update the integration. No additional data could be provided.");
            }
        }
    }
}