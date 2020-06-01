using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations
{
    public class WebhookIntegrationStrategy : IIntegrationStrategy
    {
        private readonly ILogger<WebhookIntegrationStrategy> logger;

        public WebhookIntegrationStrategy(ILogger<WebhookIntegrationStrategy> logger)
        {
            this.logger = logger;
        }

        public async Task Execute(DomainWebhookIntegration integration, GeofenceIntegrationResult geofenceIntegrationResult)
        {
            logger.LogInformation($"Executing Webhook Integration Strategy for integration: {integration.IntegrationId} on geofence: {geofenceIntegrationResult.GeofenceExternalId}");
            await Task.CompletedTask;
        }
    }
}