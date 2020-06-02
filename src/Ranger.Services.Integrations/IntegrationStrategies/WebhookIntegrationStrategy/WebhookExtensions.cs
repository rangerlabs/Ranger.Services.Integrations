using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Ranger.Services.Integrations.IntegrationStrategies
{


    public static class WebhookExtensions
    {
        public static readonly string HttpClientName = "webhook";
        public static void AddWebhookIntegrationHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient(HttpClientName, c =>
            {
                c.Timeout = TimeSpan.FromSeconds(7);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                AllowAutoRedirect = false
            });
        }
    }
}