using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Technician
{
    public class JobTechnician
    {
        [Key]
        public Guid JobTechnicianId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobId { get; set; }

        [Required]
        public Guid TechnicianId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(JobId))]
        public virtual Job Job { get; set; }

        [ForeignKey(nameof(TechnicianId))]
        public virtual Technician Technician { get; set; }
    }
}
