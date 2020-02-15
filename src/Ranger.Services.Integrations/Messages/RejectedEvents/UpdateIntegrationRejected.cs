using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class UpdateIntegrationRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public UpdateIntegrationRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}