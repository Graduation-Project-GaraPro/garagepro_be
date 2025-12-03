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

        /// <summary>
        /// Optional list of service IDs to add to this inspection.
        /// System will validate that these services don't already exist in RO or previous inspections.
        /// </summary>
        public List<Guid> ServiceIds { get; set; } = new List<Guid>();
    }
}