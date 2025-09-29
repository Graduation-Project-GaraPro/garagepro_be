using BusinessObject.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Technician
{
    public class Technician
    {
        [Key]
        public Guid TechnicianId { get; set; } = Guid.NewGuid();

        // Foreign Keys
        [Required]
        public string UserId { get; set; }     

        [Required]
        public Guid JobId { get; set; }

        [Range(0, 10)]
        public float Quality { get; set; }

        [Range(0, 10)]
        public float Speed { get; set; }

        [Range(0, 10)]
        public float Efficiency { get; set; }

        [Range(0, 10)]
        public float Score { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }                

        public virtual ICollection<JobTechnician> JobTechnicians { get; set; } = new List<JobTechnician>();
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>(); // Thêm quan hệ với Inspection
    }
}
