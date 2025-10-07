using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.EmailSenders;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IEmailSender _emailSender;

        public UserService(IUserRepository repository, IEmailSender emailSender)
        {
            _repository = repository;
            _emailSender = emailSender;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _repository.GetRolesAsync(user);
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
    }
}
