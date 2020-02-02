using System;
using Newtonsoft.Json;
using Ranger.Common.SharedKernel;

namespace Ranger.Services.Integrations.Data
{
    public static class IntegrationTypeFactory
    {
        public static Type Factory(IntegrationsEnum integrationType)
        {
            switch (integrationType)
            {
                case IntegrationsEnum.WEBHOOK:
                    {
                        return typeof(WebhookIntegration);
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'.");
                    }
            }
        }
    }
}