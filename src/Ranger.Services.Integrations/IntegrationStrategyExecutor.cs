using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;
using Ranger.Services.Integrations.IntegrationStrategies;

namespace Ranger.Services.Integrations
{
    public class IntegrationStrategyExecutor
    {
        private readonly WebhookIntegrationStrategy webhookIntegrationStrategy;
        private readonly PusherIntegrationStrategy pusherIntegrationStrategy;

        public IntegrationStrategyExecutor(WebhookIntegrationStrategy webhookIntegrationStrategy, PusherIntegrationStrategy pusherIntegrationStrategy)
        {
            this.webhookIntegrationStrategy = webhookIntegrationStrategy;
            this.pusherIntegrationStrategy = pusherIntegrationStrategy;
        }

        public async Task Execute(string tenantId, string projectName, IDomainIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults, Breadcrumb breadcrumb, EnvironmentEnum environment)
        {
            switch (integration)
            {
                case DomainWebhookIntegration i:
                    {
                        await webhookIntegrationStrategy.Execute(tenantId, projectName, i, geofenceIntegrationResults, breadcrumb, environment);
                        break;
                    }
                case DomainPusherIntegration i:
                    {
                        await pusherIntegrationStrategy.Execute(tenantId, projectName, i, geofenceIntegrationResults, breadcrumb, environment);
                        break;
                    }
                default: throw new ArgumentException("The integration does not match a registered integration strategy");
            };
        }
    }
}