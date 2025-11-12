using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Dtos.Quotations
{
    public class CreateInspectionDto
    {
        [Required]
        public Guid RepairOrderId { get; set; }

        [Required]
        [MaxLength(500)]
        public string CustomerConcern { get; set; }
    }

    public class UpdateInspectionDto
    {
        public Guid? TechnicianId { get; set; }

        [MaxLength(500)]
        public string CustomerConcern { get; set; }

        [MaxLength(500)]
        public string Finding { get; set; }

        public IssueRating IssueRating { get; set; } = IssueRating.Fair;

        public string? Note { get; set; }
        
        public string? ImageUrl { get; set; }
    }

    public class InspectionConcernDto
    {
        public Guid ConcernId { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }
    }

    public class UpdateInspectionFindingDto
    {
        public string Finding { get; set; }
        public string? Note { get; set; }
        public IssueRating IssueRating { get; set; }
    }

    public class UpdateCustomerConcernDto
    {
        public string CustomerConcern { get; set; }
    }
    
    public class UpdateInspectionImageDto
    {
        public string ImageUrl { get; set; }
    }
}