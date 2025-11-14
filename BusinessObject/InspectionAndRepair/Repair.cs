using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.InspectionAndRepair
{
    public class Repair
    {
        [Key]
        public Guid RepairId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        // Lưu vào database
        [Column(TypeName = "bigint")]
        public long? ActualTimeTicks { get; set; }

        [Column(TypeName = "bigint")]
        public long? EstimatedTimeTicks { get; set; }

        // Computed properties - thêm JsonIgnore để tránh EF serialize sai
        [NotMapped]
        [JsonIgnore] // Bỏ qua khi EF serialize
        public TimeSpan? ActualTime
        {
            get => ActualTimeTicks.HasValue ? TimeSpan.FromTicks(ActualTimeTicks.Value) : null;
            set => ActualTimeTicks = value?.Ticks;
        }

        [NotMapped]
        [JsonIgnore] // Bỏ qua khi EF serialize
        public TimeSpan? EstimatedTime
        {
            get => EstimatedTimeTicks.HasValue ? TimeSpan.FromTicks(EstimatedTimeTicks.Value) : null;
            set => EstimatedTimeTicks = value?.Ticks;
        }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(JobId))]
        public virtual Job Job { get; set; }
    }
}