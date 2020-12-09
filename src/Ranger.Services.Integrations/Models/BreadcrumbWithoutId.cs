using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations
{
    public class BreadcrumbWithoutId
    {
        public string DeviceId { get; set; }
        public string ExternalUserId { get; set; }
        public LngLat Position { get; set; }
        public double Accuracy { get; set; }
        public DateTime RecordedAt { get; set; }
        public DateTime AcceptedAt { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
    }
}