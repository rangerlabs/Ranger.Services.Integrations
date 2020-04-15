using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class UpdateIntegration : ICommand
    {
        public UpdateIntegration(string tenantId, string commandingUserEmail, Guid projectId, string messageJsonContent, IntegrationsEnum integrationType, int version)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }
            if (string.IsNullOrEmpty(messageJsonContent))
            {
                throw new ArgumentException($"{nameof(messageJsonContent)} was null or whitespace.");
            }
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException("Version must be a positive integer.");
            }

            this.TenantId = tenantId;
            this.CommandingUserEmail = commandingUserEmail;
            this.ProjectId = projectId;
            this.MessageJsonContent = messageJsonContent;
            this.IntegrationType = integrationType;
            this.Version = version;
        }

        public string TenantId { get; }
        public string CommandingUserEmail { get; }
        public Guid ProjectId { get; }
        public string MessageJsonContent { get; }
        public IntegrationsEnum IntegrationType { get; }
        public int Version { get; }
    }
}