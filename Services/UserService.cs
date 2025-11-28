using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Dtos.Customers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.EmailSenders;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IUserRepository repository, IEmailSender emailSender, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<bool> UpdateDeviceIdAsync(string userId, string deviceId)
        {
            return await _repository.UpdateDeviceIdAsync(userId, deviceId);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _repository.GetRolesAsync(user);
        }

        public async Task<ApplicationUser> GetByIdAsync(string userId)
        {
            return await _repository.GetByIdAsync(userId);
        }

        public async Task<bool> BanUserAsync(string userId, string message)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            await _repository.UpdateAsync(user);

            // 📧 Gửi mail thông báo
            await _emailSender.SendEmailAsync(
                user.Email,
                "Your account has been banned",
                $@"
                    <p>Hello {user.FirstName},</p>
                    <p>Your account has been <strong>banned</strong> by admin.</p>
                    <p><strong>Reason:</strong> {message}</p>
                    <p>Please contact support if you think this is a mistake.</p>
                    "
            );

            return true;
        }

        public async Task<bool> UnbanUserAsync(string userId, string message)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            await _repository.UpdateAsync(user);

            // 📧 Gửi mail thông báo
            await _emailSender.SendEmailAsync(
                user.Email,
                "Your account has been unbanned",
                $@"
                        <p>Hello {user.FirstName},</p>
                        <p>Your account has been <strong>re-activated</strong>.</p>
                        <p><strong>Message from admin:</strong> {message}</p>
                        <p>You can now log in again.</p>
                        "
            );

            return true;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _repository.GetByIdAsync(userId);
        }
        // 👇 Thêm các method mới
        public async Task<List<ApplicationUser>> GetManagersAndTechniciansAsync()
        {
            return await _repository.GetManagersAndTechniciansAsync();
        }

        public async Task<List<ApplicationUser>> GetManagersAsync()
        {
            return await _repository.GetManagersAsync();
        }

        public async Task<List<ApplicationUser>> GetTechniciansAsync()
        {
            return await _repository.GetTechniciansAsync();
        }

        public async Task<List<ApplicationUser>> GetManagersWithoutBranchAsync()
        {
            return await _repository.GetManagersWithoutBranchAsync();
        }

        public async Task<List<ApplicationUser>> GetTechniciansWithoutBranchAsync()
        {
            return await _repository.GetTechniciansWithoutBranchAsync();

        }

        // New method to get technicians by branch
        public async Task<List<ApplicationUser>> GetTechniciansByBranchAsync(Guid branchId)
        {
            return await _repository.GetTechniciansByBranchAsync(branchId);
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                var existingUser = await _repository.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    return false; // User không tồn tại
                }
                await _repository.UpdateAsync(user);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false; // Xử lý lỗi concurrency nếu cần
            }
        }
        public async Task<(List<object> Data, int Total)> GetUsersFiltered(UserFilterDto filters)
        {
            // 1) Lấy danh sách phù hợp Search + Status (chưa phân trang)
            var query = _repository.QueryUsers();

            if (!string.IsNullOrEmpty(filters.Search))
            {
                var q = filters.Search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(q) ||
                    u.LastName.ToLower().Contains(q) ||
                    u.Email.ToLower().Contains(q));
            }

            if (!string.IsNullOrEmpty(filters.Status))
            {
                var banned = filters.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase);
                query = query.Where(u => banned ? !u.IsActive : u.IsActive);
            }

            var list = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(); // ✅ lấy thô trước

            // 2) Lọc role trước khi phân trang
            if (!string.IsNullOrEmpty(filters.Role))
            {
                var filtered = new List<ApplicationUser>();

                foreach (var user in list)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Any(r => r.Equals(filters.Role, StringComparison.OrdinalIgnoreCase)))
                        filtered.Add(user);
                }

                list = filtered;
            }

            // 3) Tổng sau lọc role
            var total = list.Count;

            // 4) Phân trang chuẩn
            list = list
                .Skip((filters.Page - 1) * filters.Limit)
                .Take(filters.Limit)
                .ToList();

            // 5) Map ra object trả FE
            var result = new List<object>();

            foreach (var user in list)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new
                {
                    user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    user.Email,
                    user.PhoneNumber,
                    Verified = user.EmailConfirmed,
                    user.IsActive,
                    user.CreatedAt,
                    user.EmailConfirmed,
                    user.LastLogin,
                    Roles = roles
                });
            }

            return (result, total);
        }



        public async Task<object> CreateUserAsync(CreateUserDto dto)
        {
            if (await _repository.GetByEmailAsync(dto.Email) != null)
                throw new Exception("Email already exists.");
            

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                UserName = dto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _repository.CreateUserAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            await _repository.AddUserToRoleAsync(user, dto.Role);

            return new { user.Id, user.Email };
        }
    }
}
