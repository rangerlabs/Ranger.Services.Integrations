using System.Net.Http;
using System.Threading.Tasks;

namespace Ranger.Services.Integrations
{
    public class WebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly string _remoteServiceBaseUrl;

        public WebhookService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task SendWebhookRequest()
        {
            return Task.CompletedTask;
        }
    }
}