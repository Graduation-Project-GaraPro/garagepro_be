using Repositories.BranchRepositories;
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
    public interface IUnitOfWork
    {
        IRepairRequestRepository RepairRequests { get; }
        IRequestServiceRepository RequestServices { get; }
        IRequestPartRepository RequestParts { get; }
        IUserRepository Users { get; }
        IBranchRepository Branches{ get; }
        IRepairOrderRepository RepairOrders { get; }
        IOperatingHourRepository OperatingHours {  get; }
        IVehicleRepository Vehicles { get; }
        IServiceRepository Services { get; }
        IPartRepository Parts { get; }
        IRepairImageRepository RepairImages { get; }


        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();
    }
}
