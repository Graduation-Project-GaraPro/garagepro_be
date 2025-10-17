using DataAccessLayer;
using Repositories.Customers;
using Repositories.PartRepositories;
using Repositories.RepairRequestRepositories;
using Repositories.ServiceRepositories;
using Repositories.VehicleRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.UnitOfWork
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly MyAppDbContext _context;

        public IRepairRequestRepository RepairRequests { get; }
        public IRequestServiceRepository RequestServices { get; }
        public IRequestPartRepository RequestParts { get; }
        public IUserRepository Users { get; }
        public IVehicleRepository Vehicles { get; }
        public IServiceRepository Services { get; }
        public IPartRepository Parts { get; }
        public IRepairImageRepository RepairImages { get; }
      
        public UnitOfWork(
            MyAppDbContext context,
            IRepairRequestRepository repairRequestRepository,
            IRequestServiceRepository requestServiceRepository,
            IRequestPartRepository requestPartRepository,
            IUserRepository userRepository,
            IVehicleRepository vehicleRepository,
            IServiceRepository serviceRepository,
            IPartRepository partRepository,
            IRepairImageRepository repairImages)
        {
            _context = context;
            RepairRequests = repairRequestRepository;
            RequestServices = requestServiceRepository;
            RequestParts = requestPartRepository;
            Users = userRepository;
            Vehicles = vehicleRepository;
            Services = serviceRepository;
            Parts = partRepository;
            RepairImages = repairImages;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

