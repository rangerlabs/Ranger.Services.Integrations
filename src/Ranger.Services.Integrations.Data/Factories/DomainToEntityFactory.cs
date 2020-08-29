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
                            Id = d.Id,
                            ProjectId = d.ProjectId,
                            Name = d.Name,
                            Environment = d.Environment,
                            Description = d.Description,
                            Enabled = d.Enabled,
                            Deleted = d.Deleted,
                            Url = d.Url,
                            SigningKey = d.SigningKey,
                            CreatedOn = d.CreatedOn,
                            IsDefault = d.IsDefault
                        };
                        var encodedHeaders = (JsonConvert.SerializeObject(d.Headers) ?? "[]").Base64Encode();
                        var encodedMetadata = (JsonConvert.SerializeObject(d.Metadata) ?? "[]").Base64Encode();
                        entityIntegration.Headers = dataProtector.Protect(encodedHeaders);
                        entityIntegration.Metadata = dataProtector.Protect(encodedMetadata);
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