using System;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    public static class EntityIntegrationTypeFactory
    {
        public static Type Factory(IntegrationsEnum integrationType)
        {
            switch (integrationType)
            {
                case IntegrationsEnum.WEBHOOK:
                    {
                        return typeof(EntityWebhookIntegration);
                    }
                case IntegrationsEnum.PUSHER:
                    {
                        return typeof(EntityPusherIntegration);
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'");
                    }
            }
        }
    }
}