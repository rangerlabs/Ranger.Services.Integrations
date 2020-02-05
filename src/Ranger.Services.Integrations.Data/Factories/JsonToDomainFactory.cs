using System;
using Newtonsoft.Json;
using Ranger.Common.SharedKernel;
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
                        return JsonConvert.DeserializeObject<DomainWebhookIntegration>(jsonContent, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{integrationType}'.");
                    }
            }
        }
    }
}