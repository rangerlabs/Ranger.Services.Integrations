using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class PusherIntegrationContent
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Environment { get; set; }
        public Breadcrumb Breadcrumb { get; set; }
        public IEnumerable<GeofencePusherResult> Events { get; set; }
        public IEnumerable<KeyValuePair<string, string>> IntegrationMetadata { get; set; }
    }
}