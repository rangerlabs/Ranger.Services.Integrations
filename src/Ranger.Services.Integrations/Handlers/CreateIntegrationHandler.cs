using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class CreateIntegrationHandler : ICommandHandler<CreateIntegration>
    {
        private readonly IBusPublisher busPublisher;
        private readonly Func<string, IntegrationsRepository> integrationsRepository;
        private readonly SubscriptionsHttpClient subscriptionsHttpClient;
        private readonly ProjectsHttpClient projectsHttpClient;
        private readonly ILogger<CreateIntegrationHandler> logger;

        public CreateIntegrationHandler(IBusPublisher busPublisher, Func<string, IntegrationsRepository> integrationsRepository, SubscriptionsHttpClient subscriptionsHttpClient, ProjectsHttpClient projectsHttpClient, ILogger<CreateIntegrationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.integrationsRepository = integrationsRepository;
            this.subscriptionsHttpClient = subscriptionsHttpClient;
            this.projectsHttpClient = projectsHttpClient;
            this.logger = logger;
        }

        public async Task HandleAsync(CreateIntegration command, ICorrelationContext context)
        {
            var repo = integrationsRepository.Invoke(command.TenantId);

            var limitsApiResponse = await subscriptionsHttpClient.GetSubscription<SubscriptionLimitDetails>(command.TenantId);
            var projectsApiResult = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(command.TenantId);
            var allCurrentIntegrations = await repo.GetAllNotDeletedIntegrationsForProjectIds(projectsApiResult.Result.Select(p => p.ProjectId));
            if (!limitsApiResponse.Result.Active)
            {
                throw new RangerException("Subscription is inactive");
            }
            if (allCurrentIntegrations.Count() >= limitsApiResponse.Result.Limit.Integrations)
            {
                throw new RangerException("Subscription limit met");
            }


            IEntityIntegration entityIntegration;
            try
            {
                var domainIntegration = JsonToDomainFactory.Factory(command.IntegrationType, command.MessageJsonContent);
                entityIntegration = DomainToEntityFactory.Factory(domainIntegration);
                entityIntegration.IntegrationId = Guid.NewGuid();
                entityIntegration.ProjectId = command.ProjectId;
            }
            catch (JsonSerializationException ex)
            {
                logger.LogDebug(ex, "Failed to instantiate the integration from the type and message content provided");
                throw new RangerException($"Failed to create the integration. The requested integration was malformed");
            }

            try
            {
                await repo.AddIntegrationAsync(command.CommandingUserEmail, "IntegrationCreated", entityIntegration, command.IntegrationType);
                busPublisher.Publish(new IntegrationCreated(command.TenantId, entityIntegration.Name, entityIntegration.IntegrationId), CorrelationContext.FromId(context.CorrelationContextId));
            }
            catch (EventStreamDataConstraintException ex)
            {
                logger.LogDebug(ex, "Failed to create integration {IntegrationName}", entityIntegration.Name);
                throw new RangerException(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred creating integration {IntegrationName}", entityIntegration.Name);
                throw new RangerException($"An unexpected error occurred creating integration '{entityIntegration.Name}'");
            }
        }
    }
}