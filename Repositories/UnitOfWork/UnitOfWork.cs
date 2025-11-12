using DataAccessLayer;
using Microsoft.EntityFrameworkCore.Storage;
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
    public class UnitOfWork: IUnitOfWork
    {
        private readonly MyAppDbContext _context;
        private IDbContextTransaction _transaction;
        public IRepairRequestRepository RepairRequests { get; }
        public IRequestServiceRepository RequestServices { get; }
        public IRequestPartRepository RequestParts { get; }
        public IUserRepository Users { get; }
        public IVehicleRepository Vehicles { get; }
        public IServiceRepository Services { get; }
        public IPartRepository Parts { get; }
        public IRepairImageRepository RepairImages { get; }

        public IBranchRepository Branches { get; }

        public IOperatingHourRepository OperatingHours { get; }

        public IRepairOrderRepository RepairOrders {  get; }

        public UnitOfWork(
            MyAppDbContext context,
            IRepairRequestRepository repairRequestRepository,
            IRequestServiceRepository requestServiceRepository,
            IRequestPartRepository requestPartRepository,
            IUserRepository userRepository,
            IVehicleRepository vehicleRepository,
            IServiceRepository serviceRepository,
            IPartRepository partRepository,
            IBranchRepository branches,
            IOperatingHourRepository operatingHours,
            IRepairOrderRepository repairOrder,
            IRepairImageRepository repairImages)
            
        {
            _context = context;
            RepairRequests = repairRequestRepository;
            RequestServices = requestServiceRepository;
            RequestParts = requestPartRepository;
            RepairOrders = repairOrder;
            Branches = branches;
            OperatingHours = operatingHours;
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
        public async Task BeginTransactionAsync()
        {
            // Kiểm tra nếu đã có transaction thì không tạo mới
            if (_transaction != null)
            {
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitAsync()
        {
            try
            {
                // Lưu changes trước
                await _context.SaveChangesAsync();

                // Commit transaction
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                // Nếu có lỗi thì rollback
                await RollbackAsync();
                throw;
            }
            finally
            {
                // Dispose transaction
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            await _context.DisposeAsync();
        }
    }
}

