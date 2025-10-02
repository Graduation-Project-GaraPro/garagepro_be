using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Service
    {
        [Key]
        public Guid ServiceId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceCategoryId { get; set; }

        [Required]
        public Guid ServiceTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ServiceStatus { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedDuration { get; set; } // in hours

        public bool IsActive { get; set; } = true;

        public bool IsAdvanced { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ServiceCategory ServiceCategory { get; set; }
        public virtual ICollection<RepairOrderService> RepairOrderServices { get; set; } = new List<RepairOrderService>();
        public virtual ICollection<ServiceInspection> ServiceInspections { get; set; } = new List<ServiceInspection>();
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}