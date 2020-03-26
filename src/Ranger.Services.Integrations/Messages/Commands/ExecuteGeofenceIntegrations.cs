using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    public class ExecuteGeofenceIntegrations : ICommand
    {
        public string Domain { get; }
        public Guid ProjectId { get; }
        public Common.Breadcrumb Breadcrumb { get; }
        public IEnumerable<GeofenceIntegrationResult> GeofenceIntegrationResults { get; }
        public EnvironmentEnum Environment { get; }

        public ExecuteGeofenceIntegrations(string domain, Guid projectId, EnvironmentEnum environment, Common.Breadcrumb breadcrumb, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults)
        {
            this.Environment = environment;
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }

            this.GeofenceIntegrationResults = geofenceIntegrationResults ??
                throw new ArgumentNullException(nameof(geofenceIntegrationResults));
            this.Breadcrumb = breadcrumb ??
                throw new ArgumentNullException(nameof(breadcrumb));
            this.ProjectId = projectId;
            this.Domain = domain;
        }
    }

    public class GeofenceIntegrationResult
    {
        public GeofenceIntegrationResult(Guid geofenceId, string geofenceExternalId, string geofenceDescription, IEnumerable<KeyValuePair<string, string>> geofenceMetadata, IEnumerable<Guid> integrationIds, GeofenceEventEnum geofenceEvent)
        {
            this.GeofenceId = geofenceId;
            this.GeofenceExternalId = geofenceExternalId;
            this.GeofenceDescription = geofenceDescription;
            this.GeofenceMetadata = geofenceMetadata;
            this.IntegrationIds = integrationIds;
            this.GeofenceEvent = geofenceEvent;
        }

        public Guid GeofenceId { get; }
        public string GeofenceExternalId { get; }
        public string GeofenceDescription { get; }
        public IEnumerable<KeyValuePair<string, string>> GeofenceMetadata { get; }
        public IEnumerable<Guid> IntegrationIds { get; }
        public GeofenceEventEnum GeofenceEvent { get; }
    }
}