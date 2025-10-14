using BusinessObject.Enums;
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
        [ForeignKey(nameof(Service))]
        public Guid ServiceId { get; set; }

        [Required]
        [ForeignKey(nameof(Inspection))]
        public Guid InspectionId { get; set; }
        public ConditionStatus ConditionStatus { get; set; } = ConditionStatus.Not_Checked;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Service Service { get; set; }
        public virtual Inspection Inspection { get; set; }
    }
}