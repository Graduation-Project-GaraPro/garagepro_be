using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using Dtos.Services;

namespace Services.ServiceServices
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceDto>> GetAllServicesAsync();

        Task<(IEnumerable<ServiceDto> Services, int TotalCount)> GetPagedServicesAsync(
                int pageNumber, int pageSize, string? searchTerm, bool? status, Guid? serviceTypeId);

        Task<ServiceDto> GetServiceByIdAsync(Guid id);
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto);
        Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto);
        Task<bool> DeleteServiceAsync(Guid id);
    }

}
