using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class PartSpecification
    {
        [Key]
        public Guid SpecId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PartId { get; set; }

        [Required]
        public Guid SpecTypeId { get; set; }

        [Required]
        [MaxLength(500)]
        public string SpecValue { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Part Part { get; set; }
    }
}