using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class ServiceInspection
    {
        [Key]
        public Guid ServiceInspectionId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public Guid InspectionId { get; set; }

        [MaxLength(100)]
        public string Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Service Service { get; set; }
        public virtual Inspection Inspection { get; set; }
    }
}