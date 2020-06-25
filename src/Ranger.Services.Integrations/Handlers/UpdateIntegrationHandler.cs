using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Integrations.Data;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Handlers
{
    public class UpdateIntegrationHandler : ICommandHandler<UpdateIntegration>
    {
        private readonly IBusPublisher busPublisher;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly ILogger<UpdateIntegrationHandler> logger;
        private readonly IDataProtector dataProtector;

        public UpdateIntegrationHandler(IBusPublisher busPublisher, Func<string, IntegrationsRepository> integrationsRepository, ILogger<UpdateIntegrationHandler> logger, IDataProtectionProvider dataProtectionProvider)
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.logger = logger;
            this.dataProtector = dataProtectionProvider.CreateProtector(nameof(UpdateIntegrationHandler));
        }

        public async Task HandleAsync(UpdateIntegration command, ICorrelationContext context)
        {
            var repo = integrationsRepository.Invoke(command.TenantId);

            IEntityIntegration entityIntegration;
            try
            {
                var domainIntegration = JsonToDomainFactory.Factory(command.IntegrationType, command.MessageJsonContent);
                entityIntegration = DomainToEntityFactory.Factory(domainIntegration, dataProtector);
            }
            catch (JsonSerializationException ex)
            {
                logger.LogDebug(ex, "Failed to instantiate the integration from the type and message content provided");
                throw new RangerException($"Failed to update the integration. The requested integration was malformed");
            }

            try
            {
                await repo.UpdateIntegrationAsync(command.ProjectId, command.CommandingUserEmail, "IntegrationUpdated", command.Version, entityIntegration);
                busPublisher.Publish(new IntegrationUpdated(command.TenantId, entityIntegration.Name, entityIntegration.IntegrationId), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (EventStreamDataConstraintException ex)
            {
                logger.LogDebug(ex, "Failed to update integration {IntegrationName}", entityIntegration.Name);
                throw new RangerException(ex.Message);
            }
            catch (NoOpException ex)
            {
                logger.LogDebug(ex, "Failed to update integration {IntegrationName}", entityIntegration.Name);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred updating integration {IntegrationName}", entityIntegration.Name);
                throw new RangerException($"An unexpected error occurred updating integration '{entityIntegration.Name}'");
            }
        }
    }
}