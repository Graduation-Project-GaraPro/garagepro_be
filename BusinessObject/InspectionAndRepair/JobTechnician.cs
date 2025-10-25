using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.InspectionAndRepair
{
    public class JobTechnician
    {
        [Key]
        public Guid JobTechnicianId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid TechnicianId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(JobId))]
        public virtual Job Job { get; set; }

        [ForeignKey(nameof(TechnicianId))]
        public virtual Technician Technician { get; set; }
    }
}
