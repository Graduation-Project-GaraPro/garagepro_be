using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dtos.RoBoard;

namespace Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<RoBoardCustomerDto>> SearchCustomersAsync(string searchTerm);
        Task<IEnumerable<RoBoardCustomerDto>> GetAllCustomersAsync();
        Task<RoBoardCustomerDto> GetCustomerByIdAsync(string customerId);
        Task<RoBoardCustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<RoBoardCustomerDto> QuickCreateCustomerAsync(QuickCreateCustomerDto quickCreateCustomerDto);
    }
}