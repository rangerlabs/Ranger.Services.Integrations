using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations.IntegrationStrategies
{
    public class WebhookIntegrationStrategy : IIntegrationStrategy
    {
        private readonly ILogger<WebhookIntegrationStrategy> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string HeaderName = "x-ranger-signature";

        public WebhookIntegrationStrategy(ILogger<WebhookIntegrationStrategy> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task Execute(string tenantId, string projectName, DomainWebhookIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults, Breadcrumb breadcrumb, EnvironmentEnum environment)
        {
            logger.LogInformation("Executing Webhook Integration Strategy for integration {Integration} in project {Project}", integration.Id, projectName);
            var httpClient = httpClientFactory.CreateClient(WebhookExtensions.HttpClientName);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, integration.Url);

            AddIntegrationSpecificHeaders(integration, httpRequestMessage);
            var content = GetWebhookIntegrationContent(projectName, integration, geofenceIntegrationResults, breadcrumb, environment);
            SignWebhookRequest(httpRequestMessage, content, integration.SigningKey);
            SerializeContent(httpRequestMessage, content);
            try
            {
                var result = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
                logger.LogDebug("Received status code {StatusCode} from webhook integration {IntegrationId}", result.StatusCode, integration.Id);
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

        private static void SerializeContent(HttpRequestMessage httpRequestMessage, WebhookIntegrationContent content)
        {
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(content, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            }), Encoding.UTF8, "application/json");
        }

        private void AddIntegrationSpecificHeaders(DomainWebhookIntegration integration, HttpRequestMessage httpRequestMessage)
        {
            foreach (var header in integration.Headers)
            {
                try
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Invalid header. {Reason} - {HeaderName}: {HeaderValue}", ex.Message, header.Key, header.Value);
                    throw new RangerException("Invalid header resulting in a failed webhook request");
                }
            }
        }

        private static WebhookIntegrationContent GetWebhookIntegrationContent(string projectName, DomainWebhookIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResults, Breadcrumb breadcrumb, EnvironmentEnum environment)
        {
            return new WebhookIntegrationContent
            {
                Id = Guid.NewGuid().ToString("N"),
                Project = projectName,
                Environment = Enum.GetName(typeof(EnvironmentEnum), environment),
                Breadcrumb = breadcrumb,
                Events = geofenceIntegrationResults.Select(g => new GeofenceWebhookResult(g.GeofenceId,
                    g.GeofenceExternalId,
                    g.GeofenceDescription,
                    g.GeofenceMetadata,
                    g.GeofenceEvent)),
                IntegrationMetadata = integration.Metadata,
            };
        }

        private void SignWebhookRequest(HttpRequestMessage httpRequestMessage, WebhookIntegrationContent content, string signingKey)
        {
            using var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(content.Id));
            httpRequestMessage.Headers.Add(HeaderName, BitConverter.ToString(hash));
        }
    }
}