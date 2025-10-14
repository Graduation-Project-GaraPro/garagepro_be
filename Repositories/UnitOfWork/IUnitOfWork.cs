using Repositories.Customers;
using Repositories.PartRepositories;
using Repositories.RepairRequestRepositories;
using Repositories.ServiceRepositories;
using Repositories.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepairRequestRepository RepairRequests { get; }
        IRequestServiceRepository RequestServices { get; }
        IRequestPartRepository RequestParts { get; }
        IUserRepository Users { get; }
        IVehicleRepository Vehicles { get; }
        IServiceRepository Services { get; }
        IPartRepository Parts { get; }
        IRepairImageRepository RepairImages { get; }
        Task<int> SaveChangesAsync();
    }
}
