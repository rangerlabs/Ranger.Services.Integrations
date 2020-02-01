using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class CreateIntegrationRejected : IRejectedEvent
    {
        public string Reason { get; }
        public string Code { get; }

        public CreateIntegrationRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }
    }
}