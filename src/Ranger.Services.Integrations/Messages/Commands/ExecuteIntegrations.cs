using System;
using System.Collections.Generic;
using System.Linq;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespace("integrations")]
    public class ExecuteIntegrations : ICommand
    {
        public string DatabaseUsername { get; }
        public Guid ProjectId { get; }
        public EnvironmentEnum Environment { get; set; }
        public Breadcrumb Breadcrumb { get; }
        public IEnumerable<Guid> IntegrationIds { get; }
        public IEnumerable<KeyValuePair<string, string>> GeofenceMetadata { get; }
        public Guid GeofenceId { get; }
        public string GeofenceDescription { get; }
        public GeofenceEventEnum GeofenceEvent { get; }

        public ExecuteIntegrations(
            string databaseUsername,
            Guid projectId,
            EnvironmentEnum environment,
            Breadcrumb breadcrumb,
            IEnumerable<Guid> integrationIds,
            Guid geofenceId,
            GeofenceEventEnum geofenceEvent,
            string geofenceDescription = "",
            IEnumerable<KeyValuePair<string, string>> geofenceMetadata = null
            )
        {
            if (string.IsNullOrWhiteSpace(databaseUsername))
            {
                throw new ArgumentException($"{nameof(databaseUsername)} was null or whitespace.");
            }
            if (projectId.Equals(Guid.Empty))
            {
                throw new ArgumentException($"{nameof(projectId)} was an empty Guid.");
            }
            if (integrationIds.Count() == 0)
            {
                throw new ArgumentException($"{nameof(integrationIds)} was empty.");
            }

            this.DatabaseUsername = databaseUsername;
            this.ProjectId = projectId;
            this.Environment = environment;
            this.Breadcrumb = breadcrumb ?? throw new ArgumentNullException(nameof(breadcrumb));
            this.IntegrationIds = integrationIds ?? throw new ArgumentNullException(nameof(integrationIds));
            this.GeofenceId = geofenceId;
            this.GeofenceEvent = geofenceEvent;
            this.GeofenceDescription = geofenceDescription;
            this.GeofenceMetadata = geofenceMetadata;
        }
    }
}