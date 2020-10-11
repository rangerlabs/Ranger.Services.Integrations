using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PusherServer;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class PusherIntegrationStrategy : IIntegrationStrategy<DomainPusherIntegration>
    {
        private readonly ILogger<PusherIntegrationStrategy> logger;

        public PusherIntegrationStrategy(ILogger<PusherIntegrationStrategy> logger)
        {
            this.logger = logger;
        }

        public async Task Execute(string tenantId, string projectName, DomainPusherIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults, Breadcrumb breadcrumb, EnvironmentEnum environment)
        {
            logger.LogInformation("Executing Pusher Integration Strategy for integration {Integration} in project {Project}", integration.Id, projectName);
            try
            {
                var pusher = new Pusher(integration.AppId, integration.Key, integration.Secret, new PusherOptions { Cluster = integration.Cluster});
                var content = GetPusherIntegrationContent(projectName, integration, geofenceIntegrationResults, breadcrumb, environment);
                var result = await pusher.TriggerAsync(
                    $"ranger-{breadcrumb.DeviceId}",
                    "ranger-geofence-event",
                    content
               );
                logger.LogDebug("Received status code {StatusCode} from Pusher integration {IntegrationId}", result.StatusCode, integration.Id);
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("The webhook integration {IntegrationId} request timed out", integration.Id);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to execute webhook integration {IntegrationId} successfully - {Reason}", ex.Message, integration.Id);
            }
       }

        private static PusherIntegrationContent GetPusherIntegrationContent(string projectName, DomainPusherIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults, Breadcrumb breadcrumb, EnvironmentEnum environment)
        {
            return new PusherIntegrationContent
            {
                Id = Guid.NewGuid().ToString("N"),
                Project = projectName,
                Environment = Enum.GetName(typeof(EnvironmentEnum), environment),
                Breadcrumb = breadcrumb,
                Events = geofenceIntegrationResults.Select(g => new GeofencePusherResult(g.GeofenceId,
                    g.GeofenceExternalId,
                    g.GeofenceDescription,
                    g.GeofenceMetadata,
                    g.GeofenceEvent)),
                IntegrationMetadata = integration.Metadata,
            };
        }
    }
}