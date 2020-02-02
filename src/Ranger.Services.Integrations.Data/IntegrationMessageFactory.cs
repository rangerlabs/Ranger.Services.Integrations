using System;
using Newtonsoft.Json;
using Ranger.Common.SharedKernel;

namespace Ranger.Services.Integrations.Data
{
    public static class IntegrationMessageFactory
    {
        public static IIntegration Factory(IntegrationsEnum integrationType, string jsonContent)
        {
            switch (integrationType)
            {
                case IntegrationsEnum.WEBHOOK:
                    {
                        var integration = JsonConvert.DeserializeObject<WebhookIntegration>(jsonContent);
                        integration.HeadersJson = integration.HeadersJson ?? "[]";
                        integration.MetadataJson = integration.MetadataJson ?? "[]";
                        return integration;
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'.");
                    }
            }
        }
    }
}