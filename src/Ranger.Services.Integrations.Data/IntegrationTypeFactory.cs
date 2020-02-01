using System;
using Newtonsoft.Json;
using Ranger.Common.SharedKernel;

namespace Ranger.Services.Integrations.Data
{
    public static class IntegrationMessageTypeFactory
    {
        public static (IIntegration integration, Type type) Factory(IntegrationsEnum integrationType, string jsonContent)
        {
            switch (integrationType)
            {
                case IntegrationsEnum.WEBHOOK:
                    {
                        return (JsonConvert.DeserializeObject<WebhookIntegration>(jsonContent), typeof(WebhookIntegration));
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'.");
                    }
            }
        }
    }
}