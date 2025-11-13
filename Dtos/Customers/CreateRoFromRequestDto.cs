using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Customers
{
    public class CreateRoFromRequestDto
    {
        [Required]
        public DateTime ReceiveDate { get; set; } = DateTime.UtcNow;

        public DateTime? EstimatedCompletionDate { get; set; }

        [MaxLength(500)]
        public string Note { get; set; }

        // Services selected from the repair request
        public List<Guid> SelectedServiceIds { get; set; } = new List<Guid>();
    }
}