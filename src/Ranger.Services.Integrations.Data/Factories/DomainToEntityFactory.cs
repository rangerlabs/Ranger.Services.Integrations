using System;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Ranger.Services.Integrations.Data.DomainModels;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    public static class DomainToEntityFactory
    {
        public static IEntityIntegration Factory(IDomainIntegration domainIntegration, IDataProtector dataProtector)
        {
            switch (domainIntegration)
            {
                case DomainWebhookIntegration d:
                    {
                        var entityIntegration = new EntityWebhookIntegration
                        {
                            IntegrationId = d.IntegrationId,
                            ProjectId = d.ProjectId,
                            Name = d.Name,
                            Environment = d.Environment,
                            Description = d.Description,
                            Enabled = d.Enabled,
                            Deleted = d.Deleted,
                            Url = d.Url,
                            SigningKey = d.SigningKey
                        };
                        entityIntegration.Headers = dataProtector.Protect(JsonConvert.SerializeObject(d.Headers) ?? "[]");
                        entityIntegration.Metadata = dataProtector.Protect(JsonConvert.SerializeObject(d.Metadata) ?? "[]");
                        return entityIntegration;
                    }
                default:
                    {
                        throw new ArgumentException($"No Integration Type associated with '{domainIntegration.GetType()}'");
                    }
            }
        }
    }
}