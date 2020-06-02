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
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                c.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MaxAge = new TimeSpan(0),
                    MustRevalidate = true
                };
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                AllowAutoRedirect = false
            });
        }
    }
}