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
            return await _context.Users.ToListAsync();
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