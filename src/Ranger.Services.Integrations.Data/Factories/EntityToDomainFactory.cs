using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ranger.Services.Integrations.Data.DomainModels;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    public static class EntityToDomainFactory
    {
        internal static IDomainIntegration Factory(IEntityIntegration entityIntegration)
        {
            switch (entityIntegration)
            {
                case EntityWebhookIntegration e:
                    {
                        var domainIntegration = new DomainWebhookIntegration
                        {
                            IntegrationId = e.IntegrationId,
                            ProjectId = e.ProjectId,
                            Name = e.Name,
                            Environment = e.Environment,
                            Description = e.Description,
                            Enabled = e.Enabled,
                            Deleted = e.Deleted,
                            Url = e.Url,
                            SigningKey = e.SigningKey
                        };
                        domainIntegration.Headers = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(e.Headers);
                        domainIntegration.Metadata = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(e.Metadata);
                        return domainIntegration;
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{entityIntegration.GetType()}'");
                    }
            }
        }
    }
}