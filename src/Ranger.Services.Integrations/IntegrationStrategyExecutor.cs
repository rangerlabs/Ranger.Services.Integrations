using System;
using System.Threading.Tasks;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations
{
    public class IntegrationStrategyExecutor
    {
        private readonly WebhookIntegrationStrategy webhookIntegrationStrategy;

        public IntegrationStrategyExecutor(WebhookIntegrationStrategy webhookIntegrationStrategy)
        {
            this.webhookIntegrationStrategy = webhookIntegrationStrategy;
        }

        public async Task Execute(IDomainIntegration integration, GeofenceIntegrationResult geofenceIntegrationResult)
        {
            switch (integration)
            {
                case DomainWebhookIntegration i:
                    {
                        await webhookIntegrationStrategy.Execute(i, geofenceIntegrationResult);
                        break;
                    }
                default: throw new ArgumentException("The integration does not match a registered integration strategy");
            };
        }
    }
}