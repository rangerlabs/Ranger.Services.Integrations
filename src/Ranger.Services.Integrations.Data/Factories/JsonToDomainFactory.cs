using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations.Data
{
    public static class JsonToDomainFactory
    {
        public static IDomainIntegration Factory(IntegrationsEnum integrationType, string jsonContent)
        {
            switch (integrationType)
            {
                case IntegrationsEnum.WEBHOOK:
                    {
                        var webhookIntegration = JsonConvert.DeserializeObject<DomainWebhookIntegration>(jsonContent, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                        webhookIntegration.SigningKey = Crypto.GenerateSudoRandomAlphaNumericString(64);
                        return webhookIntegration;
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'");
                    }
            }
        }
    }
}