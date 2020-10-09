using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class WebhookIntegrationContent
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Environment { get; set; }
        public Breadcrumb Breadcrumb { get; set; }
        public IEnumerable<GeofenceWebhookResult> Events { get; set; }
        public IEnumerable<KeyValuePair<string, string>> IntegrationMetadata { get; set; }
    }
}