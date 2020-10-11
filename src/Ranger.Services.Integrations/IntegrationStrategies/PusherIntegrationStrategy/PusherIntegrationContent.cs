using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class PusherIntegrationContent
    {
        public string id { get; set; }
        public string project { get; set; }
        public string environment { get; set; }
        public PusherBreadcrumb breadcrumb { get; set; }
        public IEnumerable<GeofencePusherResult> events { get; set; }
        public IEnumerable<KeyValuePair<string, string>> integrationMetadata { get; set; }
    }

    public class PusherBreadcrumb {
        public string deviceId {get;set;}
        public string externalUserId  {get;set;}
        public LngLat position {get;set;}
        public double accuracy { get; set;}
        public DateTime recordedAt { get; set;}
        public DateTime acceptedAt { get;set; }
        public IEnumerable<KeyValuePair<string, string>> metadata { get; set;}
    }

    public class PusherLngLat {
        public double lat { get; set;}
        public double lng { get; set;}
    }
}