using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class UpdateIntegration : ICommand
    {
        public UpdateIntegration(string domain, string commandingUserEmail, Guid projectId, string messageJsonContent, IntegrationsEnum integrationType, int version)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrEmpty(domain))
            {
                throw new ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrEmpty(messageJsonContent))
            {
                throw new ArgumentException($"{nameof(messageJsonContent)} was null or whitespace.");
            }
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException("Version must be a positive integer.");
            }

            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
            this.ProjectId = projectId;
            this.MessageJsonContent = messageJsonContent;
            this.IntegrationType = integrationType;
            this.Version = version;
        }

        public string Domain { get; }
        public string CommandingUserEmail { get; }
        public Guid ProjectId { get; }
        public string MessageJsonContent { get; }
        public IntegrationsEnum IntegrationType { get; }
        public int Version { get; }
    }
}