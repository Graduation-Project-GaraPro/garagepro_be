using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Branches
{
    public class OperatingHour
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key tới Branch
        [Required]
        public Guid BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        // Enum cho ngày trong tuần
        [Required]
        public DayOfWeekEnum DayOfWeek { get; set; }

        public bool IsOpen { get; set; } = false;

        [MaxLength(5)]
        public string OpenTime { get; set; } = "08:00";

        [MaxLength(5)]
        public string CloseTime { get; set; } = "17:00";
    }
}
