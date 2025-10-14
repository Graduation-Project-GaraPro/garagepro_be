﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using DataAccessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyAppDbContext _context;

        public UserRepository(UserManager<ApplicationUser> userManager, MyAppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            // Lấy role Admin
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
                return await _context.Users.ToListAsync(); // nếu không có role Admin, trả về tất cả

            // Lấy userIds của Admin
            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            // Lấy tất cả user trừ Admin
            var users = await _context.Users
                .Where(u => !adminUserIds.Contains(u.Id))
                .ToListAsync();

            return users;
        }

        // Lấy tất cả user có role Manager và Technician
        public async Task<List<ApplicationUser>> GetManagersAndTechniciansAsync()
        {
            var roleNames = new[] { "Manager", "Technician" };

            // Lấy roleIds tương ứng
            var roles = await _context.Roles
                .Where(r => roleNames.Contains(r.Name))
                .ToListAsync();

            var roleIds = roles.Select(r => r.Id).ToList();

            // Lấy userIds có role Manager hoặc Technician
            var userIds = await _context.UserRoles
                .Where(ur => roleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        // Lấy tất cả user chỉ có role Technician
        public async Task<List<ApplicationUser>> GetTechniciansAsync()
        {
            var technicianRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Technician");

            if (technicianRole == null) return new List<ApplicationUser>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == technicianRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        // Lấy tất cả user chỉ có role Manager
        public async Task<List<ApplicationUser>> GetManagersAsync()
        {
            var managerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Manager");

            if (managerRole == null) return new List<ApplicationUser>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == managerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        // Lấy tất cả Manager chưa thuộc branch nào
        public async Task<List<ApplicationUser>> GetManagersWithoutBranchAsync()
        {
            var managerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Manager");

            if (managerRole == null) return new List<ApplicationUser>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == managerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id) && (u.BranchId == null))
                .ToListAsync();
        }

        // Lấy tất cả Technician chưa thuộc branch nào
        public async Task<List<ApplicationUser>> GetTechniciansWithoutBranchAsync()
        {
            var technicianRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Technician");

            if (technicianRole == null) return new List<ApplicationUser>();

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == technicianRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id) && (u.BranchId == null))
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        
        // Customer search methods
        public async Task<IEnumerable<ApplicationUser>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.Users.ToListAsync();
            }
            
            return await _context.Users
                .Where(u => u.FullName.Contains(searchTerm) || 
                            u.PhoneNumber.Contains(searchTerm) || 
                            u.Email.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetCustomersByVehicleLicensePlateAsync(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                return new List<ApplicationUser>();
            }
            
            return await _context.Users
                .Where(u => _context.Vehicles.Any(v => v.UserId == u.Id && v.LicensePlate.Contains(licensePlate)))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<ApplicationUser>> GetAllCustomersAsync()
        {
            // Get all users with "Customer" role
            var users = await _userManager.GetUsersInRoleAsync("Customer");
            return users;
        }
    }
}