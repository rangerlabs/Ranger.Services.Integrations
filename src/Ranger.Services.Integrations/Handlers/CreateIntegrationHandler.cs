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
        private readonly ITenantsClient tenantsClient;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly ILogger<CreateIntegrationHandler> logger;

        public CreateIntegrationHandler(IBusPublisher busPublisher, Func<string, IntegrationsRepository> integrationsRepository, ITenantsClient tenantsClient, ILogger<CreateIntegrationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateIntegration command, ICorrelationContext context)
        {
            var repo = integrationsRepository.Invoke(command.Domain);

            IIntegration integration;
            try
            {
                integration = IntegrationMessageFactory.Factory(command.IntegrationType, command.MessageJsonContent);
                integration.Id = Guid.NewGuid();
                integration.ProjectId = command.ProjectId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to instantiate the integration from the type and message content provided.");
                throw new RangerException("Failed to create the integration. No additional data could be provided.");
            }

            try
            {
                await repo.AddIntegrationAsync(command.CommandingUserEmail, "IntegrationCreated", integration, command.IntegrationType);
                busPublisher.Publish(new IntegrationCreated(command.Domain, integration.Name, integration.Id), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (EventStreamDataConstraintException ex)
            {
                logger.LogWarning(ex.Message);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create an integration.");
                throw new RangerException("Failed to create the integration. No additional data could be provided.");
            }
        }
    }
}