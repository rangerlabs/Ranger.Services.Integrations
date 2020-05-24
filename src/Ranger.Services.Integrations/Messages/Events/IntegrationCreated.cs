using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class IntegrationCreated : IEvent
    {
        public string TenantId { get; }
        public string Name { get; }
        public Guid Id { get; }

        public IntegrationCreated(string tenantId, string name, Guid id)
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
        }
    }
}