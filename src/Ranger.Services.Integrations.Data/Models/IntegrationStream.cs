using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ranger.Common;
using Ranger.Common.SharedKernel;

namespace Ranger.Services.Integrations.Data
{
    public class IntegrationStream : IEventStreamDbSet
    {
        [Required]
        public string DatabaseUsername { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public Guid StreamId { get; set; }
        [Required]
        public int Version { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        public string Data { get; set; }
        [Required]
        public string Event { get; set; }
        [Required]
        public DateTime InsertedAt { get; set; }
        [Required]
        public string InsertedBy { get; set; }
        [Required]
        public IntegrationsEnum IntegrationType { get; set; }
    }
}