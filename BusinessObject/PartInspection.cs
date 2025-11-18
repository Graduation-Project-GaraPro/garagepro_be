using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class PartInspection
    {
        [Key]
        public Guid PartInspectionId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PartId { get; set; }

        [Required]
        public Guid InspectionId { get; set; }
        [Required]
        public Guid PartCategoryId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Part Part { get; set; }
        public virtual Inspection Inspection { get; set; }
        public virtual PartCategory PartCategory { get; set; }
    }
}