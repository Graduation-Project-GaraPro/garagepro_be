using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Enums;
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
            await _context.Database.EnsureCreatedAsync();

            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedPermissionCategoriesAsync();
            await SeedPermissionsAsync();
            await AssignPermissionsToRolesAsync();


            await SeedPartCategoriesAsync();
            await SeedPartsAsync();
            await SeedServiceCategoriesAsync();
            await SeedServicesAsync();
            await SeedServicePartsAsync();
            await SeedBranchesAsync();

        }

        // 1. Seed Roles
        private async Task SeedRolesAsync()
        {
            string[] roleNames = { "Admin", "Manager", "Customer", "Technician" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
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
        }

        // 2. Seed Users
        private async Task SeedUsersAsync()
        {
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
                        await _userManager.AddToRoleAsync(user, role);
                    else
                        throw new Exception($"Seeding user {phone} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // 3. Seed Permission Categories
        private async Task SeedPermissionCategoriesAsync()
        {
            var categories = new List<PermissionCategory>
    {
        new PermissionCategory { Id = Guid.NewGuid(), Name = "User Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Booking Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Role Management" }
    };

            foreach (var cat in categories)
            {
                if (!await _context.PermissionCategories.AnyAsync(c => c.Name == cat.Name))
                    await _context.PermissionCategories.AddAsync(cat);
            }

            await _context.SaveChangesAsync();
        }

        // 4. Seed Permissions
        private async Task SeedPermissionsAsync()
        {
            var categories = await _context.PermissionCategories.ToListAsync();
            var userCatId = categories.First(c => c.Name == "User Management").Id;
            var bookingCatId = categories.First(c => c.Name == "Booking Management").Id;
            var roleCatId = categories.First(c => c.Name == "Role Management").Id;

            var defaultPermissions = new List<Permission>
    {
        new Permission { Id = Guid.NewGuid(), Code = "USER_VIEW", Name = "View Users", Description = "Can view user list", CategoryId = userCatId },
        new Permission { Id = Guid.NewGuid(), Code = "USER_EDIT", Name = "Edit Users", Description = "Can edit user info", CategoryId = userCatId },
        new Permission { Id = Guid.NewGuid(), Code = "USER_DELETE", Name = "Delete Users", Description = "Can delete users", CategoryId = userCatId },

        new Permission { Id = Guid.NewGuid(), Code = "ROLE_CREATE", Name = "Role create", Description = "Can create role", CategoryId = roleCatId },
        new Permission { Id = Guid.NewGuid(), Code = "ROLE_UPDATE", Name = "Role update", Description = "Can update role", CategoryId = roleCatId },
        new Permission { Id = Guid.NewGuid(), Code = "ROLE_DELETE", Name = "Role delete", Description = "Can delete role", CategoryId = roleCatId },
        new Permission { Id = Guid.NewGuid(), Code = "ROLE_VIEW", Name = "Role View", Description = "Can View role", CategoryId = roleCatId },
        new Permission { Id = Guid.NewGuid(), Code = "PERMISSION_ASIGN", Name = "Permission assign", Description = "Can assign permission", CategoryId = roleCatId },

        new Permission { Id = Guid.NewGuid(), Code = "BOOKING_VIEW", Name = "View Bookings", Description = "Can view bookings", CategoryId = bookingCatId },
        new Permission { Id = Guid.NewGuid(), Code = "BOOKING_MANAGE", Name = "Manage Bookings", Description = "Can manage bookings", CategoryId = bookingCatId }
    };

            foreach (var perm in defaultPermissions)
            {
                if (!await _context.Permissions.AnyAsync(p => p.Code == perm.Code))
                    await _context.Permissions.AddAsync(perm);
            }

            await _context.SaveChangesAsync();
        }

        // 5. Assign permissions to roles
        private async Task AssignPermissionsToRolesAsync()
        {
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
                if (!rolePermissionMap.ContainsKey(role.Name)) continue;

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

        private async Task SeedPartCategoriesAsync()
        {
            if (!_context.PartCategories.Any())
            {
                var categories = new List<PartCategory>
            {
                new PartCategory { CategoryName = "Engine" },
                new PartCategory { CategoryName = "Brakes" },
                new PartCategory { CategoryName = "Electrical" }
            };
                _context.PartCategories.AddRange(categories);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPartsAsync()
        {
            if (!_context.Parts.Any())
            {
                var engineCategory = await _context.PartCategories.FirstAsync(c => c.CategoryName == "Engine");
                var brakeCategory = await _context.PartCategories.FirstAsync(c => c.CategoryName == "Brakes");

                var parts = new List<Part>
        {
            new Part { Name = "Air Filter", PartCategoryId = engineCategory.LaborCategoryId, Price = 150000, Stock = 50, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Brake Pad", PartCategoryId = brakeCategory.LaborCategoryId, Price = 400000, Stock = 30, CreatedAt = DateTime.UtcNow }
        };

                _context.Parts.AddRange(parts);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedServiceCategoriesAsync()
        {
            if (!_context.ServiceCategories.Any())
            {
                var categories = new List<ServiceCategory>
            {
                new ServiceCategory { CategoryName = "Maintenance",Description="This mantainance car" },
                new ServiceCategory { CategoryName = "Repair" ,Description="This mantainance car"}
            };
                _context.ServiceCategories.AddRange(categories);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedServicesAsync()
        {
            if (!_context.Services.Any())
            {
                var maintenanceCategory = await _context.ServiceCategories.FirstAsync(c => c.CategoryName == "Maintenance");
                var repairCategory = await _context.ServiceCategories.FirstAsync(c => c.CategoryName == "Repair");

                // Nếu chưa có ServiceType, có thể tạo tạm
                var defaultServiceTypeId = Guid.NewGuid();

                var services = new List<Service>
        {
            new Service
            {
                ServiceName = "Oil Change",
                Description = "This is Oil Change",
                ServiceCategoryId = maintenanceCategory.ServiceCategoryId,
                ServiceTypeId = defaultServiceTypeId,
                ServiceStatus = "Active",
                Price = 300000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Brake Repair",
                Description = "This is Brake Repair",
                ServiceCategoryId = repairCategory.ServiceCategoryId,
                ServiceTypeId = defaultServiceTypeId,
                ServiceStatus = "Active",
                Price = 1200000,
                EstimatedDuration = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Services.AddRange(services);
                await _context.SaveChangesAsync();
            }
        }
        private async Task SeedServicePartsAsync()
        {
            if (!_context.ServiceParts.Any())
            {
                var oilChange = await _context.Services.FirstAsync(s => s.ServiceName == "Oil Change");
                var brakeRepair = await _context.Services.FirstAsync(s => s.ServiceName == "Brake Repair");

                var airFilter = await _context.Parts.FirstAsync(p => p.Name == "Air Filter");
                var brakePad = await _context.Parts.FirstAsync(p => p.Name == "Brake Pad");

                _context.ServiceParts.Add(new ServicePart
                {
                    ServiceId = oilChange.ServiceId,
                    PartId = airFilter.PartId,
                    Quantity = 1,
                    UnitPrice = airFilter.Price,
                    CreatedAt = DateTime.UtcNow
                });

                _context.ServiceParts.Add(new ServicePart
                {
                    ServiceId = brakeRepair.ServiceId,
                    PartId = brakePad.PartId,
                    Quantity = 2,
                    UnitPrice = brakePad.Price,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
        }
        private async Task SeedBranchesAsync()
        {
            if (!_context.Branches.Any())
            {
                // Tạo Branch
                var branch1 = new Branch
                {
                    BranchName = "Central Branch",
                    Description = "this is central Branch ",
                    Street = "123 Main Street",
                    Ward = "Ward 1",
                    District = "District 1",
                    City = "HCMC",
                    PhoneNumber = "0123456789",
                    Email = "central@garage.com",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Seed OperatingHours (7 ngày)
                foreach (DayOfWeekEnum day in Enum.GetValues(typeof(DayOfWeekEnum)))
                {
                    branch1.OperatingHours.Add(new OperatingHour
                    {
                        DayOfWeek = day,
                        IsOpen = true,
                        OpenTime = "08:00",
                        CloseTime = "17:00"
                    });
                }

                // Tìm các user đã có sẵn
                var managerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000002");
                var technicianUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000004");

                if (managerUser != null) branch1.Staffs.Add(managerUser);
                if (technicianUser != null) branch1.Staffs.Add(technicianUser);

                // Seed BranchService (nhiều-nhiều)
                var services = await _context.Services.Take(5).ToListAsync(); // lấy vài service để demo
                foreach (var service in services)
                {
                    branch1.BranchServices.Add(new BranchService
                    {
                        Branch = branch1,
                        Service = service
                    });
                }

                _context.Branches.Add(branch1);
                await _context.SaveChangesAsync();
            }
        }


    }
}
