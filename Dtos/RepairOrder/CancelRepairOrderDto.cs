using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.RepairOrder
{
    public class CancelRepairOrderDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }
        
        public string? CancelReason { get; set; }
    }
}