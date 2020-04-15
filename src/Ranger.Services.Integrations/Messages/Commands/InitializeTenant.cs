using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespace("integrations")]
    public class InitializeTenant : ICommand
    {
        public string TenantId { get; }
        public string DatabasePassword { get; }

        public InitializeTenant(string tenantId, string databasePassword)
        {
            this.TenantId = tenantId;
            this.DatabasePassword = databasePassword;
        }
    }
}