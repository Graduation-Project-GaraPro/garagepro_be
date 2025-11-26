﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Dtos.Customers;

namespace Services
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<ApplicationUser> GetByIdAsync(string userId);
        Task<bool> BanUserAsync(string userId, string message);
        Task<bool> UnbanUserAsync(string userId, string message);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(ApplicationUser user);

        Task<bool> UpdateDeviceIdAsync(string userId, string deviceId);


        // 👇 Thêm 3 method mới
        Task<List<ApplicationUser>> GetManagersAndTechniciansAsync();
        Task<List<ApplicationUser>> GetManagersAsync();
        Task<List<ApplicationUser>> GetTechniciansAsync();
        Task<List<ApplicationUser>> GetManagersWithoutBranchAsync();
        Task<List<ApplicationUser>> GetTechniciansWithoutBranchAsync();

        Task<(List<object> Data, int Total)> GetUsersFiltered(UserFilterDto filters);
        Task<object> CreateUserAsync(CreateUserDto dto);
        
        // New method to get technicians by branch
        Task<List<ApplicationUser>> GetTechniciansByBranchAsync(Guid branchId);
       
    }
}