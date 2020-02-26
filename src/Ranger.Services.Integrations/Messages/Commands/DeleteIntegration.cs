using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Integrations
{
    [MessageNamespaceAttribute("integrations")]
    public class DeleteIntegration : ICommand
    {
        public DeleteIntegration(string commandingUserEmail, string domain, string name, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new System.ArgumentException($"{nameof(domain)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace.");
            }

            this.CommandingUserEmail = commandingUserEmail;

            this.Domain = domain;
            this.Name = name;
            this.ProjectId = projectId;
        }

        public string CommandingUserEmail { get; }
        public string Domain { get; }
        public string Name { get; }
        public Guid ProjectId { get; }
    }
}