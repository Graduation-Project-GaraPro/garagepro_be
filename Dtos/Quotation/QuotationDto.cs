using BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Quotation
{
    public class QuotationDto
    {
        public Guid QuotationId { get; set; }
        public Guid InspectionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Manager name
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ResponseAt { get; set; }
        public QuotationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerNote { get; set; }
        public string ChangeRequestDetails { get; set; }
        public DateTime? EstimateExpiresAt { get; set; }
        public int RevisionNumber { get; set; }
        public Guid? OriginalQuotationId { get; set; }
        public List<QuotationServiceDto> QuotationServices { get; set; } = new List<QuotationServiceDto>();
    }
}