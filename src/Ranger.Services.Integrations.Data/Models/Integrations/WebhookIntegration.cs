using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.Services.Integrations.Data
{
    public class WebhookIntegration : IIntegration
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
        public string HeadersJson { get; set; }
        public string MetadataJson { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Deleted { get; set; } = false;
    }
}