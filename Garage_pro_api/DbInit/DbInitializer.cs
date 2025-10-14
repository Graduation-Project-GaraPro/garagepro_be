using BusinessObject.Authentication;
using BusinessObject.Roles;
using DataAccessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            // 1. Create default roles
            string[] roleNames = { "Admin", "Manager", "Customer", "Technician" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        Users = 0,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"Default {roleName} role",
                        IsDefault = roleName == "Customer",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _roleManager.CreateAsync(role);
                }
            }

            // 2. Seeding 4 accounts for each role
            var defaultUsers = new List<(string Phone, string FirstName, string LastName, string Role)>
            {
                ("0900000001", "System", "Admin", "Admin"),
                ("0900000002", "System", "Manager", "Manager"),
                ("0900000003", "Default", "Customer", "Customer"),
                ("0900000004", "Default", "Technician", "Technician")
            };

            string defaultPassword = _configuration["AdminUser:Password"] ?? "String@1";

            foreach (var (phone, first, last, role) in defaultUsers)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = phone,
                        PhoneNumber = phone,
                        PhoneNumberConfirmed = true,
                        FirstName = first,
                        LastName = last,
                        Email = $"{phone}@myapp.com",
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, defaultPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    else
                    {
                        throw new Exception($"Seeding user {phone} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // 3. Seed Permission Categories
            var categories = new List<PermissionCategory>
                {
                    new PermissionCategory { Id = Guid.NewGuid(), Name = "User Management" },
                    new PermissionCategory { Id = Guid.NewGuid(), Name = "Booking Management" },
                    new PermissionCategory { Id = Guid.NewGuid(), Name = "Role Management" }
                };

            foreach (var cat in categories)
            {
                if (!await _context.PermissionCategories.AnyAsync(c => c.Name == cat.Name))
                {
                    await _context.PermissionCategories.AddAsync(cat);
                }
            }
            await _context.SaveChangesAsync();

            // Lấy lại categories để đảm bảo có Id từ DB
            categories = await _context.PermissionCategories.ToListAsync();
            var userCatId = categories.First(c => c.Name == "User Management").Id;
            var bookingCatId = categories.First(c => c.Name == "Booking Management").Id;
            var RoleCatId = categories.First(c => c.Name == "Role Management").Id;

            // 4. Seed Permissions
            var defaultPermissions = new List<Permission>
            {
                new Permission { Id = Guid.NewGuid(), Code = "USER_VIEW", Name = "View Users", Description = "Can view user list", CategoryId = userCatId },
                new Permission { Id = Guid.NewGuid(), Code = "USER_EDIT", Name = "Edit Users", Description = "Can edit user info", CategoryId = userCatId },
                new Permission { Id = Guid.NewGuid(), Code = "USER_DELETE", Name = "Delete Users", Description = "Can delete users", CategoryId = userCatId },


                new Permission { Id = Guid.NewGuid(), Code = "ROLE_CREATE", Name = "Role create", Description = "Can create role", CategoryId = RoleCatId },
                new Permission { Id = Guid.NewGuid(), Code = "ROLE_UPDATE", Name = "Role update", Description = "Can update role", CategoryId = RoleCatId },
                new Permission { Id = Guid.NewGuid(), Code = "ROLE_DELETE", Name = "Role delete", Description = "Can delete role", CategoryId = RoleCatId },
                new Permission { Id = Guid.NewGuid(), Code = "ROLE_VIEW", Name = "Role View", Description = "Can View role", CategoryId = RoleCatId },
                new Permission { Id = Guid.NewGuid(), Code = "PERMISSION_ASIGN", Name = "permission asign", Description = "Can view role", CategoryId = RoleCatId },

                new Permission { Id = Guid.NewGuid(), Code = "BOOKING_VIEW", Name = "View Bookings", Description = "Can view bookings", CategoryId = bookingCatId },
                new Permission { Id = Guid.NewGuid(), Code = "BOOKING_MANAGE", Name = "Manage Bookings", Description = "Can manage bookings", CategoryId = bookingCatId }
            };

            foreach (var perm in defaultPermissions)
            {
                if (!await _context.Permissions.AnyAsync(p => p.Code == perm.Code))
                {
                    await _context.Permissions.AddAsync(perm);
                }
            }
            await _context.SaveChangesAsync();

            // 5. Assign default permissions to roles
            var roles = await _roleManager.Roles.ToListAsync();
            var permissions = await _context.Permissions.ToListAsync();

            var rolePermissionMap = new Dictionary<string, string[]>
            {
                { "Admin", new[] { "USER_VIEW", "USER_EDIT", "USER_DELETE", "BOOKING_VIEW", "BOOKING_MANAGE", "ROLE_VIEW", "ROLE_CREATE", "ROLE_UPDATE", "ROLE_DELETE", "PERMISSION_ASIGN" } },
                { "Manager", new[] { "USER_VIEW", "BOOKING_VIEW", "BOOKING_MANAGE" } },
                { "Customer", new[] { "BOOKING_VIEW" } },
                { "Technician", new[] { "BOOKING_MANAGE" } }
            };

            foreach (var role in roles)
            {
                if (!rolePermissionMap.ContainsKey(role.Name))
                    continue;

                var codes = rolePermissionMap[role.Name];

                foreach (var code in codes)
                {
                    var perm = permissions.FirstOrDefault(p => p.Code == code);
                    if (perm == null) continue;

                    bool exists = await _context.RolePermissions
                        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);

                    if (!exists)
                    {
                        var rp = new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = perm.Id,
                            GrantedBy = "SYSTEM",
                            GrantedAt = DateTime.UtcNow
                        };

                        await _context.RolePermissions.AddAsync(rp);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }


    }
}