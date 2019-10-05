using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class TenantInitialized : IEvent
    {
        public TenantInitialized() { }
    }
}