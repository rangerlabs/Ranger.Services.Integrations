using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data.DomainModels
{
    public class DomainWebhookIntegration : IDomainIntegration
    {
        public Guid IntegrationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
        public string Url { get; set; }
        [JsonIgnore]
        public string SigningKey { get; set; }
        public EnvironmentEnum Environment { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Deleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
    }
}