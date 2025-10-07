using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Quotation
{
    public class QuotationServicePartDto
    {
        public Guid QuotationServicePartId { get; set; }
        public Guid QuotationServiceId { get; set; }
        public Guid PartId { get; set; }
        public string? PartName { get; set; } // Make nullable
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}