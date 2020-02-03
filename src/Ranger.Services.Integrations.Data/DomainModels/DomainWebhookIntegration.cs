using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common.SharedKernel;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Data.DomainModels
{
    public class DomainWebhookIntegration : IDomainIntegration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
        public string Url { get; set; }
        public EnvironmentEnum Environment { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Deleted { get; set; } = false;
    }
}