using System;
using System.Collections.Generic;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespace("integrations")]
    public class EnforceIntegrationResourceLimits : ICommand
    {
        public IEnumerable<(string tenantId, int limit, IEnumerable<Guid> remainingProjectIds)> TenantLimits { get; }

        public EnforceIntegrationResourceLimits(IEnumerable<(string tenantId, int limit, IEnumerable<Guid> remainingProjectIds)> tenantLimits)
        {
            TenantLimits = tenantLimits;
        }
    }
}