using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ranger.Common;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    internal static class JsonToEntityFactory
    {
        public static IEntityIntegration Factory(IntegrationsEnum integrationType, string jsonContent)
        {
            switch (integrationType)
            {
                case IntegrationsEnum.WEBHOOK:
                    {
                        return JsonConvert.DeserializeObject<EntityWebhookIntegration>(jsonContent, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
                    }
                case IntegrationsEnum.PUSHER:
                    {
                        return JsonConvert.DeserializeObject<EntityPusherIntegration>(jsonContent, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'");
                    }
            }
        }
    }
}