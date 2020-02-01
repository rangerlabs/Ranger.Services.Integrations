using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ranger.Services.Integrations.Data
{
    public class IntegrationUniqueConstraint
    {
        //Foreign Key references are not supported for JsonB columns
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid IntegrationId { get; set; }

        [Required]
        public string DatabaseUsername { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(140)]
        public string Name { get; set; }
    }
}