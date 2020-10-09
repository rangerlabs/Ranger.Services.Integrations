using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data.DomainModels
{
    public class DomainPusherIntegration : IDomainIntegration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
        public string AppId { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Cluster { get; set; }
        public EnvironmentEnum Environment { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Deleted { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedOn { get; set; }
    }
}