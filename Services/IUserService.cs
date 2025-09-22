using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace Services
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> BanUserAsync(string userId, string message);
        Task<bool> UnbanUserAsync(string userId, string message);
    }
}
