using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Quotation
{
    public class QuotationServiceDto
    {
        public Guid QuotationServiceId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } // For display purposes
        public decimal ServicePrice { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerRequestedParts { get; set; } // For display only
        public List<QuotationServicePartDto> QuotationServiceParts { get; set; } = new List<QuotationServicePartDto>();
    }
}