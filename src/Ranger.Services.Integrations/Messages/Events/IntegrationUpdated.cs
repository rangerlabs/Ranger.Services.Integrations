using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class IntegrationUpdated : IEvent
    {
        public string Domain { get; }
        public string Name { get; }
        public Guid Id { get; }

        public IntegrationUpdated(string domain, string name, Guid id)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace.");
            }

            this.Domain = domain;
            this.Name = name;
            this.Id = id;
        }
    }
}