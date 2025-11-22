using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.InspectionAndRepair
{
    public class UpdateInspectionRequest
    {
        public string? Finding { get; set; }
        public List<ServiceUpdateDto> ServiceUpdates { get; set; } = new();
        public bool IsCompleted { get; set; }
    }

    public class ServiceUpdateDto
    {
        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        public ConditionStatus ConditionStatus { get; set; }

        public List<Guid>? SelectedPartCategoryIds { get; set; } = new();

        public Dictionary<Guid, List<PartWithQuantityDto>>? SuggestedPartsByCategory { get; set; } = new();
    }
    public class PartWithQuantityDto
    {
        [Required]
        public Guid PartId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 1;
    }

}
