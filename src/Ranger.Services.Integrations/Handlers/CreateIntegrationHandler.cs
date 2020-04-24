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
    public class CreateIntegrationHandler : ICommandHandler<CreateIntegration>
    {
        private readonly IBusPublisher busPublisher;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly ILogger<CreateIntegrationHandler> logger;

        public CreateIntegrationHandler(IBusPublisher busPublisher, Func<string, IntegrationsRepository> integrationsRepository, ILogger<CreateIntegrationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateIntegration command, ICorrelationContext context)
        {
            var repo = integrationsRepository.Invoke(command.TenantId);

            IIntegration entityIntegration;
            try
            {
                var domainIntegration = JsonToDomainFactory.Factory(command.IntegrationType, command.MessageJsonContent);
                entityIntegration = DomainToEntityFactory.Factory(domainIntegration);
                entityIntegration.IntegrationId = Guid.NewGuid();
                entityIntegration.ProjectId = command.ProjectId;
            }
            catch (JsonSerializationException ex)
            {
                logger.LogError(ex, "Failed to instantiate the integration from the type and message content provided");
                throw new RangerException($"Failed to create the integration. The requested integration was malformed");
            }

            try
            {
                await repo.AddIntegrationAsync(command.CommandingUserEmail, "IntegrationCreated", entityIntegration, command.IntegrationType);
                busPublisher.Publish(new IntegrationCreated(command.TenantId, entityIntegration.Name, entityIntegration.IntegrationId), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (EventStreamDataConstraintException ex)
            {
                logger.LogWarning(ex.Message);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create an integration");
                throw new RangerException("Failed to create the integration. No additional data could be provided");
            }
        }
    }
}