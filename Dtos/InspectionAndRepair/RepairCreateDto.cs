using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dtos.InspectionAndRepair
{
    public class RepairCreateDto
    {
        [Required(ErrorMessage = "JobId là bắt buộc")]
        public Guid JobId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        [Required(ErrorMessage = "EstimatedTime là bắt buộc")]
        [RegularExpression(@"^\d{1,4}:\d{2}$",
           ErrorMessage = "Format phải là 'HH:mm' (ví dụ: 25:30, 02:15, 00:45)")]
        public string EstimatedTime { get; set; }
    }
}