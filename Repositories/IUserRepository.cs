﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
namespace Repositories
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetByIdAsync(string userId);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<bool> UpdateDeviceIdAsync(string userId, string deviceId);
        Task<List<ApplicationUser>> GetManagersAndTechniciansAsync();
        Task<List<ApplicationUser>> GetTechniciansAsync();
        Task<List<ApplicationUser>> GetManagersAsync();

        Task<List<ApplicationUser>> GetManagersWithoutBranchAsync();
        Task<List<ApplicationUser>> GetTechniciansWithoutBranchAsync();
        Task UpdateAsync(ApplicationUser user);
        
        // Customer search methods
        Task<IEnumerable<ApplicationUser>> SearchCustomersAsync(string searchTerm);
        Task<IEnumerable<ApplicationUser>> GetCustomersByVehicleLicensePlateAsync(string licensePlate);
        Task<IEnumerable<ApplicationUser>> GetAllCustomersAsync();
    }
}