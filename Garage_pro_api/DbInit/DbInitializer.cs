using BusinessObject.Authentication;
using BusinessObject.Roles;
using DataAccessLayer;
using Microsoft.AspNetCore.Identity;

namespace Garage_pro_api.DbInit
{
    public class DbInitializer
    {
        private readonly MyAppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;

        public DbInitializer(
            MyAppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task Initialize()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Create default roles
            string[] roleNames = { "Admin", "Manager", "Customer", "Technician" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        Users=0,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"Default {roleName} role",
                        IsDefault = true, // ví dụ: Customer là mặc định
                        CreatedAt = DateTime.UtcNow
                    };

                    await _roleManager.CreateAsync(role);
                }
            }

            // Create admin user
            var adminEmail = _configuration["AdminUser:Email"];
            var adminPassword = _configuration["AdminUser:Password"];

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                //var createPowerUser = await _userManager.CreateAsync(adminUser, adminPassword);
                //if (createPowerUser.Succeeded)
                //{
                //    await _userManager.AddToRoleAsync(adminUser, "Administrator");

                //    // Add admin permissions
                //    await _userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("Permission", "EditProducts"));
                //    await _userManager.AddClaimAsync(adminUser, new System.Security.Claims.Claim("Permission", "DeleteUsers"));
                //}
            }
        }
    }
}
