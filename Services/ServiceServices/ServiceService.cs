using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using Repositories.ServiceRepositories;

namespace Services.ServiceServices
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;

        public ServiceService(IServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            // Có thể xử lý thêm logic business ở đây
            return await _repository.GetAllAsync();
        }
    }
}
