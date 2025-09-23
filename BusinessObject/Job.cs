using BusinessObject.Technician;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public Guid RepairOrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobName { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime? Deadline { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int Level { get; set; }

        // Navigation properties
        public virtual Service Service { get; set; }
        public virtual RepairOrder RepairOrder { get; set; }
        public virtual ICollection<JobPart> JobParts { get; set; }
        public virtual ICollection<JobTechnician> JobTechnicians { get; set; } = new List<JobTechnician>(); // Thêm quan hệ với JobTechnician
        public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();
    }
}