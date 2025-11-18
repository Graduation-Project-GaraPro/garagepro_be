using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Dtos.Quotations
{
    public class CreateManagerInspectionDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }

        public Guid? TechnicianId { get; set; }

        [Required]
        [MaxLength(500)]
        public string CustomerConcern { get; set; }
    }
}