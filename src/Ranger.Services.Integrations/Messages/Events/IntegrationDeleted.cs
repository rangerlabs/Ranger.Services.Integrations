using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class IntegrationDeleted : IEvent
    {
        public string TenantId { get; }
        public Guid Id { get; }
        public string Name { get; }

        public IntegrationDeleted(string tenantId, Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace");
            }

            this.TenantId = tenantId;
            this.Id = id;
            this.Name = name;
        }
    }
}