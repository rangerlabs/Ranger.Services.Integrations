using System;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data.EntityModels
{
    public class EntityWebhookIntegration : IEntityIntegration
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public string SigningKey { get; set; }
        [Required]
        public EnvironmentEnum Environment { get; set; }
        public string Headers { get; set; }
        public string Metadata { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Deleted { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedOn { get; set; }
    }
}