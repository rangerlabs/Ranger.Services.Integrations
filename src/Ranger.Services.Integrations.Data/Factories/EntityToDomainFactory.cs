using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Ranger.Services.Integrations.Data.DomainModels;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    public static class EntityToDomainFactory
    {
        internal static IDomainIntegration Factory(IEntityIntegration entityIntegration, IDataProtector dataProtector)
        {
            switch (entityIntegration)
            {
                case EntityWebhookIntegration e:
                    {
                        var domainIntegration = new DomainWebhookIntegration
                        {
                            Id = e.Id,
                            ProjectId = e.ProjectId,
                            Name = e.Name,
                            Environment = e.Environment,
                            Description = e.Description,
                            Enabled = e.Enabled,
                            Deleted = e.Deleted,
                            Url = e.Url,
                            SigningKey = e.SigningKey,
                            CreatedOn = e.CreatedOn,
                            IsDefault = e.IsDefault
                        };
                        var unprotectedHeaders = dataProtector.Unprotect(e.Headers);
                        var unprotectedMetadata = dataProtector.Unprotect(e.Metadata);
                        domainIntegration.Headers = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(unprotectedHeaders.Base64Decode());
                        domainIntegration.Metadata = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(unprotectedMetadata.Base64Decode());
                        return domainIntegration;
                    }
                    case EntityPusherIntegration e:
                    {
                        var domainIntegration = new DomainPusherIntegration
                        {
                            Id = e.Id,
                            ProjectId = e.ProjectId,
                            Name = e.Name,
                            Environment = e.Environment,
                            Description = e.Description,
                            Enabled = e.Enabled,
                            Deleted = e.Deleted,
                            AppId = e.AppId,
                            Key = e.Key,
                            Secret = e.Secret,
                            Cluster = e.Cluster,
                            CreatedOn = e.CreatedOn,
                            IsDefault = e.IsDefault
                        };
                        var unprotectedMetadata = dataProtector.Unprotect(e.Metadata);
                        domainIntegration.Metadata = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(unprotectedMetadata.Base64Decode());
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