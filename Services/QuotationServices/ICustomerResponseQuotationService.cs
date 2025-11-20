using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Quotations;

namespace Services.QuotationServices
{
    public interface ICustomerResponseQuotationService
    {
        Task<QuotationDto> ProcessCustomerResponseAsync(CustomerQuotationResponseDto responseDto, string userId);
    }
}
