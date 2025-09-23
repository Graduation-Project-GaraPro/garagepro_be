using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Technician
{
    public class Repair
    {
        [Key]
        public Guid RepairId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public RepairStatus Status { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public long? ActualTime { get; set; }
        public long? EstimatedTime { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(JobId))]
        public virtual Job Job { get; set; }
    }
}
