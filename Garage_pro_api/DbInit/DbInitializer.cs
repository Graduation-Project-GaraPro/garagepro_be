using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Campaigns;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using BusinessObject.Roles;
using BusinessObject.Vehicles;
using DataAccessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Customers;

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

            await SeedTechniciansAsync();
            await SeedPermissionCategoriesAsync();
            await SeedPermissionsAsync();
            await AssignPermissionsToRolesAsync();


            await SeedPartCategoriesAsync();
            await SeedPartsAsync();
            await SeedServiceCategoriesAsync();
            await SeedServicesAsync();
            await SeedServicePartsAsync();
            await SeedBranchesAsync();
            await SeedOrderStatusesAsync();
            await SeedLabelsAsync();
            await SeedVehicleRelatedEntitiesAsync();
            await SeedVehiclesAsync();
            await SeedRepairOrdersAsync();

            await SeedPromotionalCampaignsWithServicesAsync();

            // await SeedRepairOrdersAsync();
            // await SeedInspectionsAsync();
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
                        IsDefault = true,
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
                ("0900000003", "System", "Manager1", "Manager"),
                ("0900000004", "System", "Manager2", "Manager"),
                ("0900000005", "Default", "Customer", "Customer"),
                ("0900000006", "Default", "Technician", "Technician"),
                ("0900000007", "Default", "Technician1", "Technician"),
                ("0900000008", "Default", "Technician2", "Technician"),
                ("0987654321", "Manager", "User", "Manager")
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

                    // Use the specific password for the requested manager user
                    string password = phone == "0987654321" ? "Admin123!" : defaultPassword;
                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                        await _userManager.AddToRoleAsync(user, role);
                    else
                        throw new Exception($"Seeding user {phone} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private async Task SeedTechniciansAsync()
        {
            if (!_context.Technicians.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Lấy tất cả user có role Technician
                    var technicianUsers = await _context.Users
                        .Join(_context.UserRoles,
                            u => u.Id,
                            ur => ur.UserId,
                            (u, ur) => new { User = u, RoleId = ur.RoleId })
                        .Join(_context.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => new { ur.User, RoleName = r.Name })
                        .Where(x => x.RoleName == "Technician")
                        .ToListAsync();

                    if (!technicianUsers.Any())
                    {
                        throw new Exception("Không tìm thấy user nào có role Technician");
                    }

                    var technicians = new List<Technician>();
                    var random = new Random();

                    foreach (var techUser in technicianUsers)
                    {
                        // Tạo điểm số ngẫu nhiên nhưng chất lượng
                        var quality = (float)Math.Round(random.NextDouble() * 3 + 7, 1); // 7.0 - 10.0
                        var speed = (float)Math.Round(random.NextDouble() * 3 + 6.5, 1); // 6.5 - 9.5
                        var efficiency = (float)Math.Round(random.NextDouble() * 3 + 7.2, 1); // 7.2 - 10.2

                        // Tính điểm trung bình (có thể weighted nếu cần)
                        var score = (float)Math.Round((quality + speed + efficiency) / 3, 1);

                        var technician = new Technician
                        {
                            TechnicianId = Guid.NewGuid(),
                            UserId = techUser.User.Id,
                            Quality = quality,
                            Speed = speed,
                            Efficiency = efficiency,
                            Score = score
                        };

                        technicians.Add(technician);
                    }

                    _context.Technicians.AddRange(technicians);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    Console.WriteLine($"Seeded {technicians.Count} technicians successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error seeding technicians: {ex.Message}");
                    throw;
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
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Role Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Branch Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Service Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Promotional Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Part Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Log Monitoring" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Policy Security" }
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
            var branchCatId = categories.First(c => c.Name == "Branch Management").Id;
            var serviceCatId = categories.First(c => c.Name == "Service Management").Id;
            var promotionalCatId = categories.First(c => c.Name == "Promotional Management").Id;
            var partCatId = categories.First(c => c.Name == "Part Management").Id;
            var logCatId = categories.First(c => c.Name == "Log Monitoring").Id;
            var policyCatId = categories.First(c => c.Name == "Policy Security").Id;

            var defaultPermissions = new List<Permission>
                {
                    // ✅ User Management
                    new Permission { Id = Guid.NewGuid(), Code = "USER_VIEW", Name = "View Users", Description = "Can view user list", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "USER_EDIT", Name = "Edit Users", Description = "Can edit user info", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "USER_DELETE", Name = "Delete Users", Description = "Can delete users", CategoryId = userCatId },

                    // ✅ Role Management
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_CREATE", Name = "Create Role", Description = "Can create roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_UPDATE", Name = "Update Role", Description = "Can update roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_DELETE", Name = "Delete Role", Description = "Can delete roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_VIEW", Name = "View Roles", Description = "Can view roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PERMISSION_ASSIGN", Name = "Assign Permissions", Description = "Can assign permissions to roles", CategoryId = roleCatId },

                    // ✅ Booking Management
                    new Permission { Id = Guid.NewGuid(), Code = "BOOKING_VIEW", Name = "View Bookings", Description = "Can view booking records", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BOOKING_MANAGE", Name = "Manage Bookings", Description = "Can manage booking details", CategoryId = bookingCatId },

                    // ✅ Branch Management
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_VIEW", Name = "View Branches", Description = "Can view branch list", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_CREATE", Name = "Create Branch", Description = "Can create branches", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_UPDATE", Name = "Update Branch", Description = "Can update branch info", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_DELETE", Name = "Delete Branch", Description = "Can delete branches", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_STATUS_TOGGLE", Name = "Toggle Branch Status", Description = "Can activate/deactivate branches", CategoryId = branchCatId },

                    // ✅ Service Management
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_VIEW", Name = "View Services", Description = "Can view services", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_CREATE", Name = "Create Service", Description = "Can create new services", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_UPDATE", Name = "Update Service", Description = "Can update service information", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_DELETE", Name = "Delete Service", Description = "Can delete services", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_STATUS_TOGGLE", Name = "Toggle Service Status", Description = "Can activate/deactivate services", CategoryId = serviceCatId },

                    // ✅ Promotional Management
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_VIEW", Name = "View Promotions", Description = "Can view promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_CREATE", Name = "Create Promotion", Description = "Can create promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_UPDATE", Name = "Update Promotion", Description = "Can update promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_DELETE", Name = "Delete Promotion", Description = "Can delete promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_TOGGLE", Name = "Toggle Promotion Status", Description = "Can activate/deactivate promotions", CategoryId = promotionalCatId },

                    // ✅ Part Management
                    new Permission { Id = Guid.NewGuid(), Code = "PART_VIEW", Name = "View Parts", Description = "Can view parts", CategoryId = partCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PART_CREATE", Name = "Create Part", Description = "Can create new parts", CategoryId = partCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PART_UPDATE", Name = "Update Part", Description = "Can update part information", CategoryId = partCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PART_DELETE", Name = "Delete Part", Description = "Can delete parts", CategoryId = partCatId },
                    
                    // ✅ Vehicle Management
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_VIEW", Name = "View Vehicles", Description = "Can view vehicles", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_CREATE", Name = "Create Vehicle", Description = "Can create new vehicles", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_UPDATE", Name = "Update Vehicle", Description = "Can update vehicle information", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_DELETE", Name = "Delete Vehicle", Description = "Can delete vehicles", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_SCHEDULE", Name = "Schedule Vehicle Service", Description = "Can schedule vehicle services", CategoryId = bookingCatId },

                     // ✅ Log View

                     new Permission { Id = Guid.NewGuid(), Code = "LOG_VIEW", Name = "View Logs", Description = "Can view Logs page", CategoryId = logCatId },

                     // ✅ PolicySecurity

                     new Permission { Id = Guid.NewGuid(), Code = "POLICY_MANAGEMENT", Name = "Policy Management", Description = "Can view and update, revert Policy,Policy history.", CategoryId = policyCatId }

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
                            {
                                "Admin", new[]
                                {
                                    // User
                                    "USER_VIEW", "USER_EDIT", "USER_DELETE",
            
                                    // Booking
                                    "BOOKING_VIEW", "BOOKING_MANAGE",
            
                                    // Role
                                    "ROLE_VIEW", "ROLE_CREATE", "ROLE_UPDATE", "ROLE_DELETE", "PERMISSION_ASSIGN",
            
                                    // ✅ Branch Management
                                    "BRANCH_VIEW", "BRANCH_CREATE", "BRANCH_UPDATE", "BRANCH_DELETE", "BRANCH_STATUS_TOGGLE",
            
                                    // ✅ Service Management
                                    "SERVICE_VIEW", "SERVICE_CREATE", "SERVICE_UPDATE", "SERVICE_DELETE", "SERVICE_STATUS_TOGGLE",
            
                                    // ✅ Promotional Management
                                    "PROMO_VIEW", "PROMO_CREATE", "PROMO_UPDATE", "PROMO_DELETE", "PROMO_TOGGLE",
                                    // ✅ LOG MONITORING
                                    "LOG_VIEW" , 
                                    
                                    // ✅ Security Policy
                                    "POLICY_MANAGEMENT"
                                }
                            },
                            {
                                "Manager", new[]
                                {
                                    "USER_VIEW", "BOOKING_VIEW", "BOOKING_MANAGE",
                                    "BRANCH_VIEW", "SERVICE_VIEW", "PROMO_VIEW",
                                    "VEHICLE_VIEW", "VEHICLE_CREATE", "VEHICLE_UPDATE", "VEHICLE_SCHEDULE"
                                }
                            },
                            {
                                "Customer", new[] { "BOOKING_VIEW" }
                            },
                            {
                                "Technician", new[] { "BOOKING_MANAGE" }
                            }
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
            new PartCategory { CategoryName = "Front - Engine" },
            new PartCategory { CategoryName = "Rear - Engine" },
            new PartCategory { CategoryName = "Front - Brakes" },
            new PartCategory { CategoryName = "Rear - Brakes" },
            new PartCategory { CategoryName = "Front - Electrical System" },
            new PartCategory { CategoryName = "Rear - Electrical System" },
            new PartCategory { CategoryName = "Front - Suspension" },
            new PartCategory { CategoryName = "Rear - Suspension" },
            new PartCategory { CategoryName = "Front - Cooling System" },
            new PartCategory { CategoryName = "Rear - Cooling System" }
        };

                _context.PartCategories.AddRange(categories);
                await _context.SaveChangesAsync();
            }
        }


        private async Task SeedPartsAsync()
        {
            if (!_context.Parts.Any())
            {
                var categories = await _context.PartCategories.ToListAsync();

                var parts = new List<Part>();

                PartCategory? FindCategory(string name) =>
                    categories.FirstOrDefault(c => c.CategoryName == name);

                // 🔧 Engine
                parts.AddRange(new[]
                {
            new Part { Name = "Air Filter (Cheap)", PartCategoryId = FindCategory("Front - Engine").LaborCategoryId, Price = 120000, Stock = 60, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Oil Filter (Medium)", PartCategoryId = FindCategory("Rear - Engine").LaborCategoryId, Price = 250000, Stock = 40, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Spark Plug (Expensive)", PartCategoryId = FindCategory("Front - Engine").LaborCategoryId, Price = 500000, Stock = 20, CreatedAt = DateTime.UtcNow },
        });

                // 🛑 Brakes
                parts.AddRange(new[]
                {
            new Part { Name = "Brake Pad (Cheap)", PartCategoryId = FindCategory("Front - Brakes").LaborCategoryId, Price = 300000, Stock = 50, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Brake Disc (Medium)", PartCategoryId = FindCategory("Rear - Brakes").LaborCategoryId, Price = 600000, Stock = 25, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Brake Caliper (Expensive)", PartCategoryId = FindCategory("Front - Brakes").LaborCategoryId, Price = 1200000, Stock = 15, CreatedAt = DateTime.UtcNow },
        });

                // ⚡ Electrical System
                parts.AddRange(new[]
                {
            new Part { Name = "Battery (Cheap)", PartCategoryId = FindCategory("Front - Electrical System").LaborCategoryId, Price = 900000, Stock = 30, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Alternator (Medium)", PartCategoryId = FindCategory("Rear - Electrical System").LaborCategoryId, Price = 1800000, Stock = 20, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Starter Motor (Expensive)", PartCategoryId = FindCategory("Front - Electrical System").LaborCategoryId, Price = 2800000, Stock = 10, CreatedAt = DateTime.UtcNow },
        });

                // 🦾 Suspension
                parts.AddRange(new[]
                {
            new Part { Name = "Shock Absorber (Cheap)", PartCategoryId = FindCategory("Front - Suspension").LaborCategoryId, Price = 700000, Stock = 35, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Control Arm (Medium)", PartCategoryId = FindCategory("Rear - Suspension").LaborCategoryId, Price = 950000, Stock = 25, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Suspension Strut (Expensive)", PartCategoryId = FindCategory("Front - Suspension").LaborCategoryId, Price = 1600000, Stock = 12, CreatedAt = DateTime.UtcNow },
        });

                // 🌡️ Cooling System
                parts.AddRange(new[]
                {
            new Part { Name = "Coolant Hose (Cheap)", PartCategoryId = FindCategory("Front - Cooling System").LaborCategoryId, Price = 150000, Stock = 45, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Radiator (Medium)", PartCategoryId = FindCategory("Rear - Cooling System").LaborCategoryId, Price = 1900000, Stock = 10, CreatedAt = DateTime.UtcNow },
            new Part { Name = "Water Pump (Expensive)", PartCategoryId = FindCategory("Front - Cooling System").LaborCategoryId, Price = 2600000, Stock = 8, CreatedAt = DateTime.UtcNow },
        });

                _context.Parts.AddRange(parts);
                await _context.SaveChangesAsync();
            }
        }


        private async Task SeedServiceCategoriesAsync()
        {
            if (!_context.ServiceCategories.Any())
            {
                // --- STEP 1: Create parent categories ---
                var parentCategories = new List<ServiceCategory>
        {
            new ServiceCategory { CategoryName = "Maintenance", Description = "General maintenance services for vehicles" },
            new ServiceCategory { CategoryName = "Repair", Description = "Repair services for damaged parts" },
            new ServiceCategory { CategoryName = "Inspection", Description = "Vehicle inspection and diagnostics" },
            new ServiceCategory { CategoryName = "Upgrade", Description = "Performance and aesthetic upgrades" }
        };

                _context.ServiceCategories.AddRange(parentCategories);
                await _context.SaveChangesAsync();

                // --- STEP 2: Create child categories ---
                var maintenance = parentCategories.First(c => c.CategoryName == "Maintenance");
                var repair = parentCategories.First(c => c.CategoryName == "Repair");
                var inspection = parentCategories.First(c => c.CategoryName == "Inspection");
                var upgrade = parentCategories.First(c => c.CategoryName == "Upgrade");

                var childCategories = new List<ServiceCategory>
        {
            // 🔧 Maintenance
            new ServiceCategory { CategoryName = "Oil Change", ParentServiceCategoryId = maintenance.ServiceCategoryId, Description = "Engine oil and filter replacement" },
            new ServiceCategory { CategoryName = "Tire Rotation", ParentServiceCategoryId = maintenance.ServiceCategoryId, Description = "Rotating tires for even wear" },
            new ServiceCategory { CategoryName = "Battery Check", ParentServiceCategoryId = maintenance.ServiceCategoryId, Description = "Battery testing and replacement" },
            new ServiceCategory { CategoryName = "Fluid Refill", ParentServiceCategoryId = maintenance.ServiceCategoryId, Description = "Coolant, brake fluid, and transmission fluid refill" },

            // ⚙️ Repair
            new ServiceCategory { CategoryName = "Engine Repair", ParentServiceCategoryId = repair.ServiceCategoryId, Description = "Engine part replacement and tuning" },
            new ServiceCategory { CategoryName = "Brake Repair", ParentServiceCategoryId = repair.ServiceCategoryId, Description = "Brake pad, caliper, and disc replacement" },
            new ServiceCategory { CategoryName = "Electrical Repair", ParentServiceCategoryId = repair.ServiceCategoryId, Description = "Fixing alternator, starter motor, and wiring issues" },
            new ServiceCategory { CategoryName = "Suspension Repair", ParentServiceCategoryId = repair.ServiceCategoryId, Description = "Shock absorber and suspension arm repair" },

            // 🔍 Inspection
            new ServiceCategory { CategoryName = "Safety Inspection", ParentServiceCategoryId = inspection.ServiceCategoryId, Description = "Check safety systems like brakes, lights, and tires" },
            new ServiceCategory { CategoryName = "Emissions Inspection", ParentServiceCategoryId = inspection.ServiceCategoryId, Description = "Check exhaust and emissions compliance" },
            new ServiceCategory { CategoryName = "Pre-Purchase Inspection", ParentServiceCategoryId = inspection.ServiceCategoryId, Description = "Comprehensive vehicle check before buying" },
            new ServiceCategory { CategoryName = "Engine Diagnostic", ParentServiceCategoryId = inspection.ServiceCategoryId, Description = "Computer-based engine and sensor diagnostics" },

            // 🏎️ Upgrade
            new ServiceCategory { CategoryName = "Performance Tuning", ParentServiceCategoryId = upgrade.ServiceCategoryId, Description = "Boost vehicle performance and horsepower" },
            new ServiceCategory { CategoryName = "Lighting Upgrade", ParentServiceCategoryId = upgrade.ServiceCategoryId, Description = "Install LED or HID lighting systems" },
            new ServiceCategory { CategoryName = "Interior Upgrade", ParentServiceCategoryId = upgrade.ServiceCategoryId, Description = "Improve interior design and comfort" },
            new ServiceCategory { CategoryName = "Exterior Styling", ParentServiceCategoryId = upgrade.ServiceCategoryId, Description = "Add body kits, spoilers, and paint customization" }
        };

                _context.ServiceCategories.AddRange(childCategories);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedServicesAsync()
        {
            if (!_context.Services.Any())
            {
                // Load all categories into memory
                var categories = await _context.ServiceCategories.ToListAsync();

                // Helper method to find a category by name
                Guid GetCategoryId(string name) => categories.First(c => c.CategoryName == name).ServiceCategoryId;

                var services = new List<Service>
        {
            // 🔧 Maintenance
            new Service
            {
                ServiceName = "Basic Oil Change",
                Description = "Drain old oil and refill with standard engine oil.",
                ServiceCategoryId = GetCategoryId("Oil Change"),
                Price = 300000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Premium Oil Change",
                Description = "Use synthetic oil for better performance and protection.",
                ServiceCategoryId = GetCategoryId("Oil Change"),
                Price = 550000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Tire Rotation Service",
                Description = "Rotate tires to ensure even wear and longer life.",
                ServiceCategoryId = GetCategoryId("Tire Rotation"),
                Price = 200000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Battery Health Check",
                Description = "Inspect and test vehicle battery condition.",
                ServiceCategoryId = GetCategoryId("Battery Check"),
                Price = 150000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // ⚙️ Repair
            new Service
            {
                ServiceName = "Engine Tune-Up",
                Description = "Adjust and replace necessary engine components for smoother performance.",
                ServiceCategoryId = GetCategoryId("Engine Repair"),
                Price = 1800000,
                EstimatedDuration = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Brake Pad Replacement",
                Description = "Replace worn brake pads and check calipers and rotors.",
                ServiceCategoryId = GetCategoryId("Brake Repair"),
                Price = 900000,
                EstimatedDuration = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Electrical Wiring Repair",
                Description = "Diagnose and repair wiring or connection issues.",
                ServiceCategoryId = GetCategoryId("Electrical Repair"),
                Price = 1000000,
                EstimatedDuration = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Shock Absorber Replacement",
                Description = "Replace front or rear shock absorbers for better ride quality.",
                ServiceCategoryId = GetCategoryId("Suspension Repair"),
                Price = 1400000,
                EstimatedDuration = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // 🔍 Inspection
            new Service
            {
                ServiceName = "Basic Safety Inspection",
                Description = "Inspect brakes, tires, and lights for safety compliance.",
                ServiceCategoryId = GetCategoryId("Safety Inspection"),
                Price = 350000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Emissions Test",
                Description = "Check emission levels to meet environmental regulations.",
                ServiceCategoryId = GetCategoryId("Emissions Inspection"),
                Price = 400000,
                EstimatedDuration = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Pre-Purchase Inspection",
                Description = "Full vehicle inspection before purchase, including test drive and report.",
                ServiceCategoryId = GetCategoryId("Pre-Purchase Inspection"),
                Price = 600000,
                EstimatedDuration = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Full Engine Diagnostic",
                Description = "Use OBD tools to detect engine faults and suggest repairs.",
                ServiceCategoryId = GetCategoryId("Engine Diagnostic"),
                Price = 700000,
                EstimatedDuration = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // 🏎️ Upgrade
            new Service
            {
                ServiceName = "ECU Performance Tuning",
                Description = "Remap ECU software for optimized performance and fuel efficiency.",
                ServiceCategoryId = GetCategoryId("Performance Tuning"),
                Price = 2500000,
                EstimatedDuration = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "LED Lighting Installation",
                Description = "Upgrade headlights and taillights to modern LED systems.",
                ServiceCategoryId = GetCategoryId("Lighting Upgrade"),
                Price = 800000,
                EstimatedDuration = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Interior Detailing",
                Description = "Deep clean and restore car interior with premium materials.",
                ServiceCategoryId = GetCategoryId("Interior Upgrade"),
                Price = 1000000,
                EstimatedDuration = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                ServiceName = "Exterior Body Kit Installation",
                Description = "Install custom bumpers, spoilers, and side skirts for a sporty look.",
                ServiceCategoryId = GetCategoryId("Exterior Styling"),
                Price = 3200000,
                EstimatedDuration = 5,
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
                // Lấy các service từ database - sửa tên service cho khớp với dữ liệu đã seed
                var basicOilChange = await _context.Services.FirstAsync(s => s.ServiceName == "Basic Oil Change");
                var premiumOilChange = await _context.Services.FirstAsync(s => s.ServiceName == "Premium Oil Change");
                var brakePadReplacement = await _context.Services.FirstAsync(s => s.ServiceName == "Brake Pad Replacement");
                var batteryHealthCheck = await _context.Services.FirstAsync(s => s.ServiceName == "Battery Health Check");
                var shockAbsorberReplacement = await _context.Services.FirstAsync(s => s.ServiceName == "Shock Absorber Replacement");
                var fullEngineDiagnostic = await _context.Services.FirstAsync(s => s.ServiceName == "Full Engine Diagnostic");

                // Lấy các parts từ database - sửa tên part cho khớp với dữ liệu đã seed
                var airFilterCheap = await _context.Parts.FirstAsync(p => p.Name == "Air Filter (Cheap)");
                var oilFilterMedium = await _context.Parts.FirstAsync(p => p.Name == "Oil Filter (Medium)");
                var sparkPlugExpensive = await _context.Parts.FirstAsync(p => p.Name == "Spark Plug (Expensive)");
                var brakePadCheap = await _context.Parts.FirstAsync(p => p.Name == "Brake Pad (Cheap)");
                var brakeDiscMedium = await _context.Parts.FirstAsync(p => p.Name == "Brake Disc (Medium)");
                var batteryCheap = await _context.Parts.FirstAsync(p => p.Name == "Battery (Cheap)");
                var alternatorMedium = await _context.Parts.FirstAsync(p => p.Name == "Alternator (Medium)");
                var shockAbsorberCheap = await _context.Parts.FirstAsync(p => p.Name == "Shock Absorber (Cheap)");
                var controlArmMedium = await _context.Parts.FirstAsync(p => p.Name == "Control Arm (Medium)");
                var radiatorMedium = await _context.Parts.FirstAsync(p => p.Name == "Radiator (Medium)");
                var coolantHoseCheap = await _context.Parts.FirstAsync(p => p.Name == "Coolant Hose (Cheap)");

                var mappings = new List<ServicePart>
        {
            // 🔧 Basic Oil Change Service
            new ServicePart { ServiceId = basicOilChange.ServiceId, PartId = oilFilterMedium.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = basicOilChange.ServiceId, PartId = airFilterCheap.PartId, CreatedAt = DateTime.UtcNow },

            // 🔧 Premium Oil Change Service - dùng linh kiện cao cấp hơn
            new ServicePart { ServiceId = premiumOilChange.ServiceId, PartId = oilFilterMedium.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = premiumOilChange.ServiceId, PartId = sparkPlugExpensive.PartId, CreatedAt = DateTime.UtcNow },

            // 🚗 Brake Pad Replacement
            new ServicePart { ServiceId = brakePadReplacement.ServiceId, PartId = brakePadCheap.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = brakePadReplacement.ServiceId, PartId = brakeDiscMedium.PartId, CreatedAt = DateTime.UtcNow },

            // 🔋 Battery Health Check - có thể cần thay thế
            new ServicePart { ServiceId = batteryHealthCheck.ServiceId, PartId = batteryCheap.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = batteryHealthCheck.ServiceId, PartId = alternatorMedium.PartId, CreatedAt = DateTime.UtcNow },

            // 🛞 Shock Absorber Replacement
            new ServicePart { ServiceId = shockAbsorberReplacement.ServiceId, PartId = shockAbsorberCheap.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = shockAbsorberReplacement.ServiceId, PartId = controlArmMedium.PartId, CreatedAt = DateTime.UtcNow },

            // 🔍 Full Engine Diagnostic - các linh kiện cần kiểm tra
            new ServicePart { ServiceId = fullEngineDiagnostic.ServiceId, PartId = sparkPlugExpensive.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = fullEngineDiagnostic.ServiceId, PartId = airFilterCheap.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = fullEngineDiagnostic.ServiceId, PartId = coolantHoseCheap.PartId, CreatedAt = DateTime.UtcNow },
            new ServicePart { ServiceId = fullEngineDiagnostic.ServiceId, PartId = radiatorMedium.PartId, CreatedAt = DateTime.UtcNow }
        };

                _context.ServiceParts.AddRange(mappings);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedBranchesAsync()
        {
            if (!_context.Branches.Any())
            {
                var branches = new List<Branch>
        {
            new Branch
            {
                BranchName = "Central Garage - Hồ Chí Minh",
                Description = "Main branch providing full vehicle maintenance and repair services in Ho Chi Minh City.",
                Street = "123 Nguyễn Thị Minh Khai",
                Commune = "Phường Bến Thành",
                
                Province = "Hồ Chí Minh",
                PhoneNumber = "02838220001",
                Email = "central.hcm@garage.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Latitude=1,
                Longitude=2
            },
            new Branch
            {
                BranchName = "Hà Nội Garage",
                Description = "Professional car repair and maintenance services in Hanoi.",
                Street = "45 Phạm Hùng",
                Commune = "Phường Mỹ Đình 2",
                
                Province = "Hà Nội",
                PhoneNumber = "02437760002",
                Email = "hanoi@garage.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                 Latitude=1,
                Longitude=2
            },
            new Branch
            {
                BranchName = "Đà Nẵng Garage",
                Description = "Trusted auto service center for central region customers.",
                Street = "88 Nguyễn Văn Linh",
                Commune = "Phường Nam Dương",
                
                Province = "Đà Nẵng",
                PhoneNumber = "02363880003",
                Email = "danang@garage.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                 Latitude=1,
                Longitude=2
            },
            new Branch
            {
                BranchName = "Cần Thơ Garage",
                Description = "Serving customers in the Mekong Delta with full maintenance packages.",
                Street = "22 Trần Hưng Đạo",
                Commune = "Phường An Cư",
               
                Province = "Cần Thơ",
                PhoneNumber = "02923890004",
                Email = "cantho@garage.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                 Latitude=1,
                Longitude=2
            },
            new Branch
            {
                BranchName = "Nha Trang Garage",
                Description = "High-quality vehicle service center near the coast.",
                Street = "56 Lê Thánh Tôn",
                Commune = "Phường Lộc Thọ",             
                Province= "Khánh Hòa",
                PhoneNumber = "02583560005",
                Email = "nhatrang@garage.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                 Latitude=1,
                Longitude=2
            }
        };

                // Seed OperatingHours (7 ngày cho tất cả chi nhánh)
                foreach (var branch in branches)
                {
                    foreach (DayOfWeekEnum day in Enum.GetValues(typeof(DayOfWeekEnum)))
                    {
                        branch.OperatingHours.Add(new OperatingHour
                        {
                            DayOfWeek = day,
                            IsOpen = true,
                            OpenTime = new TimeSpan(8, 0, 0),   // 08:00
                            CloseTime = new TimeSpan(17, 30, 0) // 17:30
                        });
                    }

                    // Gán staff
                    var managerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000002");
                    var technicianUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000006");

                    if (managerUser != null) branch.Staffs.Add(managerUser);
                    if (technicianUser != null) branch.Staffs.Add(technicianUser);

                    // Gán dịch vụ
                    var services = await _context.Services.Take(5).ToListAsync();
                    foreach (var service in services)
                    {
                        branch.BranchServices.Add(new BranchService
                        {
                            Branch = branch,
                            Service = service
                        });
                    }
                }

                _context.Branches.AddRange(branches);
                await _context.SaveChangesAsync();
            }
        }


        private async Task SeedOrderStatusesAsync()
        {
            if (!_context.OrderStatuses.Any())
            {
                var orderStatuses = new List<OrderStatus>
                {
                    new OrderStatus { StatusName = "Pending" },
                    new OrderStatus { StatusName = "In Progress" },
                    new OrderStatus { StatusName = "Completed" }
                };

                _context.OrderStatuses.AddRange(orderStatuses);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedLabelsAsync()
        {
            if (!_context.Labels.Any())
            {
                var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(os => os.StatusName == "Pending");
                var inProgressStatus = await _context.OrderStatuses.FirstOrDefaultAsync(os => os.StatusName == "In Progress");
                var completedStatus = await _context.OrderStatuses.FirstOrDefaultAsync(os => os.StatusName == "Completed");

                // Only create labels if we have the corresponding order statuses
                if (pendingStatus != null && inProgressStatus != null && completedStatus != null)
                {
                    // Check if labels already exist
                    if (!_context.Labels.Any())
                    {
                        var labels = new List<Label>
                        {
                            new Label
                            {
                                LabelName = "Pending",
                                Description = "Order is waiting to be processed",
                                OrderStatusId = pendingStatus.OrderStatusId, // Now using int ID
                                ColorName = "Red",
                                HexCode = "#FF0000"
                            },
                            new Label
                            {
                                LabelName = "In Progress",
                                Description = "Order is being worked on",
                                OrderStatusId = inProgressStatus.OrderStatusId, // Now using int ID
                                ColorName = "Yellow",
                                HexCode = "#FFFF00"
                            },
                            new Label
                            {
                                LabelName = "Done",
                                Description = "Order completed",
                                OrderStatusId = completedStatus.OrderStatusId, // Now using int ID
                                ColorName = "Green",
                                HexCode = "#00FF00"
                            }
                        };

                        _context.Labels.AddRange(labels);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task SeedVehicleRelatedEntitiesAsync()
        {
            // Seed Vehicle Brands
            if (!_context.VehicleBrands.Any())
            {
                var brands = new List<VehicleBrand>
                {
                    new VehicleBrand
                    {
                        BrandID = Guid.NewGuid(),
                        BrandName = "Toyota",
                        Country = "Japan",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleBrand
                    {
                        BrandID = Guid.NewGuid(),
                        BrandName = "Honda",
                        Country = "Japan",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleBrand
                    {
                        BrandID = Guid.NewGuid(),
                        BrandName = "Ford",
                        Country = "USA",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleBrand
                    {
                        BrandID = Guid.NewGuid(),
                        BrandName = "BMW",
                        Country = "Germany",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleBrand
                    {
                        BrandID = Guid.NewGuid(),
                        BrandName = "Mercedes-Benz",
                        Country = "Germany",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                _context.VehicleBrands.AddRange(brands);
                await _context.SaveChangesAsync();
            }

            // Seed Vehicle Colors
            if (!_context.VehicleColors.Any())
            {
                var colors = new List<VehicleColor>
                {
                    new VehicleColor
                    {
                        ColorID = Guid.NewGuid(),
                        ColorName = "White",
                        HexCode = "#FFFFFF",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleColor
                    {
                        ColorID = Guid.NewGuid(),
                        ColorName = "Black",
                        HexCode = "#000000",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleColor
                    {
                        ColorID = Guid.NewGuid(),
                        ColorName = "Silver",
                        HexCode = "#C0C0C0",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleColor
                    {
                        ColorID = Guid.NewGuid(),
                        ColorName = "Red",
                        HexCode = "#FF0000",
                        CreatedAt = DateTime.UtcNow
                    },
                    new VehicleColor
                    {
                        ColorID = Guid.NewGuid(),
                        ColorName = "Blue",
                        HexCode = "#0000FF",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                _context.VehicleColors.AddRange(colors);
                await _context.SaveChangesAsync();
            }

            // Seed Vehicle Models (after brands are created)
            if (!_context.VehicleModels.Any())
            {
                var toyotaBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Toyota");
                var hondaBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Honda");
                var fordBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Ford");
                var bmwBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "BMW");
                var mercedesBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Mercedes-Benz");

                if (toyotaBrand != null && hondaBrand != null && fordBrand != null && bmwBrand != null && mercedesBrand != null)
                {
                    var models = new List<VehicleModel>
                    {
                        // Toyota models
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "Camry",
                            ManufacturingYear = 2022,
                            BrandID = toyotaBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "Corolla",
                            ManufacturingYear = 2021,
                            BrandID = toyotaBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Honda models
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "Civic",
                            ManufacturingYear = 2022,
                            BrandID = hondaBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "Accord",
                            ManufacturingYear = 2020,
                            BrandID = hondaBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Ford models
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "Focus",
                            ManufacturingYear = 2021,
                            BrandID = fordBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "Mustang",
                            ManufacturingYear = 2023,
                            BrandID = fordBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        // BMW models
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "3 Series",
                            ManufacturingYear = 2022,
                            BrandID = bmwBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "5 Series",
                            ManufacturingYear = 2021,
                            BrandID = bmwBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        // Mercedes models
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "C-Class",
                            ManufacturingYear = 2022,
                            BrandID = mercedesBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        },
                        new VehicleModel
                        {
                            ModelID = Guid.NewGuid(),
                            ModelName = "E-Class",
                            ManufacturingYear = 2020,
                            BrandID = mercedesBrand.BrandID,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.VehicleModels.AddRange(models);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedVehiclesAsync()
        {
            if (!_context.Vehicles.Any())
            {
                // Get required entities
                var customerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000005"); // Default Customer

                // Get vehicle related entities
                var whiteColor = await _context.VehicleColors.FirstOrDefaultAsync(c => c.ColorName == "White");
                var blackColor = await _context.VehicleColors.FirstOrDefaultAsync(c => c.ColorName == "Black");
                var silverColor = await _context.VehicleColors.FirstOrDefaultAsync(c => c.ColorName == "Silver");
                var redColor = await _context.VehicleColors.FirstOrDefaultAsync(c => c.ColorName == "Red");
                var blueColor = await _context.VehicleColors.FirstOrDefaultAsync(c => c.ColorName == "Blue");

                var toyotaBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Toyota");
                var hondaBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Honda");
                var fordBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Ford");
                var bmwBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "BMW");
                var mercedesBrand = await _context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Mercedes-Benz");

                var camryModel = await _context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "Camry");
                var civicModel = await _context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "Civic");
                var focusModel = await _context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "Focus");
                var series3Model = await _context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "3 Series");
                var cClassModel = await _context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "C-Class");

                if (customerUser != null &&
                    whiteColor != null && blackColor != null && silverColor != null && redColor != null && blueColor != null &&
                    toyotaBrand != null && hondaBrand != null && fordBrand != null && bmwBrand != null && mercedesBrand != null &&
                    camryModel != null && civicModel != null && focusModel != null && series3Model != null && cClassModel != null)
                {
                    var vehicles = new List<Vehicle>
                    {
                        new Vehicle
                        {
                            BrandId = toyotaBrand.BrandID,
                            UserId = customerUser.Id,
                            ModelId = camryModel.ModelID,
                            ColorId = whiteColor.ColorID,
                            LicensePlate = "51F12345",
                            VIN = "1HGBH41JXMN109186",
                            Year = 2020,
                            Odometer = 15000,
                            LastServiceDate = DateTime.UtcNow.AddDays(-30),
                            NextServiceDate = DateTime.UtcNow.AddDays(30),
                            WarrantyStatus = "Active",
                            CreatedAt = DateTime.UtcNow.AddDays(-30)
                        },
                        new Vehicle
                        {
                            BrandId = hondaBrand.BrandID,
                            UserId = customerUser.Id,
                            ModelId = civicModel.ModelID,
                            ColorId = blackColor.ColorID,
                            LicensePlate = "51F67890",
                            VIN = "2T1BURHE5JC012345",
                            Year = 2018,
                            Odometer = 25000,
                            LastServiceDate = DateTime.UtcNow.AddDays(-60),
                            NextServiceDate = DateTime.UtcNow.AddDays(15),
                            WarrantyStatus = "Expired",
                            CreatedAt = DateTime.UtcNow.AddDays(-60)
                        },
                        new Vehicle
                        {
                            BrandId = fordBrand.BrandID,
                            UserId = customerUser.Id,
                            ModelId = focusModel.ModelID,
                            ColorId = silverColor.ColorID,
                            LicensePlate = "51F54321",
                            VIN = "3VWBP29M85M000001",
                            Year = 2022,
                            Odometer = 8000,
                            LastServiceDate = DateTime.UtcNow.AddDays(-15),
                            NextServiceDate = DateTime.UtcNow.AddDays(45),
                            WarrantyStatus = "Active",
                            CreatedAt = DateTime.UtcNow.AddDays(-15)
                        },
                        new Vehicle
                        {
                            BrandId = bmwBrand.BrandID,
                            UserId = customerUser.Id,
                            ModelId = series3Model.ModelID,
                            ColorId = redColor.ColorID,
                            LicensePlate = "51F98765",
                            VIN = "WBAVA33598NL67342",
                            Year = 2021,
                            Odometer = 12000,
                            LastServiceDate = DateTime.UtcNow.AddDays(-45),
                            NextServiceDate = DateTime.UtcNow.AddDays(20),
                            WarrantyStatus = "Active",
                            CreatedAt = DateTime.UtcNow.AddDays(-45)
                        },
                        new Vehicle
                        {
                            BrandId = mercedesBrand.BrandID,
                            UserId = customerUser.Id,
                            ModelId = cClassModel.ModelID,
                            ColorId = blueColor.ColorID,
                            LicensePlate = "51F24680",
                            VIN = "WDDHF8JB0DA123456",
                            Year = 2019,
                            Odometer = 35000,
                            LastServiceDate = DateTime.UtcNow.AddDays(-90),
                            NextServiceDate = DateTime.UtcNow.AddDays(10),
                            WarrantyStatus = "Expiring Soon",
                            CreatedAt = DateTime.UtcNow.AddDays(-90)
                        }
                    };

                    _context.Vehicles.AddRange(vehicles);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedPromotionalCampaignsWithServicesAsync()
        {
            if (!_context.PromotionalCampaigns.Any())
            {
                var campaigns = new List<PromotionalCampaign>
        {
            new PromotionalCampaign
            {
                Id = Guid.NewGuid(),
                Name = "Grand Opening Discount",
                Description = "Celebrate our grand opening with 20% off all services!",
                Type = CampaignType.Discount,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20,
                StartDate = DateTime.UtcNow.AddDays(-3),
                EndDate = DateTime.UtcNow.AddDays(7),
                MinimumOrderValue = 0,
                MaximumDiscount = 500000,
                UsageLimit = 200,
                UsedCount = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PromotionalCampaign
            {
                Id = Guid.NewGuid(),
                Name = "Loyalty Appreciation",
                Description = "Fixed discount of 150,000₫ for our returning customers.",
                Type = CampaignType.Discount,
                DiscountType = DiscountType.Fixed,
                DiscountValue = 150000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                MinimumOrderValue = 500000,
                MaximumDiscount = null,
                UsageLimit = 100,
                UsedCount = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PromotionalCampaign
            {
                Id = Guid.NewGuid(),
                Name = "Year-End Free Checkup",
                Description = "Get a free maintenance check for any service above 1,000,000₫.",
                Type = CampaignType.Discount,
                DiscountType = DiscountType.FreeService,
                DiscountValue = 0,
                StartDate = new DateTime(DateTime.UtcNow.Year, 12, 1),
                EndDate = new DateTime(DateTime.UtcNow.Year, 12, 31),
                MinimumOrderValue = 1000000,
                MaximumDiscount = null,
                UsageLimit = 300,
                UsedCount = 0,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

                _context.PromotionalCampaigns.AddRange(campaigns);
                await _context.SaveChangesAsync();

                // --- Sau khi lưu campaign, thêm liên kết Service ---
                var services = await _context.Services.ToListAsync();
                if (services.Any())
                {
                    var promoCampaignServices = new List<PromotionalCampaignService>();

                    foreach (var campaign in campaigns)
                    {
                        // Lấy ngẫu nhiên 2 dịch vụ đầu tiên cho demo
                        var selectedServices = services.Take(2).ToList();

                        foreach (var service in selectedServices)
                        {
                            promoCampaignServices.Add(new PromotionalCampaignService
                            {
                                PromotionalCampaignId = campaign.Id,
                                ServiceId = service.ServiceId
                            });
                        }
                    }

                    _context.PromotionalCampaignServices.AddRange(promoCampaignServices);
                    await _context.SaveChangesAsync();
                }
            }
        }





        private async Task SeedRepairOrdersAsync()
        {
            if (!_context.RepairOrders.Any())
            {
                // Truy vấn database để lấy userId có role Customer
                var customer = await _context.Users
                    .Join(_context.UserRoles,
                        u => u.Id,
                        ur => ur.UserId,
                        (u, ur) => new { User = u, RoleId = ur.RoleId })
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.User, RoleName = r.Name })
                    .FirstOrDefaultAsync(x => x.RoleName == "Customer");

                if (customer == null)
                {
                    throw new Exception("Không tìm thấy user có role Customer trong database");
                }

                var userId = customer.User.Id;

                // Truy vấn database để lấy technicianIds có role Technician
                var technicians = await _context.Technicians
                    .Include(t => t.User) // Include user information if needed
                    .Take(2)
                    .ToListAsync();


                if (technicians.Count < 2)
                {
                    throw new Exception("Cần ít nhất 2 technicians trong database");
                }

                var technicianIds = technicians.Select(t => t.TechnicianId).ToList();
               

                // Truy vấn vehicleId từ database
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleId == Guid.Parse("6D960DA7-D0A8-4C8A-8E8F-1BE2024A5DC6"));

                if (vehicle == null)
                {
                    // Nếu không tìm thấy vehicle với ID cụ thể, lấy vehicle đầu tiên
                    vehicle = await _context.Vehicles.FirstAsync();
                }

                var vehicleId = vehicle.VehicleId;

                // Lấy các service từ database
                var basicOilChange = await _context.Services.FirstAsync(s => s.ServiceName == "Basic Oil Change");
                var brakePadReplacement = await _context.Services.FirstAsync(s => s.ServiceName == "Brake Pad Replacement");
                var engineTuneUp = await _context.Services.FirstAsync(s => s.ServiceName == "Engine Tune-Up");
                var fullEngineDiagnostic = await _context.Services.FirstAsync(s => s.ServiceName == "Full Engine Diagnostic");
                var tireRotation = await _context.Services.FirstAsync(s => s.ServiceName == "Tire Rotation Service");

                // Lấy các parts từ database
                var oilFilterMedium = await _context.Parts.FirstAsync(p => p.Name == "Oil Filter (Medium)");
                var airFilterCheap = await _context.Parts.FirstAsync(p => p.Name == "Air Filter (Cheap)");
                var brakePadCheap = await _context.Parts.FirstAsync(p => p.Name == "Brake Pad (Cheap)");
                var brakeDiscMedium = await _context.Parts.FirstAsync(p => p.Name == "Brake Disc (Medium)");
                var sparkPlugExpensive = await _context.Parts.FirstAsync(p => p.Name == "Spark Plug (Expensive)");
                var shockAbsorberCheap = await _context.Parts.FirstAsync(p => p.Name == "Shock Absorber (Cheap)");

                // Lấy branch và status
                var branch = await _context.Branches.FirstAsync();
                var pendingStatus = await _context.OrderStatuses.FirstAsync(s => s.StatusName == "Pending");
                var inProgressStatus = await _context.OrderStatuses.FirstAsync(s => s.StatusName == "In Progress");
                var completedStatus = await _context.OrderStatuses.FirstAsync(s => s.StatusName == "Completed");

                // Get required entities for creating repair requests
                var customerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000005"); // Default Customer
                if (customerUser == null)
                {
                    throw new Exception("Customer user not found for seeding repair requests");
                }

                // Create Repair Requests first
                var repairRequests = new List<RepairRequest>
                {
                    new RepairRequest
                    {
                        RepairRequestID = Guid.NewGuid(),
                        VehicleID = vehicleId,
                        UserID = customerUser.Id,
                        Description = "Regular maintenance service - Oil change and basic check",
                        BranchId = branch.BranchId,
                        RequestDate = DateTime.UtcNow.AddDays(-5),
                        CompletedDate = DateTime.UtcNow.AddDays(-4),
                        Status = RepairRequestStatus.Completed,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow.AddDays(-4),
                        EstimatedCost = 1500000,
                        ArrivalWindowStart = DateTimeOffset.UtcNow.AddDays(-5)
                    },
                    new RepairRequest
                    {
                        RepairRequestID = Guid.NewGuid(),
                        VehicleID = vehicleId,
                        UserID = customerUser.Id,
                        Description = "Brake system repair - Waiting for customer approval",
                        BranchId = branch.BranchId,
                        RequestDate = DateTime.UtcNow.AddDays(-2),
                        Status = RepairRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow,
                        EstimatedCost = 2500000,
                        ArrivalWindowStart = DateTimeOffset.UtcNow.AddDays(-2)
                    },
                    new RepairRequest
                    {
                        RepairRequestID = Guid.NewGuid(),
                        VehicleID = vehicleId,
                        UserID = customerUser.Id,
                        Description = "Emergency brake and engine repair - Urgent service required",
                        BranchId = branch.BranchId,
                        RequestDate = DateTime.UtcNow.AddDays(-1),
                        Status = RepairRequestStatus.Arrived,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow,
                        EstimatedCost = 3500000,
                        ArrivalWindowStart = DateTimeOffset.UtcNow.AddDays(-1)
                    },
                    new RepairRequest
                    {
                        RepairRequestID = Guid.NewGuid(),
                        VehicleID = vehicleId,
                        UserID = customerUser.Id,
                        Description = "Tire rotation and basic inspection",
                        BranchId = branch.BranchId,
                        RequestDate = DateTime.UtcNow,
                        Status = RepairRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        EstimatedCost = 800000,
                        ArrivalWindowStart = DateTimeOffset.UtcNow
                    }
                };

                _context.RepairRequests.AddRange(repairRequests);
                await _context.SaveChangesAsync();

                // Tạo Repair Orders
                var repairOrders = new List<RepairOrder>
        {
            new RepairOrder
            {
                RepairOrderId = Guid.NewGuid(),
                ReceiveDate = DateTime.UtcNow.AddDays(-5),
                RoType = RoType.Scheduled,
                EstimatedCompletionDate = DateTime.UtcNow.AddDays(2),
                CompletionDate = DateTime.UtcNow.AddDays(1),
                Cost = 1450000,
                EstimatedAmount = 1500000,
                PaidAmount = 1450000,
                PaidStatus = PaidStatus.Paid,
                EstimatedRepairTime = 3,
                Note = "Regular maintenance service - Oil change and basic check",
                BranchId = branch.BranchId,
                StatusId = completedStatus.OrderStatusId,
                VehicleId = vehicleId,
                UserId = userId,
                RepairRequestId = null,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(1)
            },
            new RepairOrder
            {
                RepairOrderId = Guid.NewGuid(),
                ReceiveDate = DateTime.UtcNow.AddDays(-2),
                RoType = RoType.WalkIn,
                EstimatedCompletionDate = DateTime.UtcNow.AddDays(5),
                Cost = 0,
                EstimatedAmount = 2500000,
                PaidAmount = 0,
                PaidStatus = PaidStatus.Unpaid,
                EstimatedRepairTime = 5,
                Note = "Brake system repair - Waiting for customer approval",
                BranchId = branch.BranchId,
                StatusId = pendingStatus.OrderStatusId,
                VehicleId = vehicleId,
                UserId = userId,
                RepairRequestId = null,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow
            },
            new RepairOrder
            {
                RepairOrderId = Guid.NewGuid(),
                ReceiveDate = DateTime.UtcNow.AddDays(-1),
                RoType = RoType.Breakdown,
                EstimatedCompletionDate = DateTime.UtcNow.AddDays(1),
                Cost = 3200000,
                EstimatedAmount = 3500000,
                PaidAmount = 2000000,
                PaidStatus = PaidStatus.Unpaid,
                EstimatedRepairTime = 6,
                Note = "Emergency brake and engine repair - Urgent service required",
                BranchId = branch.BranchId,
                StatusId = inProgressStatus.OrderStatusId,
                VehicleId = vehicleId,
                UserId = userId,
                RepairRequestId = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            },
            new RepairOrder
            {
                RepairOrderId = Guid.NewGuid(),
                ReceiveDate = DateTime.UtcNow,
                RoType = RoType.Scheduled,
                EstimatedCompletionDate = DateTime.UtcNow.AddDays(3),
                Cost = 0,
                EstimatedAmount = 800000,
                PaidAmount = 0,
                PaidStatus = PaidStatus.Unpaid,
                EstimatedRepairTime = 2,
                Note = "Tire rotation and basic inspection",
                BranchId = branch.BranchId,
                StatusId = completedStatus.OrderStatusId,
                VehicleId = vehicleId,
                UserId = userId,
                RepairRequestId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

                _context.RepairOrders.AddRange(repairOrders);
                await _context.SaveChangesAsync();

                // Tạo Repair Order Services
                var repairOrderServices = new List<RepairOrderService>
                {
                    // Order 1 - Completed repair
                    new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = repairOrders[0].RepairOrderId,
                        ServiceId = basicOilChange.ServiceId,
                        
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = repairOrders[0].RepairOrderId,
                        ServiceId = tireRotation.ServiceId,
                        
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },

                    // Order 2 - Pending repair
                    new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = repairOrders[1].RepairOrderId,
                        ServiceId = brakePadReplacement.ServiceId,
                        
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },

                    // Order 3 - Emergency repair
                    new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = repairOrders[2].RepairOrderId,
                        ServiceId = brakePadReplacement.ServiceId,
                        
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = repairOrders[2].RepairOrderId,
                        ServiceId = engineTuneUp.ServiceId,
                       
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },

                    // Order 4 - Approved repair
                    new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = repairOrders[3].RepairOrderId,
                        ServiceId = tireRotation.ServiceId,
                        
                        CreatedAt = DateTime.UtcNow
                    }
                };

                _context.RepairOrderServices.AddRange(repairOrderServices);
                await _context.SaveChangesAsync();

                // Seed Jobs
                var jobs = new List<Job>
        {
            // Order 1 - Completed jobs
            new Job
            {
                JobId = Guid.NewGuid(),
                ServiceId = basicOilChange.ServiceId,
                RepairOrderId = repairOrders[0].RepairOrderId,
                JobName = "Basic Oil Change Service",
                Status = JobStatus.Completed,
                Deadline = DateTime.UtcNow.AddDays(1),
                TotalAmount = basicOilChange.Price,
                Note = "Standard oil change completed successfully",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(1),
                Level = 1,
                AssignedAt = DateTime.UtcNow.AddDays(-5),
                AssignedByManagerId = userId
            },
            new Job
            {
                JobId = Guid.NewGuid(),
                ServiceId = tireRotation.ServiceId,
                RepairOrderId = repairOrders[0].RepairOrderId,
                JobName = "Tire Rotation Service",
                Status = JobStatus.Completed,
                Deadline = DateTime.UtcNow.AddDays(1),
                TotalAmount = tireRotation.Price,
                Note = "Tire rotation completed - even wear achieved",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(1),
                Level = 1,
                AssignedAt = DateTime.UtcNow.AddDays(-5),
                AssignedByManagerId = userId
            },

            // Order 2 - Pending approval jobs
            new Job
            {
                JobId = Guid.NewGuid(),
                ServiceId = brakePadReplacement.ServiceId,
                RepairOrderId = repairOrders[1].RepairOrderId,
                JobName = "Brake Pad Replacement",
                Status = JobStatus.Completed,
                Deadline = DateTime.UtcNow.AddDays(5),
                TotalAmount = brakePadReplacement.Price,
                Note = "Waiting for customer approval of brake repair",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                Level = 2,
                SentToCustomerAt = DateTime.UtcNow.AddDays(-1),
                AssignedByManagerId = userId
            },

            // Order 3 - In progress jobs
            new Job
            {
                JobId = Guid.NewGuid(),
                ServiceId = brakePadReplacement.ServiceId,
                RepairOrderId = repairOrders[2].RepairOrderId,
                JobName = "Emergency Brake Repair",
                Status = JobStatus.InProgress,
                Deadline = DateTime.UtcNow.AddDays(1),
                TotalAmount = brakePadReplacement.Price,
                Note = "Urgent brake system repair in progress",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Level = 3,
                AssignedAt = DateTime.UtcNow.AddDays(-1),
                AssignedByManagerId = userId
            },
            new Job
            {
                JobId = Guid.NewGuid(),
                ServiceId = engineTuneUp.ServiceId,
                RepairOrderId = repairOrders[2].RepairOrderId,
                JobName = "Engine Tune-Up",
                Status = JobStatus.InProgress,
                Deadline = DateTime.UtcNow.AddDays(2),
                TotalAmount = engineTuneUp.Price,
                Note = "Engine performance optimization",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Level = 2,
                AssignedAt = DateTime.UtcNow.AddDays(-1),
                AssignedByManagerId = userId
            },

            // Order 4 - New jobs
            new Job
            {
                JobId = Guid.NewGuid(),
                ServiceId = tireRotation.ServiceId,
                RepairOrderId = repairOrders[3].RepairOrderId,
                JobName = "Tire Rotation Service",
                Status = JobStatus.New,
                Deadline = DateTime.UtcNow.AddDays(3),
                TotalAmount = tireRotation.Price,
                Note = "Scheduled tire rotation service",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Level = 1,
                AssignedByManagerId = userId
            }
        };

                _context.Jobs.AddRange(jobs);
                await _context.SaveChangesAsync();



                // Seed Repairs - Thêm phần này
                var repairs = new List<Repair>
        {
            // Repair for Job 1 - Basic Oil Change (Completed)
            new Repair
            {
                RepairId = Guid.NewGuid(),
                JobId = jobs[0].JobId,
                Description = "Complete oil change service including oil filter replacement and lubrication check",
                StartTime = DateTime.UtcNow.AddDays(-5).AddHours(2),
                EndTime = DateTime.UtcNow.AddDays(-5).AddHours(3),
                ActualTime = TimeSpan.FromHours(1),
                EstimatedTime = TimeSpan.FromHours(1.5),
                Notes = "Oil changed successfully. Used synthetic oil for better performance. Checked for any leaks - none found."
            },
            // Repair for Job 2 - Tire Rotation (Completed)
            new Repair
            {
                RepairId = Guid.NewGuid(),
                JobId = jobs[1].JobId,
                Description = "Four-tire rotation with pressure check and visual inspection",
                StartTime = DateTime.UtcNow.AddDays(-5).AddHours(3),
                EndTime = DateTime.UtcNow.AddDays(-5).AddHours(4),
                ActualTime = TimeSpan.FromHours(1),
                EstimatedTime = TimeSpan.FromHours(1.2),
                Notes = "Tires rotated in X pattern. All tires show even wear. Tire pressures adjusted to manufacturer specifications."
            },
            // Repair for Job 3 - Brake Pad Replacement (Completed)
            new Repair
            {
                RepairId = Guid.NewGuid(),
                JobId = jobs[2].JobId,
                Description = "Front brake pad replacement with rotor inspection",
                StartTime = DateTime.UtcNow.AddDays(-2).AddHours(1),
                EndTime = DateTime.UtcNow.AddDays(-2).AddHours(3),
                ActualTime = TimeSpan.FromHours(2),
                EstimatedTime = TimeSpan.FromHours(2.5),
                Notes = "Replaced front brake pads. Rotors are in good condition, no need for resurfacing. Brake fluid level adequate."
            },
            // Repair for Job 4 - Emergency Brake Repair (In Progress)
            new Repair
            {
                RepairId = Guid.NewGuid(),
                JobId = jobs[3].JobId,
                Description = "Emergency brake system diagnosis and repair",
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
                EndTime = null, // Still in progress
                ActualTime = null, // Not completed yet
                EstimatedTime = TimeSpan.FromHours(3),
                Notes = "Diagnosed brake fluid leak from master cylinder. Parts ordered. Awaiting replacement parts for completion."
            },
            // Repair for Job 5 - Engine Tune-Up (In Progress)
            new Repair
            {
                RepairId = Guid.NewGuid(),
                JobId = jobs[4].JobId,
                Description = "Complete engine tune-up including spark plug replacement and ignition system check",
                StartTime = DateTime.UtcNow.AddDays(-1).AddHours(2),
                EndTime = null, // Still in progress
                ActualTime = null, // Not completed yet
                EstimatedTime = TimeSpan.FromHours(4),
                Notes = "Replaced spark plugs. Currently checking ignition coils and fuel injection system. Engine compression test pending."
            },
            // Repair for Job 6 - Tire Rotation (New - Not started)
            new Repair
            {
                RepairId = Guid.NewGuid(),
                JobId = jobs[5].JobId,
                Description = "Scheduled tire rotation service with wheel balancing",
                StartTime = null, // Not started yet
                EndTime = null, // Not started yet
                ActualTime = null, // Not started yet
                EstimatedTime = TimeSpan.FromHours(1.5),
                Notes = "Scheduled service. Will include tire pressure adjustment and visual inspection for uneven wear."
            }
        };

                _context.Repairs.AddRange(repairs);
                await _context.SaveChangesAsync();



                // Seed JobParts
                var jobParts = new List<JobPart>
        {
            // Job 1 - Oil change parts
            new JobPart
            {
                JobPartId = Guid.NewGuid(),
                JobId = jobs[0].JobId,
                PartId = oilFilterMedium.PartId,
                Quantity = 1,
                UnitPrice = oilFilterMedium.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new JobPart
            {
                JobPartId = Guid.NewGuid(),
                JobId = jobs[0].JobId,
                PartId = airFilterCheap.PartId,
                Quantity = 1,
                UnitPrice = airFilterCheap.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },

            // Job 3 - Brake repair parts
            new JobPart
            {
                JobPartId = Guid.NewGuid(),
                JobId = jobs[2].JobId,
                PartId = brakePadCheap.PartId,
                Quantity = 2,
                UnitPrice = brakePadCheap.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new JobPart
            {
                JobPartId = Guid.NewGuid(),
                JobId = jobs[2].JobId,
                PartId = brakeDiscMedium.PartId,
                Quantity = 2,
                UnitPrice = brakeDiscMedium.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },

            // Job 4 - Emergency brake parts
            new JobPart
            {
                JobPartId = Guid.NewGuid(),
                JobId = jobs[3].JobId,
                PartId = brakePadCheap.PartId,
                Quantity = 2,
                UnitPrice = brakePadCheap.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },

            // Job 5 - Engine tune-up parts
            new JobPart
            {
                JobPartId = Guid.NewGuid(),
                JobId = jobs[4].JobId,
                PartId = sparkPlugExpensive.PartId,
                Quantity = 4,
                UnitPrice = sparkPlugExpensive.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

                _context.JobParts.AddRange(jobParts);
                await _context.SaveChangesAsync();

                // Seed JobTechnicians
                var jobTechnicians = new List<JobTechnician>
        {
            // Assign technicians to jobs
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[0].JobId,
                TechnicianId = technicianIds[0],
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[1].JobId,
                TechnicianId = technicianIds[1],
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[2].JobId,
                TechnicianId = technicianIds[0],
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[3].JobId,
                TechnicianId = technicianIds[0],
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[3].JobId,
                TechnicianId = technicianIds[1],
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[4].JobId,
                TechnicianId = technicianIds[1],
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new JobTechnician
            {
                JobTechnicianId = Guid.NewGuid(),
                JobId = jobs[5].JobId,
                TechnicianId = technicianIds[0],
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.JobTechnicians.AddRange(jobTechnicians);
                await _context.SaveChangesAsync();

                // Seed Quotations
                var quotations = new List<Quotation>
        {
            new Quotation
            {
                QuotationId = Guid.NewGuid(),
                RepairOrderId = repairOrders[1].RepairOrderId,
                UserId = userId,
                VehicleId = vehicleId,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                SentToCustomerAt = DateTime.UtcNow.AddDays(-1),
                Status = QuotationStatus.Sent,
                TotalAmount = 2500000,
                DiscountAmount = 100000,
                Note = "Brake system repair quotation - Please review and approve the services",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            }
        };

                _context.Quotations.AddRange(quotations);
                await _context.SaveChangesAsync();

                // Seed QuotationServices
                var quotationServices = new List<QuotationService>
        {
            new QuotationService
            {
                QuotationServiceId = Guid.NewGuid(),
                QuotationId = quotations[0].QuotationId,
                ServiceId = brakePadReplacement.ServiceId,
                IsSelected = true,
                IsRequired = true,
                Price = brakePadReplacement.Price
            },
            new QuotationService
            {
                QuotationServiceId = Guid.NewGuid(),
                QuotationId = quotations[0].QuotationId,
                ServiceId = fullEngineDiagnostic.ServiceId,
                IsRequired = true,
                IsSelected = false,
                Price = fullEngineDiagnostic.Price
            }
        };

                _context.QuotationServices.AddRange(quotationServices);
                await _context.SaveChangesAsync();

                // Seed QuotationServiceParts
                var quotationServiceParts = new List<QuotationServicePart>
        {
            new QuotationServicePart
            {
                QuotationServicePartId = Guid.NewGuid(),
                QuotationServiceId = quotationServices[0].QuotationServiceId,
                PartId = brakePadCheap.PartId,
                IsSelected = true,
                
                Price = brakePadCheap.Price,
                Quantity = 2
            },
            new QuotationServicePart
            {
                QuotationServicePartId = Guid.NewGuid(),
                QuotationServiceId = quotationServices[0].QuotationServiceId,
                PartId = brakeDiscMedium.PartId,
                IsSelected = false,
                
                Price = brakeDiscMedium.Price,
                Quantity = 2
            },
            new QuotationServicePart
            {
                QuotationServicePartId = Guid.NewGuid(),
                QuotationServiceId = quotationServices[0].QuotationServiceId,
                PartId = shockAbsorberCheap.PartId,
                IsSelected = false,
                
                Price = shockAbsorberCheap.Price,
                Quantity = 1
            }
        };

                _context.QuotationServiceParts.AddRange(quotationServiceParts);
                await _context.SaveChangesAsync();

                Console.WriteLine("Repair Orders and related data seeded successfully!");
            }
        }

        private async Task SeedInspectionsAsync()
        {
            // Check if there are already inspections seeded
            if (!_context.Inspections.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Get the repair order with the specified ID
                    var repairOrderId = Guid.Parse("6c062742-e2d5-4f11-953f-eee173fcfa79");
                    var repairOrder = await _context.RepairOrders.FirstOrDefaultAsync(ro => ro.RepairOrderId == repairOrderId);
                    
                    // If the repair order doesn't exist, we'll create one
                    if (repairOrder == null)
                    {
                        // Get required entities for creating a repair order
                        var customerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000005"); // Default Customer
                        var vehicle = await _context.Vehicles.FirstOrDefaultAsync();
                        var branch = await _context.Branches.FirstOrDefaultAsync();
                        var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Pending");
                        
                        if (customerUser != null && vehicle != null && branch != null && pendingStatus != null)
                        {
                            // Create a RepairRequest first
                            var repairRequest = new RepairRequest
                            {
                                RepairRequestID = Guid.NewGuid(),
                                VehicleID = vehicle.VehicleId,
                                UserID = customerUser.Id,
                                Description = "Inspection repair order",
                                BranchId = branch.BranchId,
                                RequestDate = DateTime.UtcNow,
                                Status = RepairRequestStatus.Pending,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                EstimatedCost = 0,
                                ArrivalWindowStart = DateTimeOffset.UtcNow
                            };

                            _context.RepairRequests.Add(repairRequest);
                            await _context.SaveChangesAsync();

                            repairOrder = new RepairOrder
                            {
                                RepairOrderId = repairOrderId,
                                ReceiveDate = DateTime.UtcNow,
                                RoType = RoType.WalkIn,
                                EstimatedCompletionDate = DateTime.UtcNow.AddDays(3),
                                Cost = 0,
                                EstimatedAmount = 0,
                                PaidAmount = 0,
                                PaidStatus = PaidStatus.Unpaid,
                                EstimatedRepairTime = 2,
                                Note = "Inspection repair order",
                                BranchId = branch.BranchId,
                                StatusId = pendingStatus.OrderStatusId,
                                VehicleId = vehicle.VehicleId,
                                UserId = customerUser.Id,
                                RepairRequestId = repairRequest.RepairRequestID, // Use actual RepairRequest ID
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            
                            _context.RepairOrders.Add(repairOrder);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            throw new Exception("Required entities for creating repair order not found");
                        }
                    }

                    // Get two services and two parts for the inspection
                    var services = await _context.Services.Take(2).ToListAsync();
                    var parts = await _context.Parts.Take(2).ToListAsync();
                    
                    if (services.Count < 2 || parts.Count < 2)
                    {
                        throw new Exception("Not enough services or parts available for seeding");
                    }

                    // Create the inspection
                    var inspection = new Inspection
                    {
                        InspectionId = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        TechnicianId = null,
                        Status = InspectionStatus.New,
                        CustomerConcern = "Vehicle inspection for maintenance services",
                        Finding = "Initial inspection findings",
                        IssueRating = IssueRating.Good,
                        Note = "Initial inspection note",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Inspections.Add(inspection);
                    await _context.SaveChangesAsync();

                    // Create ServiceInspection entries (2 services)
                    var serviceInspections = new List<ServiceInspection>
                    {
                        new ServiceInspection
                        {
                            ServiceInspectionId = Guid.NewGuid(),
                            ServiceId = services[0].ServiceId,
                            InspectionId = inspection.InspectionId,
                            ConditionStatus = ConditionStatus.Good,
                            CreatedAt = DateTime.UtcNow
                        },
                        new ServiceInspection
                        {
                            ServiceInspectionId = Guid.NewGuid(),
                            ServiceId = services[1].ServiceId,
                            InspectionId = inspection.InspectionId,
                            ConditionStatus = ConditionStatus.Replace,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.ServiceInspections.AddRange(serviceInspections);
                    await _context.SaveChangesAsync();

                    // Create PartInspection entries (1 part for each service)
                    var partInspections = new List<PartInspection>
                    {
                        new PartInspection
                        {
                            PartInspectionId = Guid.NewGuid(),
                            PartId = parts[0].PartId,
                            InspectionId = inspection.InspectionId,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PartInspection
                        {
                            PartInspectionId = Guid.NewGuid(),
                            PartId = parts[1].PartId,
                            InspectionId = inspection.InspectionId,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    _context.PartInspections.AddRange(partInspections);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    Console.WriteLine("Inspection data seeded successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error seeding inspection data: {ex.Message}");
                    throw;
                }
            }
        }
    }

    }
