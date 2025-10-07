using BusinessObject.Technician;
using Repositories.Technician;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Technician
{
    public class SpecificationService: ISpecificationService
    {
        private readonly ISpecificationRepository _repository;

        public SpecificationService(ISpecificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VehicleLookup>> GetAllSpecificationsAsync()
        {
            return await _repository.GetAllSpecificationsAsync();
        }

        public async Task<List<VehicleLookup>> SearchSpecificationsAsync(string keyword)
        {
            return await _repository.SearchSpecificationsAsync(keyword);
        }
    }
}
