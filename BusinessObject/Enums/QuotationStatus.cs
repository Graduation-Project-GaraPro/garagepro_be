using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum QuotationStatus
    {
        Draft,           // Initial state when quotation is created
        Sent,            // Sent to customer for approval
        Approved,        // Customer approved the quotation
        Rejected,        // Customer rejected the quotation
        Revised,         // Manager revised the quotation after rejection
        Expired,         // Quotation expired
        Cancelled        // Quotation was cancelled
    }
}