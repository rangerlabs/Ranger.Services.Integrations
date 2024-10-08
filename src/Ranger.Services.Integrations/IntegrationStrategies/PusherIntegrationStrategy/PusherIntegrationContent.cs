using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class PusherIntegrationContent
    {
        public Guid id { get; set; }
        public string project { get; set; }
        public string environment { get; set; }
        public PusherBreadcrumbWithoutId breadcrumb { get; set; }
        public IEnumerable<GeofencePusherResult> events { get; set; }
        public IEnumerable<PusherKeyValuePair> integrationMetadata { get; set; }
    }

    public class PusherBreadcrumbWithoutId
    {
        public string deviceId { get; set; }
        public string externalUserId { get; set; }
        public PusherLngLat position { get; set; }
        public double accuracy { get; set; }
        public DateTime recordedAt { get; set; }
        public DateTime acceptedAt { get; set; }
        public IEnumerable<PusherKeyValuePair> metadata { get; set; }
    }

    public class PusherLngLat
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class PusherKeyValuePair
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}