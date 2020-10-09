using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.Services.Integrations
{
    public class GeofencePusherResult
    {
        public GeofencePusherResult(Guid geofenceId, string geofenceExternalId, string geofenceDescription, IEnumerable<KeyValuePair<string, string>> geofenceMetadata, GeofenceEventEnum geofenceEvent)
        {
            this.GeofenceId = geofenceId;
            this.GeofenceExternalId = geofenceExternalId;
            this.GeofenceDescription = geofenceDescription;
            this.GeofenceMetadata = geofenceMetadata;
            this.GeofenceEvent = Enum.GetName(typeof(GeofenceEventEnum), geofenceEvent);
        }

        public Guid GeofenceId { get; }
        public string GeofenceExternalId { get; }
        public string GeofenceDescription { get; }
        public IEnumerable<KeyValuePair<string, string>> GeofenceMetadata { get; }
        public string GeofenceEvent { get; }
    }
}