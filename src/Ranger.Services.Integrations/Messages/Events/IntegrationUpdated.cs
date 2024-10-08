using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class IntegrationUpdated : IEvent
    {
        public string TenantId { get; }
        public string Name { get; }
        public Guid Id { get; }
        public bool IsDefault { get; }

        public IntegrationUpdated(string tenantId, string name, Guid id, bool isDefault)
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
            this.Name = name;
            this.Id = id;
            this.IsDefault = isDefault;
        }
    }
}