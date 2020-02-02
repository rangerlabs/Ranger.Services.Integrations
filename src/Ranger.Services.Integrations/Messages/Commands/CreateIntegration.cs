using System;
using Ranger.Common.SharedKernel;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class CreateIntegration : ICommand
    {
        public CreateIntegration(string domain, string commandingUserEmail, Guid projectId, string messageJsonContent, IntegrationsEnum integrationType)
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

            this.Domain = domain;
            this.CommandingUserEmail = commandingUserEmail;
            this.ProjectId = projectId;
            this.MessageJsonContent = messageJsonContent;
            this.IntegrationType = integrationType;
        }
        public string Domain { get; }
        public string CommandingUserEmail { get; }
        public Guid ProjectId { get; }
        public string MessageJsonContent { get; }
        public IntegrationsEnum IntegrationType { get; }
    }
}