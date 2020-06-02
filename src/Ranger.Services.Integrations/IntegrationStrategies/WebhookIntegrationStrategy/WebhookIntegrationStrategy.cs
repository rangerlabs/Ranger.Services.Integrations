using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class WebhookIntegrationStrategy : IIntegrationStrategy
    {
        private readonly ILogger<WebhookIntegrationStrategy> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly HttpClient httpClient;

        public WebhookIntegrationStrategy(ILogger<WebhookIntegrationStrategy> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task Execute(string tenantId, string projectName, DomainWebhookIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults, Breadcrumb breadcrumb, EnvironmentEnum environment)
        {
            logger.LogInformation("Executing Webhook Integration Strategy for integration {Integration} in project {Project}", integration.IntegrationId, projectName);
            var httpClientId = $"webhook-{tenantId}-{integration.IntegrationId}";
            using var httpClient = httpClientFactory.CreateClient(httpClientId);

            foreach (var header in integration.Headers)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Invalid header. {Reason} - {HeaderName}: {HeaderValue}", ex.Message, header.Key, header.Value);
                    throw new RangerException("Invalid header resulting in a failed webhook request");
                }
            }

            var content = new WebhookIntegrationContent
            {
                Id = Guid.NewGuid().ToString("N"),
                Project = projectName,
                BreadCrumb = breadcrumb,
                Environment = Enum.GetName(typeof(EnvironmentEnum), environment),
                Events = geofenceIntegrationResults.Select(g => new GeofenceWebhookResult(g.GeofenceId,
                    g.GeofenceExternalId,
                    g.GeofenceDescription,
                    g.GeofenceMetadata,
                    g.GeofenceEvent)),
                IntegrationMetadata = integration.Metadata,
            };

            try
            {
                var result = await httpClient.PostAsync(integration.Url, new StringContent(JsonConvert.SerializeObject(content)));
                logger.LogWarning("Received status code {StatusCode} from webhook integration", result.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute webhook successfully.");
            }
        }
    }
}