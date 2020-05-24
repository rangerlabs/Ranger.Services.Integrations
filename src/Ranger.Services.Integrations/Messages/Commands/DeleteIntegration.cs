using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class DeleteIntegration : ICommand
    {
        public DeleteIntegration(string commandingUserEmail, string tenantId, string name, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new System.ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace");
            }

            this.CommandingUserEmail = commandingUserEmail;

            this.TenantId = tenantId;
            this.Name = name;
            this.ProjectId = projectId;
        }

        public string CommandingUserEmail { get; }
        public string TenantId { get; }
        public string Name { get; }
        public Guid ProjectId { get; }
    }
}