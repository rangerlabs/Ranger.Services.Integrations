using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations
{
    public class GeofencePusherResult
    {
        public GeofencePusherResult(Guid geofenceId, string geofenceExternalId, string geofenceDescription, IEnumerable<KeyValuePair<string, string>> geofenceMetadata, GeofenceEventEnum geofenceEvent)
        {
            this.geofenceId = geofenceId;
            this.geofenceExternalId = geofenceExternalId;
            this.geofenceDescription = geofenceDescription;
            this.geofenceMetadata = geofenceMetadata;
            this.geofenceEvent = Enum.GetName(typeof(GeofenceEventEnum), geofenceEvent);
        }

        public Guid geofenceId { get; }
        public string geofenceExternalId { get; }
        public string geofenceDescription { get; }
        public IEnumerable<KeyValuePair<string, string>> geofenceMetadata { get; }
        public string geofenceEvent { get; }
    }
}