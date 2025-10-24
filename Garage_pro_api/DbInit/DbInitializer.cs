﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Campaigns;
using BusinessObject.Enums;
using BusinessObject.Roles;
using BusinessObject.Vehicles;
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
            await SeedOrderStatusesAsync();
            await SeedLabelsAsync();
            await SeedVehicleRelatedEntitiesAsync();
            await SeedVehiclesAsync();
            await SeedRepairOrdersAsync();

            await SeedPromotionalCampaignsWithServicesAsync();
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
                ("0900000013", "System", "Manager1", "Manager"),
                ("0900000014", "System", "Manager2", "Manager"),
                ("0900000005", "Default", "Customer", "Customer"),
                ("0900000006", "Default", "Technician", "Technician"),
                ("0900000007", "Default", "Technician1", "Technician"),
                ("0900000008", "Default", "Technician2", "Technician")
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
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Role Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Branch Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Service Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Promotional Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Part Management" },
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
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_SCHEDULE", Name = "Schedule Vehicle Service", Description = "Can schedule vehicle services", CategoryId = bookingCatId }
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
                                    "PROMO_VIEW", "PROMO_CREATE", "PROMO_UPDATE", "PROMO_DELETE", "PROMO_TOGGLE"
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
                   
                    CreatedAt = DateTime.UtcNow
                });

                _context.ServiceParts.Add(new ServicePart
                {
                    ServiceId = brakeRepair.ServiceId,
                    PartId = brakePad.PartId,
                   
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
                        OpenTime = new TimeSpan(8, 0, 0),   // 08:00
                        CloseTime = new TimeSpan(17, 0, 0)  // 17:00
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
                Type = CampaignType.Loyalty,
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
                Type = CampaignType.Seasonal,
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
                // Get required entities
                var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(os => os.StatusName == "Pending");
                var inProgressStatus = await _context.OrderStatuses.FirstOrDefaultAsync(os => os.StatusName == "In Progress");
                var completedStatus = await _context.OrderStatuses.FirstOrDefaultAsync(os => os.StatusName == "Completed");
                
                var branch = await _context.Branches.FirstOrDefaultAsync();
                var customerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000005"); // Default Customer
                var managerUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == "0900000002"); // System Manager
                var vehicles = await _context.Vehicles.ToListAsync();
                
                if (pendingStatus != null && inProgressStatus != null && completedStatus != null &&
                    branch != null && customerUser != null && vehicles.Any())
                {
                    var repairOrders = new List<RepairOrder>
                    {
                        new RepairOrder
                        {
                            ReceiveDate = DateTime.UtcNow.AddDays(-5),
                            EstimatedCompletionDate = DateTime.UtcNow.AddDays(2),
                            Cost = 1500000,
                            EstimatedAmount = 2000000,
                            PaidAmount = 0,
                            PaidStatus = "Unpaid",
                            EstimatedRepairTime = 120,
                            Note = "Regular maintenance service",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            BranchId = branch.BranchId,
                            StatusId = pendingStatus.OrderStatusId,
                            VehicleId = vehicles[0].VehicleId,
                            UserId = customerUser.Id,
                            RepairRequestId = Guid.NewGuid(),
                            IsArchived = false,
                            ArchivedByUserId = null
                        },
                        new RepairOrder
                        {
                            ReceiveDate = DateTime.UtcNow.AddDays(-3),
                            EstimatedCompletionDate = DateTime.UtcNow.AddDays(1),
                            Cost = 3000000,
                            EstimatedAmount = 3500000,
                            PaidAmount = 1000000,
                            PaidStatus = "Partial",
                            EstimatedRepairTime = 180,
                            Note = "Brake system repair",
                            CreatedAt = DateTime.UtcNow.AddDays(-3),
                            BranchId = branch.BranchId,
                            StatusId = inProgressStatus.OrderStatusId,
                            VehicleId = vehicles.Count > 1 ? vehicles[1].VehicleId : vehicles[0].VehicleId,
                            UserId = customerUser.Id,
                            RepairRequestId = Guid.NewGuid(),
                            IsArchived = false,
                            ArchivedByUserId = null
                        },
                        new RepairOrder
                        {
                            ReceiveDate = DateTime.UtcNow.AddDays(-10),
                            EstimatedCompletionDate = DateTime.UtcNow.AddDays(-2),
                            CompletionDate = DateTime.UtcNow.AddDays(-1),
                            Cost = 5000000,
                            EstimatedAmount = 5000000,
                            PaidAmount = 5000000,
                            PaidStatus = "Paid",
                            EstimatedRepairTime = 240,
                            Note = "Complete vehicle overhaul",
                            CreatedAt = DateTime.UtcNow.AddDays(-10),
                            BranchId = branch.BranchId,
                            StatusId = completedStatus.OrderStatusId,
                            VehicleId = vehicles.Count > 2 ? vehicles[2].VehicleId : vehicles[0].VehicleId,
                            UserId = customerUser.Id,
                            RepairRequestId = Guid.NewGuid(),
                            IsArchived = false,
                            ArchivedByUserId = null
                        }
                    };

                    _context.RepairOrders.AddRange(repairOrders);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}