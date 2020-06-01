using System.Threading.Tasks;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations
{
    public interface IIntegrationStrategy
    {
        Task Execute(DomainWebhookIntegration integration, GeofenceIntegrationResult geofenceIntegrationResult);
    }
}