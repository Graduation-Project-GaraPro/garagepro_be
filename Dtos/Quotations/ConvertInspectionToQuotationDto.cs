using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Quotations
{
    public class ConvertInspectionToQuotationDto
    {
        [Required]
        public Guid InspectionId { get; set; }
        
        public string? Note { get; set; }
    }
}