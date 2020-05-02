using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespace("integrations")]
    public class EnforceIntegrationResourceLimits : ICommand
    {
        public IEnumerable<(string, int)> TenantLimits { get; }

        public EnforceIntegrationResourceLimits(IEnumerable<(string, int)> tenantLimits)
        {
            TenantLimits = tenantLimits;
        }
    }
}