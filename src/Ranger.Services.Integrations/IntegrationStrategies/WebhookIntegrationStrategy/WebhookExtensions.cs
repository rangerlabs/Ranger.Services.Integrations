using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ranger.Services.Integrations
{
    public class WebhookExtensions
    {
        static IServiceCollection AddWebhookHttpClient(IServiceCollection services, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' was null or whitespace", nameof(tenantId));
            }

            services.AddHttpClient(tenantId).SetHandlerLifetime(TimeSpan.FromHours(2));
            return services;
        }
    }
}