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
using BusinessObject.Customers;
using BusinessObject.RequestEmergency;

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
            await SeedInspectionTypesAsync();
            await SeedPartCategoriesAsync();
            await SeedPartsAsync();
            await SeedServiceCategoriesAsync();
            await SeedServicesAsync();
            await SeedServicePartCategoriesAsync();
            await SeedBranchesAsync();
            await SeedOrderStatusesAsync();
            await SeedLabelsAsync();
            await SeedVehicleRelatedEntitiesAsync();
            await SeedVehiclesAsync();
            await SeedVehicleModelColorsAsync();
            await SeedPriceEmergenciesAsync();

            await SeedPromotionalCampaignsWithServicesAsync();
            //await SeedManyCustomersAndRepairOrdersAsync(customerCount: 15, totalOrdersTarget: 800);

            //await SeedRepairOrdersAsync();
            // await SeedInspectionsAsync();

            // Seed Vehicle Specifications
            var vehicleSpecsSeeder = new VehicleSpecificationsSeeder(_context);
            await vehicleSpecsSeeder.SeedAllSpecificationsDataAsync();
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
                ("0900000003", "System", "Manager 1", "Manager"),
                ("0900000004", "System", "Manager 2", "Manager"),
                ("0900000005", "Default", "Customer 1", "Customer"),
                ("0900000006", "Default", "Technician 1", "Technician"),
                ("0900000007", "Default", "Technician 2", "Technician"),
                ("0900000008", "Default", "Technician 3", "Technician"),
                ("0900000009", "Default", "Customer 3", "Customer"),
                ("0900000010", "Default", "Customer 4", "Customer"),

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
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Basic Permission" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Role Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Branch Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Service Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Promotional Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Part Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Booking Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Log Monitoring" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Policy Security" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Statistic Monitoring" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Job Repair" },

        new PermissionCategory { Id = Guid.NewGuid(), Name = "Inspection Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Quotation Management" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Inspection Technician" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Job Technician" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Notification" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Repair History" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Repair" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Specification" },
        new PermissionCategory { Id = Guid.NewGuid(), Name = "Statistical" }


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
            var basicCatId = categories.First(c => c.Name == "Basic Permission").Id;
            var roleCatId = categories.First(c => c.Name == "Role Management").Id;
            var branchCatId = categories.First(c => c.Name == "Branch Management").Id;
            var serviceCatId = categories.First(c => c.Name == "Service Management").Id;
            var promotionalCatId = categories.First(c => c.Name == "Promotional Management").Id;
            var partCatId = categories.First(c => c.Name == "Part Management").Id;
            var bookingCatId = categories.First(c => c.Name == "Booking Management").Id;
            var logCatId = categories.First(c => c.Name == "Log Monitoring").Id;
            var policyCatId = categories.First(c => c.Name == "Policy Security").Id;
            var statCatId = categories.First(c => c.Name == "Statistic Monitoring").Id;
            var jobCatId = categories.First(c => c.Name == "Job Repair").Id;
            var inspectionMgmtId = categories.First(c => c.Name == "Inspection Management").Id;
            var quotationMgmtId = categories.First(c => c.Name == "Quotation Management").Id;

            var inspectionTechnicianId = categories.First(c => c.Name == "Inspection Technician").Id;
            var jobTechnicianId = categories.First(c => c.Name == "Job Technician").Id;
            var notificationId = categories.First(c => c.Name == "Notification").Id;
            var repairHistoryId = categories.First(c => c.Name == "Repair History").Id;
            var repairId = categories.First(c => c.Name == "Repair").Id;
            var specificationId = categories.First(c => c.Name == "Specification").Id;
            var statisticalId = categories.First(c => c.Name == "Statistical").Id;

            var defaultPermissions = new List<Permission>
                {
                    // User Management
                    new Permission { Id = Guid.NewGuid(), Code = "USER_VIEW", Name = "View Users", Description = "Can view user list", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "USER_EDIT", Name = "Edit Users", Description = "Can edit user info", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "USER_DELETE", Name = "Delete Users", Description = "Can delete users", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "TECHNICIAN_VIEW", Name = "View Technicians", Description = "Can view technician list by branch", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "TECHNICIAN_ASSIGN", Name = "Assign Technicians", Description = "Can assign technicians to inspections/jobs", CategoryId = userCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "TECHNICIAN_SCHEDULE", Name = "View Technician Schedule", Description = "Can view technician schedules and availability", CategoryId = userCatId },

                    // Role Management
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_CREATE", Name = "Create Role", Description = "Can create roles", CategoryId = roleCatId },

                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_UPDATE", Name = "Update Role", Description = "Can update roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_DELETE", Name = "Delete Role", Description = "Can delete roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "ROLE_VIEW", Name = "View Roles", Description = "Can view roles", CategoryId = roleCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PERMISSION_ASSIGN", Name = "Assign Permissions", Description = "Can assign permissions to roles", CategoryId = roleCatId },
                     // ✅ Statistic Monitoring
                     new Permission { Id = Guid.NewGuid(), Code = "VIEW_STAT", Name = "View Statistic", Description = "Can view stats in the system", CategoryId = statCatId },


                    // ✅ Basic permission
                    new Permission { Id = Guid.NewGuid(), Code = "BASIC_ACCESS", Name = "Basic Access", Description = "Can do action as a customer role", CategoryId = basicCatId },

                    // ✅ Branch Management
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_VIEW", Name = "View Branches", Description = "Can view branch list", CategoryId = branchCatId ,IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_CREATE", Name = "Create Branch", Description = "Can create branches", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_UPDATE", Name = "Update Branch", Description = "Can update branch info", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_DELETE", Name = "Delete Branch", Description = "Can delete branches", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_STATUS_TOGGLE", Name = "Toggle Branch Status", Description = "Can activate/deactivate branches", CategoryId = branchCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "BRANCH_IMPORT_EXCEL", Name = "Import Branches From Excel", Description = "Can import branch data via Excel files", CategoryId = branchCatId },

                    // ✅ Service Management
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_VIEW", Name = "View Services", Description = "Can view services", CategoryId = serviceCatId,IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_CREATE", Name = "Create Service", Description = "Can create new services", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_UPDATE", Name = "Update Service", Description = "Can update service information", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_DELETE", Name = "Delete Service", Description = "Can delete services", CategoryId = serviceCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "SERVICE_STATUS_TOGGLE", Name = "Toggle Service Status", Description = "Can activate/deactivate services", CategoryId = serviceCatId },

                    // ✅ Promotional Management
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_VIEW", Name = "View Promotions", Description = "Can view promotional campaigns", CategoryId = promotionalCatId,IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_CREATE", Name = "Create Promotion", Description = "Can create promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_UPDATE", Name = "Update Promotion", Description = "Can update promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_DELETE", Name = "Delete Promotion", Description = "Can delete promotional campaigns", CategoryId = promotionalCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PROMO_TOGGLE", Name = "Toggle Promotion Status", Description = "Can activate/deactivate promotions", CategoryId = promotionalCatId },

                    // ✅ Part Management
                    new Permission { Id = Guid.NewGuid(), Code = "PART_VIEW", Name = "View Parts", Description = "Can view parts", CategoryId = partCatId,IsDefault=true },

                    new Permission { Id = Guid.NewGuid(), Code = "PART_CREATE", Name = "Create Part", Description = "Can create new parts", CategoryId = partCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PART_UPDATE", Name = "Update Part", Description = "Can update part information", CategoryId = partCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "PART_DELETE", Name = "Delete Part", Description = "Can delete parts", CategoryId = partCatId },

                    //Job
                     new Permission { Id = Guid.NewGuid(), Code = "JOB_VIEW", Name = "View Job", Description = "Can view jobs", CategoryId =  jobCatId},
                     new Permission { Id = Guid.NewGuid(), Code = "JOB_MANAGE", Name = "Manage Job", Description = "Can manage jobs", CategoryId =  jobCatId},

                    
                    // ✅ Booking Management (Inspections & Jobs)
                    new Permission { Id = Guid.NewGuid(), Code = "BOOKING_VIEW", Name = "View Bookings", Description = "Can view inspections and jobs", CategoryId = bookingCatId, IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "BOOKING_MANAGE", Name = "Manage Bookings", Description = "Can create, update, and manage inspections and jobs", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "REPAIR_REQUEST_VIEW", Name = "View Repair Requests", Description = "Can view customer repair requests", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "REPAIR_REQUEST_CANCEL", Name = "Cancel Repair Requests", Description = "Can cancel repair requests on behalf of customers", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "EMERGENCY_REQUEST_VIEW", Name = "View Emergency Requests", Description = "Can view emergency repair requests", CategoryId = bookingCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "EMERGENCY_REQUEST_MANAGE", Name = "Manage Emergency Requests", Description = "Can approve/reject emergency requests", CategoryId = bookingCatId },

                    // ✅ Inspection Management (Manager)
                    new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_VIEW", Name = "View Inspections", Description = "Can view all inspections", CategoryId = inspectionMgmtId, IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_CREATE", Name = "Create Inspection", Description = "Can create new inspections", CategoryId = inspectionMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_UPDATE", Name = "Update Inspection", Description = "Can update inspection details", CategoryId = inspectionMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_DELETE", Name = "Delete Inspection", Description = "Can delete inspections", CategoryId = inspectionMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_ASSIGN", Name = "Assign Inspection", Description = "Can assign inspections to technicians", CategoryId = inspectionMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_CONVERT", Name = "Convert to Quotation", Description = "Can convert completed inspections to quotations", CategoryId = inspectionMgmtId },

                    // ✅ Quotation Management (Manager)
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_VIEW", Name = "View Quotations", Description = "Can view all quotations", CategoryId = quotationMgmtId, IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_CREATE", Name = "Create Quotation", Description = "Can create new quotations", CategoryId = quotationMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_UPDATE", Name = "Update Quotation", Description = "Can update quotation details", CategoryId = quotationMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_DELETE", Name = "Delete Quotation", Description = "Can delete quotations", CategoryId = quotationMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_SEND", Name = "Send Quotation", Description = "Can send quotations to customers", CategoryId = quotationMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_APPROVE", Name = "Approve Quotation", Description = "Can approve/reject quotations", CategoryId = quotationMgmtId },
                    new Permission { Id = Guid.NewGuid(), Code = "QUOTATION_COPY_TO_JOBS", Name = "Copy to Jobs", Description = "Can copy approved quotations to jobs", CategoryId = quotationMgmtId },

                     //Technician
                     //Inspections Technician
                     new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_TECHNICIAN_VIEW", Name = "View Inspection Technician", Description = "Can view assigned inspection and all servivice", CategoryId = inspectionTechnicianId, IsDefault= true ,IsSystem = true },
                     new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_TECHNICIAN_UPDATE", Name = "Update Inspection Technician", Description = "Can update assigned inspection", CategoryId = inspectionTechnicianId , IsSystem = true},
                     new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_TECHNICIAN_DELETE", Name = "Delete Inspection Technician", Description = "Can delete service or part to assigned inspection", CategoryId = inspectionTechnicianId ,IsSystem = true},
                     new Permission { Id = Guid.NewGuid(), Code = "INSPECTION_ADD_SERVICE", Name = "Add Service Inspection ", Description = "Can add service to assigned inspection", CategoryId = inspectionTechnicianId,IsSystem = true },
                    // Job Technician 
                     new Permission { Id = Guid.NewGuid(), Code = "JOB_TECHNICIAN_VIEW", Name = "View Job Technician", Description = "Can view assigned job", CategoryId = jobTechnicianId , IsDefault= true },
                     new Permission { Id = Guid.NewGuid(), Code = "JOB_TECHNICIAN_UPDATE", Name = "Update Job Technician", Description = "Can update status assigned job", CategoryId = jobTechnicianId },
                    // Notification
                     new Permission { Id = Guid.NewGuid(), Code = "NOTIFICATION_VIEW", Name = "View Notifications", Description = "Can view notifications", CategoryId = notificationId , IsDefault = true},
                     new Permission { Id = Guid.NewGuid(), Code = "NOTIFICATION_MARK", Name = "Mark Notifications", Description = "Can mark notifications", CategoryId = notificationId },
                     new Permission { Id = Guid.NewGuid(), Code = "NOTIFICATION_DELETE", Name = "Delete Notifications", Description = "Can delete notifications", CategoryId = notificationId },
                     // Repair History
                     new Permission { Id = Guid.NewGuid(), Code = "REPAIR_HISTORY_VIEW", Name = "View Repair History", Description = "Can view repair history", CategoryId = repairHistoryId , IsDefault= true },
                     // Repair
                     new Permission { Id = Guid.NewGuid(), Code = "REPAIR_VIEW", Name = "View Repair", Description = "Can view repair", CategoryId = repairId, IsDefault= true, IsSystem = true },
                     new Permission { Id = Guid.NewGuid(), Code = "REPAIR_CREATE", Name = "Create Repair", Description = "Can create repair", CategoryId = repairId ,IsSystem = true },
                     new Permission { Id = Guid.NewGuid(), Code = "REPAIR_UPDATE", Name = "Update Repair", Description = "Can update repair", CategoryId = repairId ,IsSystem = true},
                     // Specification
                     new Permission { Id = Guid.NewGuid(), Code = "SPECIFICATION_MANAGE", Name = "Manage Specification", Description = "Can view  and search specification of vehicle",IsSystem = true, CategoryId = specificationId },
                     // Statistical
                     new Permission { Id = Guid.NewGuid(), Code = "STATISTICAL_VIEW", Name = "View Statistical", Description = "Can view Statistical page", CategoryId = statisticalId , IsDefault = true, IsSystem = true},
            
                    // ✅ Vehicle Management
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_VIEW", Name = "View Vehicles", Description = "Can view vehicles", CategoryId = basicCatId,IsDefault=true },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_CREATE", Name = "Create Vehicle", Description = "Can create new vehicles", CategoryId = basicCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_UPDATE", Name = "Update Vehicle", Description = "Can update vehicle information", CategoryId = basicCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_DELETE", Name = "Delete Vehicle", Description = "Can delete vehicles", CategoryId = basicCatId },
                    new Permission { Id = Guid.NewGuid(), Code = "VEHICLE_SCHEDULE", Name = "Schedule Vehicle Service", Description = "Can schedule vehicle services", CategoryId = basicCatId },

                     // ✅ Log View

                     new Permission { Id = Guid.NewGuid(), Code = "LOG_VIEW", Name = "View Logs", Description = "Can view Logs page", CategoryId = logCatId,IsDefault=true },

                     // ✅ JobRepair
                     new Permission { Id = Guid.NewGuid(), Code = "JOB_UPDATE", Name = "Job Update", Description = "Can view Logs page", CategoryId = jobCatId },
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
                                    //Stat
                                    "VIEW_STAT",
                                    // Role
                                    "ROLE_VIEW", "ROLE_CREATE", "ROLE_UPDATE", "ROLE_DELETE", "PERMISSION_ASSIGN",
            
                                    // ✅ Branch Management
                                    "BRANCH_VIEW", "BRANCH_CREATE", "BRANCH_UPDATE", "BRANCH_DELETE", "BRANCH_STATUS_TOGGLE","BRANCH_IMPORT_EXCEL",
            
                                    // ✅ Service Management
                                    "SERVICE_VIEW", "SERVICE_CREATE", "SERVICE_UPDATE", "SERVICE_DELETE", "SERVICE_STATUS_TOGGLE",
            
                                    // ✅ Promotional Management
                                    "PROMO_VIEW", "PROMO_CREATE", "PROMO_UPDATE", "PROMO_DELETE", "PROMO_TOGGLE",
                                    // LOG MONITORING
                                    "LOG_VIEW" ,                                    
                                    // Security Policy
                                    "POLICY_MANAGEMENT"
                                }
                            },
                            {
                                "Manager", new[]
                                {

                                    "NOTIFICATION_VIEW", "NOTIFICATION_MARK", "NOTIFICATION_DELETE",
                                    //"USER_VIEW", "JOB_MANAGE", "JOB_VIEW",
                                    //"BRANCH_VIEW", "SERVICE_VIEW", "PROMO_VIEW",
                                    //"VEHICLE_VIEW", "VEHICLE_CREATE", "VEHICLE_UPDATE", "VEHICLE_SCHEDULE"
                                    "JOB_MANAGE", "JOB_VIEW",
                                    // User Management
                                    "USER_VIEW",
                                    // Technician Management
                                    "TECHNICIAN_VIEW", "TECHNICIAN_ASSIGN", "TECHNICIAN_SCHEDULE",
                                    // Branch Management
                                    "BRANCH_VIEW",
                                    // Service Management
                                    "SERVICE_VIEW",
                                    // Promotional Management
                                    "PROMO_VIEW",
                                    // Part Management
                                    "PART_VIEW",
                                    // Booking Management (Inspections & Jobs)
                                    "BOOKING_VIEW", "BOOKING_MANAGE",
                                    // Inspection Management
                                    "INSPECTION_VIEW", "INSPECTION_CREATE", "INSPECTION_UPDATE", "INSPECTION_DELETE", "INSPECTION_ASSIGN", "INSPECTION_CONVERT",
                                    // Quotation Management
                                    "QUOTATION_VIEW", "QUOTATION_CREATE", "QUOTATION_UPDATE", "QUOTATION_DELETE", "QUOTATION_SEND", "QUOTATION_APPROVE", "QUOTATION_COPY_TO_JOBS",
                                    // Repair Request Management
                                    "REPAIR_REQUEST_VIEW", "REPAIR_REQUEST_CANCEL",
                                    // Emergency Request Management
                                    "EMERGENCY_REQUEST_VIEW", "EMERGENCY_REQUEST_MANAGE",
                                    // Vehicle Management
                                    "VEHICLE_VIEW", "VEHICLE_CREATE", "VEHICLE_UPDATE", "VEHICLE_DELETE", "VEHICLE_SCHEDULE",
                                    // Repair Management
                                    "REPAIR_VIEW", "REPAIR_CREATE", "REPAIR_UPDATE", "REPAIR_HISTORY_VIEW"

                                }
                            },
                            {
                                "Customer", new[] { "BASIC_ACCESS" }
                            },
                            {
                                "Technician", new[] 
                                { 
                                    "BOOKING_MANAGE",

                                    "INSPECTION_TECHNICIAN_VIEW", 
                                    "INSPECTION_TECHNICIAN_UPDATE", 
                                    "INSPECTION_TECHNICIAN_DELETE",
                                    "INSPECTION_ADD_SERVICE",

                                    "JOB_TECHNICIAN_VIEW", 
                                    "JOB_TECHNICIAN_UPDATE",


                                    "NOTIFICATION_VIEW", "NOTIFICATION_MARK", "NOTIFICATION_DELETE",
                                    "REPAIR_HISTORY_VIEW",
                                    
                                    "REPAIR_UPDATE",
                                    "REPAIR_CREATE",
                                    "REPAIR_VIEW",

                                    "SPECIFICATION_MANAGE",

                                    "STATISTICAL_VIEW"

                                }

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
            // Nếu bạn muốn seed thêm mà không bị skip khi đã có vài cái:
            // => không dùng Any() return; mà dùng Ensure theo từng category.
            async Task EnsureAsync(string name, string desc)
            {
                var exists = await _context.PartCategories.AnyAsync(x => x.CategoryName == name);
                if (exists) return;

                _context.PartCategories.Add(new PartCategory
                {
                    CategoryName = name,
                    Description = desc
                });
            }

            // ===== BRAKES =====
            await EnsureAsync("Front - Brake Pads", "Front brake pads only");
            await EnsureAsync("Rear - Brake Pads", "Rear brake pads only");
            await EnsureAsync("Front - Brake Discs", "Front brake discs/rotors only");
            await EnsureAsync("Rear - Brake Discs", "Rear brake discs/rotors only");
            await EnsureAsync("Front - Brake Calipers", "Front brake calipers only");
            await EnsureAsync("Rear - Brake Calipers", "Rear brake calipers only");
            await EnsureAsync("Brake - Fluid", "Brake fluid only");
            await EnsureAsync("Brake - Hoses & Lines", "Brake hoses/lines only");
            await EnsureAsync("Brake - Master Cylinder", "Master cylinder only");

            // ===== TIRES / WHEELS =====
            await EnsureAsync("Front - Tires", "Front tires only");
            await EnsureAsync("Rear - Tires", "Rear tires only");
            await EnsureAsync("Front - Wheel Bearings", "Front wheel bearings only");
            await EnsureAsync("Rear - Wheel Bearings", "Rear wheel bearings only");
            await EnsureAsync("Wheel - Balancing Weights", "Balancing weights only");
            await EnsureAsync("Wheel - Valve & TPMS", "Valve stems / TPMS sensors only");

            // ===== SUSPENSION =====
            await EnsureAsync("Front - Shock/Strut", "Front shocks/struts only");
            await EnsureAsync("Rear - Shock", "Rear shocks only");
            await EnsureAsync("Front - Control Arm", "Front control arms only");
            await EnsureAsync("Rear - Control Arm", "Rear control arms only");
            await EnsureAsync("Front - Ball Joint", "Front ball joints only");
            await EnsureAsync("Bushings - Suspension", "Suspension bushings only");
            await EnsureAsync("Stabilizer Link - Front", "Front stabilizer links only");
            await EnsureAsync("Stabilizer Link - Rear", "Rear stabilizer links only");

            // ===== STEERING =====
            await EnsureAsync("Steering - Tie Rod End", "Tie rod ends only");
            await EnsureAsync("Steering - Rack", "Steering rack only");
            await EnsureAsync("Steering - Pump", "Power steering pump only");

            // ===== ENGINE (common replacement) =====
            await EnsureAsync("Engine - Oil", "Engine oil only");
            await EnsureAsync("Engine - Oil Filter", "Oil filter only");
            await EnsureAsync("Engine - Air Filter", "Engine air filter only");
            await EnsureAsync("Engine - Spark Plugs", "Spark plugs only");
            await EnsureAsync("Engine - Ignition Coils", "Ignition coils only");
            await EnsureAsync("Engine - Drive Belt", "Serpentine/drive belt only");
            await EnsureAsync("Engine - Mounts", "Engine mounts only");
            await EnsureAsync("Engine - Gaskets & Seals", "Common engine gaskets/seals (valve cover, etc.)");

            // ===== INTAKE / FUEL =====
            await EnsureAsync("Intake - Throttle Body", "Throttle body / gasket only");
            await EnsureAsync("Fuel - Fuel Pump", "Fuel pump only");
            await EnsureAsync("Fuel - Fuel Filter", "Fuel filter only");
            await EnsureAsync("Fuel - Injectors", "Fuel injectors only");

            // ===== COOLING =====
            await EnsureAsync("Cooling - Radiator", "Radiator only");
            await EnsureAsync("Cooling - Water Pump", "Water pump only");
            await EnsureAsync("Cooling - Thermostat", "Thermostat only");
            await EnsureAsync("Cooling - Hoses", "Cooling hoses only");
            await EnsureAsync("Cooling - Coolant", "Coolant only");

            // ===== ELECTRICAL (charge/start) =====
            await EnsureAsync("Electrical - Battery", "Battery only");
            await EnsureAsync("Electrical - Alternator", "Alternator only");
            await EnsureAsync("Electrical - Starter Motor", "Starter motor only");
            await EnsureAsync("Electrical - Fuses/Relays", "Fuses and relays only");

            // ===== SENSORS (hay lỗi) =====
            await EnsureAsync("Sensors - O2", "Oxygen sensors only");
            await EnsureAsync("Sensors - MAF/MAP", "MAF/MAP sensors only");
            await EnsureAsync("Sensors - ABS Front", "Front ABS wheel speed sensors only");
            await EnsureAsync("Sensors - ABS Rear", "Rear ABS wheel speed sensors only");

            // ===== TRANSMISSION =====
            await EnsureAsync("Transmission - ATF", "Automatic transmission fluid only");
            await EnsureAsync("Transmission - Clutch Kit", "Clutch kit only (manual)");
            await EnsureAsync("Transmission - Mount", "Transmission mount only");

            // ===== HVAC =====
            await EnsureAsync("HVAC - Cabin Filter", "Cabin air filter only");
            await EnsureAsync("HVAC - Refrigerant", "Refrigerant gas only");
            await EnsureAsync("HVAC - AC Compressor", "AC compressor only");
            await EnsureAsync("HVAC - Blower Motor", "Blower motor only");

            // ===== EXHAUST =====
            await EnsureAsync("Exhaust - Muffler", "Muffler only");
            await EnsureAsync("Exhaust - Catalytic Converter", "Catalytic converter only");
            await EnsureAsync("Exhaust - Gaskets/Hangers", "Exhaust gaskets/hangers only");

            await _context.SaveChangesAsync();
        }





        private async Task SeedPartsAsync()
        {
            if (_context.Parts.Any()) return;

            // helper: lấy PartCategoryId (bạn đang dùng LaborCategoryId)
            async Task<Guid> PC(string categoryName)
            {
                var pc = await _context.PartCategories.FirstAsync(x => x.CategoryName == categoryName);
                return pc.LaborCategoryId; // đổi nếu PK của bạn là PartCategoryId
            }

            void Add(List<Part> parts, Guid pcId, string name, int price, int stock)
            {
                parts.Add(new Part
                {
                    Name = name,
                    PartCategoryId = pcId,
                    Price = price,
                    Stock = stock,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var parts = new List<Part>();

            // ===== BRAKE PADS FRONT =====
            {
                var id = await PC("Front - Brake Pads");
                Add(parts, id, "Front Brake Pads - Budget (Generic)", 700, 60);
                Add(parts, id, "Front Brake Pads - Standard (Semi-metallic)", 1100, 50);
                Add(parts, id, "Front Brake Pads - Premium (Ceramic)", 1600, 40);

                Add(parts, id, "Front Brake Pads - Toyota Vios 2014-2018 (Standard)", 1250, 25);
                Add(parts, id, "Front Brake Pads - Toyota Altis 2015-2019 (Premium)", 1750, 20);
                Add(parts, id, "Front Brake Pads - Honda City 2014-2019 (Standard)", 1350, 25);
                Add(parts, id, "Front Brake Pads - Honda Civic 2016-2020 (Premium)", 2200, 15);
            }

            // ===== BRAKE PADS REAR =====
            {
                var id = await PC("Rear - Brake Pads");
                Add(parts, id, "Rear Brake Pads - Budget (Generic)", 700, 60);
                Add(parts, id, "Rear Brake Pads - Standard (Semi-metallic)", 1100, 50);
                Add(parts, id, "Rear Brake Pads - Premium (Ceramic)", 1600, 40);

                Add(parts, id, "Rear Brake Pads - Toyota Vios 2014-2018 (Standard)", 1200, 25);
                Add(parts, id, "Rear Brake Pads - Honda Civic 2016-2020 (Premium)", 2100, 15);
            }

            // ===== BRAKE DISCS FRONT/REAR =====
            {
                var id = await PC("Front - Brake Discs");
                Add(parts, id, "Front Brake Disc Pair - Budget (Generic)", 1200, 30);
                Add(parts, id, "Front Brake Disc Pair - Standard (Vented)", 1800, 25);
                Add(parts, id, "Front Brake Disc Pair - Premium (High Carbon)", 2600, 15);
                Add(parts, id, "Front Brake Disc Pair - Honda Civic 2016-2020", 2900, 10);
            }
            {
                var id = await PC("Rear - Brake Discs");
                Add(parts, id, "Rear Brake Disc Pair - Budget (Generic)", 1200, 30);
                Add(parts, id, "Rear Brake Disc Pair - Standard", 1800, 25);
                Add(parts, id, "Rear Brake Disc Pair - Premium", 2600, 15);
                Add(parts, id, "Rear Brake Disc Pair - Toyota Altis 2015-2019", 2400, 12);
            }

            // ===== BRAKE CALIPERS FRONT/REAR =====
            {
                var id = await PC("Front - Brake Calipers");
                Add(parts, id, "Front Brake Caliper - Reman (Budget)", 1400, 15);
                Add(parts, id, "Front Brake Caliper - Standard", 2200, 12);
                Add(parts, id, "Front Brake Caliper - OEM Grade (Premium)", 3200, 8);
            }
            {
                var id = await PC("Rear - Brake Calipers");
                Add(parts, id, "Rear Brake Caliper - Reman (Budget)", 1400, 15);
                Add(parts, id, "Rear Brake Caliper - Standard", 2200, 12);
                Add(parts, id, "Rear Brake Caliper - OEM Grade (Premium)", 3200, 8);
            }

            // ===== BRAKE FLUID / LINES / MASTER =====
            {
                var id = await PC("Brake - Fluid");
                Add(parts, id, "Brake Fluid DOT4 1L - Budget", 200, 80);
                Add(parts, id, "Brake Fluid DOT4 1L - Standard", 320, 80);
                Add(parts, id, "Brake Fluid DOT4 LV 1L - Premium", 450, 60);
            }
            {
                var id = await PC("Brake - Hoses & Lines");
                Add(parts, id, "Brake Hose Set - Budget", 350, 30);
                Add(parts, id, "Brake Hose Set - Standard", 550, 25);
                Add(parts, id, "Brake Hose Set - Premium (Braided)", 900, 15);
            }
            {
                var id = await PC("Brake - Master Cylinder");
                Add(parts, id, "Master Cylinder - Budget", 1200, 10);
                Add(parts, id, "Master Cylinder - Standard", 1800, 8);
                Add(parts, id, "Master Cylinder - Premium", 2600, 6);
            }

            // ===== TIRES FRONT/REAR =====
            {
                var id = await PC("Front - Tires");
                Add(parts, id, "Front Tire Pair 195/65R15 - Budget", 2200, 25);
                Add(parts, id, "Front Tire Pair 195/65R15 - Standard Touring", 3200, 20);
                Add(parts, id, "Front Tire Pair 195/65R15 - Premium Performance", 4500, 12);

                Add(parts, id, "Front Tire Pair 185/60R15 - Budget", 2100, 20);
                Add(parts, id, "Front Tire Pair 205/55R16 - Standard", 3600, 15);
            }
            {
                var id = await PC("Rear - Tires");
                Add(parts, id, "Rear Tire Pair 195/65R15 - Budget", 2200, 25);
                Add(parts, id, "Rear Tire Pair 195/65R15 - Standard Touring", 3200, 20);
                Add(parts, id, "Rear Tire Pair 195/65R15 - Premium Performance", 4500, 12);

                Add(parts, id, "Rear Tire Pair 205/55R16 - Standard", 3600, 15);
            }

            // ===== WHEEL BEARING FRONT/REAR =====
            {
                var id = await PC("Front - Wheel Bearings");
                Add(parts, id, "Front Wheel Bearing - Budget", 600, 25);
                Add(parts, id, "Front Wheel Bearing - Standard", 900, 20);
                Add(parts, id, "Front Wheel Bearing - Premium", 1300, 15);
            }
            {
                var id = await PC("Rear - Wheel Bearings");
                Add(parts, id, "Rear Wheel Bearing - Budget", 600, 25);
                Add(parts, id, "Rear Wheel Bearing - Standard", 900, 20);
                Add(parts, id, "Rear Wheel Bearing - Premium", 1300, 15);
            }

            // ===== TPMS / VALVE =====
            {
                var id = await PC("Wheel - Valve & TPMS");
                Add(parts, id, "Valve Stem Set - Budget", 80, 100);
                Add(parts, id, "Valve Stem Set - Standard", 120, 100);
                Add(parts, id, "TPMS Sensor - Standard", 650, 25);
                Add(parts, id, "TPMS Sensor - Premium (OE)", 950, 15);
            }

            // ===== BALANCING WEIGHTS =====
            {
                var id = await PC("Wheel - Balancing Weights");
                Add(parts, id, "Balancing Weights Set - Budget", 100, 200);
                Add(parts, id, "Balancing Weights Set - Standard", 160, 200);
                Add(parts, id, "Balancing Weights Set - Premium", 250, 150);
            }

            // ===== SUSPENSION: shocks/control arms/links =====
            {
                var id = await PC("Front - Shock/Strut");
                Add(parts, id, "Front Shock/Strut Pair - Budget", 1800, 18);
                Add(parts, id, "Front Shock/Strut Pair - Standard", 2600, 14);
                Add(parts, id, "Front Shock/Strut Pair - Premium", 3600, 10);
            }
            {
                var id = await PC("Rear - Shock");
                Add(parts, id, "Rear Shock Pair - Budget", 1700, 18);
                Add(parts, id, "Rear Shock Pair - Standard", 2500, 14);
                Add(parts, id, "Rear Shock Pair - Premium", 3500, 10);
            }
            {
                var id = await PC("Front - Control Arm");
                Add(parts, id, "Front Control Arm - Budget", 900, 20);
                Add(parts, id, "Front Control Arm - Standard", 1400, 16);
                Add(parts, id, "Front Control Arm - Premium", 2000, 12);
            }
            {
                var id = await PC("Bushings - Suspension");
                Add(parts, id, "Suspension Bushing Set - Budget", 300, 30);
                Add(parts, id, "Suspension Bushing Set - Standard", 500, 25);
                Add(parts, id, "Suspension Bushing Set - Premium (PU)", 900, 15);
            }
            {
                var id = await PC("Stabilizer Link - Front");
                Add(parts, id, "Front Stabilizer Link Pair - Budget", 250, 30);
                Add(parts, id, "Front Stabilizer Link Pair - Standard", 400, 25);
                Add(parts, id, "Front Stabilizer Link Pair - Premium", 650, 15);
            }

            // ===== STEERING: tie rod/rack =====
            {
                var id = await PC("Steering - Tie Rod End");
                Add(parts, id, "Tie Rod End Pair - Budget", 500, 25);
                Add(parts, id, "Tie Rod End Pair - Standard", 800, 20);
                Add(parts, id, "Tie Rod End Pair - Premium", 1200, 12);
            }
            {
                var id = await PC("Steering - Rack");
                Add(parts, id, "Steering Rack - Reman (Budget)", 2500, 6);
                Add(parts, id, "Steering Rack - Standard", 3500, 5);
                Add(parts, id, "Steering Rack - Premium (OE Grade)", 5200, 3);
            }

            // ===== ENGINE basics =====
            {
                var id = await PC("Engine - Oil");
                Add(parts, id, "Engine Oil 4L - Mineral (Budget)", 900, 80);
                Add(parts, id, "Engine Oil 4L - Semi Synthetic (Standard)", 1300, 60);
                Add(parts, id, "Engine Oil 4L - Full Synthetic (Premium)", 1800, 40);
            }
            {
                var id = await PC("Engine - Oil Filter");
                Add(parts, id, "Oil Filter - Budget", 120, 80);
                Add(parts, id, "Oil Filter - Standard", 200, 80);
                Add(parts, id, "Oil Filter - Premium", 350, 60);
            }
            {
                var id = await PC("Engine - Air Filter");
                Add(parts, id, "Air Filter - Budget", 250, 60);
                Add(parts, id, "Air Filter - Standard", 400, 50);
                Add(parts, id, "Air Filter - Premium", 650, 40);
                Add(parts, id, "Air Filter - Toyota Vios 2014-2018", 420, 30);
                Add(parts, id, "Air Filter - Honda City 2014-2019", 450, 30);
            }
            {
                var id = await PC("Engine - Spark Plugs");
                Add(parts, id, "Spark Plug Set - Nickel (Budget)", 600, 50);
                Add(parts, id, "Spark Plug Set - Platinum (Standard)", 900, 40);
                Add(parts, id, "Spark Plug Set - Iridium (Premium)", 1400, 30);
            }
            {
                var id = await PC("Engine - Ignition Coils");
                Add(parts, id, "Ignition Coil - Budget", 700, 25);
                Add(parts, id, "Ignition Coil - Standard", 1100, 20);
                Add(parts, id, "Ignition Coil - Premium", 1700, 12);
            }

            // ===== COOLING =====
            {
                var id = await PC("Cooling - Radiator");
                Add(parts, id, "Radiator - Budget", 1500, 10);
                Add(parts, id, "Radiator - Standard", 2300, 8);
                Add(parts, id, "Radiator - Premium", 3400, 6);
            }
            {
                var id = await PC("Cooling - Water Pump");
                Add(parts, id, "Water Pump - Budget", 900, 12);
                Add(parts, id, "Water Pump - Standard", 1400, 10);
                Add(parts, id, "Water Pump - Premium", 2100, 8);
            }
            {
                var id = await PC("Cooling - Thermostat");
                Add(parts, id, "Thermostat - Budget", 250, 20);
                Add(parts, id, "Thermostat - Standard", 400, 18);
                Add(parts, id, "Thermostat - Premium", 650, 12);
            }
            {
                var id = await PC("Cooling - Coolant");
                Add(parts, id, "Coolant 4L - Budget", 300, 50);
                Add(parts, id, "Coolant 4L - Standard", 450, 40);
                Add(parts, id, "Coolant 4L - Premium (Long Life)", 650, 30);
            }

            // ===== ELECTRICAL =====
            {
                var id = await PC("Electrical - Battery");
                Add(parts, id, "Battery 12V 45Ah - Budget", 1800, 20);
                Add(parts, id, "Battery 12V 55Ah - Standard", 2600, 15);
                Add(parts, id, "Battery 12V 60Ah AGM - Premium", 3800, 8);
            }
            {
                var id = await PC("Electrical - Alternator");
                Add(parts, id, "Alternator - Budget", 2500, 8);
                Add(parts, id, "Alternator - Standard", 3500, 6);
                Add(parts, id, "Alternator - Premium", 5200, 4);
            }
            {
                var id = await PC("Electrical - Starter Motor");
                Add(parts, id, "Starter Motor - Budget", 2300, 8);
                Add(parts, id, "Starter Motor - Standard", 3300, 6);
                Add(parts, id, "Starter Motor - Premium", 4800, 4);
            }

            // ===== SENSORS =====
            {
                var id = await PC("Sensors - O2");
                Add(parts, id, "O2 Sensor - Budget", 700, 20);
                Add(parts, id, "O2 Sensor - Standard", 1100, 16);
                Add(parts, id, "O2 Sensor - Premium", 1700, 10);
            }
            {
                var id = await PC("Sensors - ABS Front");
                Add(parts, id, "ABS Sensor Front - Budget", 650, 20);
                Add(parts, id, "ABS Sensor Front - Standard", 1000, 15);
                Add(parts, id, "ABS Sensor Front - Premium", 1500, 10);
            }
            {
                var id = await PC("Sensors - ABS Rear");
                Add(parts, id, "ABS Sensor Rear - Budget", 650, 20);
                Add(parts, id, "ABS Sensor Rear - Standard", 1000, 15);
                Add(parts, id, "ABS Sensor Rear - Premium", 1500, 10);
            }

            // ===== HVAC =====
            {
                var id = await PC("HVAC - Cabin Filter");
                Add(parts, id, "Cabin Filter - Budget", 200, 40);
                Add(parts, id, "Cabin Filter - Standard", 320, 30);
                Add(parts, id, "Cabin Filter - Premium (Carbon)", 500, 20);
            }
            {
                var id = await PC("HVAC - Refrigerant");
                Add(parts, id, "Refrigerant R134a - Budget", 250, 80);
                Add(parts, id, "Refrigerant R134a - Standard", 400, 60);
                Add(parts, id, "Refrigerant R134a - Premium", 650, 40);
            }
            {
                var id = await PC("HVAC - AC Compressor");
                Add(parts, id, "AC Compressor - Budget", 3500, 6);
                Add(parts, id, "AC Compressor - Standard", 5200, 4);
                Add(parts, id, "AC Compressor - Premium", 7600, 3);
            }

            // ===== EXHAUST =====
            {
                var id = await PC("Exhaust - Muffler");
                Add(parts, id, "Muffler - Budget", 1800, 10);
                Add(parts, id, "Muffler - Standard", 2600, 8);
                Add(parts, id, "Muffler - Premium", 3800, 6);
            }
            {
                var id = await PC("Exhaust - Catalytic Converter");
                Add(parts, id, "Catalytic Converter - Budget", 4500, 3);
                Add(parts, id, "Catalytic Converter - Standard", 6500, 2);
                Add(parts, id, "Catalytic Converter - Premium (OE Grade)", 9500, 1);
            }

            _context.Parts.AddRange(parts);
            await _context.SaveChangesAsync();
        }





        private async Task SeedServiceCategoriesAsync()
        {
            var existing = await _context.ServiceCategories
                .Select(c => c.CategoryName)
                .ToListAsync();

            void AddIfMissing(ServiceCategory cat)
            {
                if (!existing.Contains(cat.CategoryName))
                    _context.ServiceCategories.Add(cat);
            }

            // ===== PARENT =====
            AddIfMissing(new ServiceCategory { CategoryName = "Maintenance", Description = "General maintenance services" });
            AddIfMissing(new ServiceCategory { CategoryName = "Repair", Description = "Repair services" });
            AddIfMissing(new ServiceCategory { CategoryName = "Inspection", Description = "Inspection & diagnostics" });
            AddIfMissing(new ServiceCategory { CategoryName = "Upgrade", Description = "Upgrade & enhancement services" });

            await _context.SaveChangesAsync();

            var parents = await _context.ServiceCategories.ToListAsync();
            Guid P(string name) => parents.First(c => c.CategoryName == name).ServiceCategoryId;

            // ===== CHILD =====
            // ===== MAINTENANCE =====
            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Oil Change",
                ParentServiceCategoryId = P("Maintenance"),
                Description = "Engine oil and oil filter replacement to maintain engine lubrication and performance."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Tire Rotation",
                ParentServiceCategoryId = P("Maintenance"),
                Description = "Tire rotation, balancing, alignment and basic tire-related services to ensure even wear."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Battery Check",
                ParentServiceCategoryId = P("Maintenance"),
                Description = "Battery testing, charging system inspection, and battery replacement services."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Fluid Refill",
                ParentServiceCategoryId = P("Maintenance"),
                Description = "Refill and replacement of essential vehicle fluids such as brake fluid, coolant, and transmission fluid."
            });


            // ===== REPAIR =====
            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Brake Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Repair and replacement of brake components including pads, discs, calipers, and brake hydraulics."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Suspension Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Repair of suspension components such as shock absorbers, struts, control arms, and stabilizer links."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Steering Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Steering system repair including tie rods, steering rack, power steering components and alignment."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Electrical Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Electrical system repair including alternator, starter motor, sensors, wiring, and control modules."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Engine Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Engine repair and tuning services such as spark plugs, ignition coils, air intake, and fuel system components."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Cooling System Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Cooling system repair including radiator, water pump, thermostat, hoses, and coolant leaks."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "HVAC Service",
                ParentServiceCategoryId = P("Repair"),
                Description = "Heating, ventilation and air conditioning services including AC recharge, compressor repair, and cabin air quality."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Safety System Repair",
                ParentServiceCategoryId = P("Repair"),
                Description = "Repair of vehicle safety systems such as airbags, ABS, traction control, and safety sensors."
            });


            // ===== INSPECTION =====
            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Safety Inspection",
                ParentServiceCategoryId = P("Inspection"),
                Description = "Basic vehicle safety inspection covering brakes, tires, lights, suspension and essential safety items."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Engine Diagnostic",
                ParentServiceCategoryId = P("Inspection"),
                Description = "Computer-based diagnostics using OBD tools to detect engine, sensor, and performance-related faults."
            });

            AddIfMissing(new ServiceCategory
            {
                CategoryName = "Pre-Purchase Inspection",
                ParentServiceCategoryId = P("Inspection"),
                Description = "Comprehensive vehicle inspection before purchase including engine, transmission, brakes, suspension, body and interior."
            });


            await _context.SaveChangesAsync();
        }



        private async Task SeedServicesAsync()
        {
            

            var categories = await _context.ServiceCategories.ToListAsync();
            Guid Cat(string name)
            {
                var cat = categories.FirstOrDefault(c => c.CategoryName.Equals(name));
                if (cat == null)
                    throw new Exception($" ServiceCategory '{name}' NOT FOUND. Check SeedServiceCategoriesAsync.");

                return cat.ServiceCategoryId;
            }

            var now = DateTime.UtcNow;

            var services = new List<Service>
    {
        // ================= OIL CHANGE =================
        new Service
        {
            ServiceName = "Basic Oil Change",
            Description = "Drain old oil, replace oil filter, refill with standard engine oil.",
            ServiceCategoryId = Cat("Oil Change"),
            Price = 1000, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Premium Oil Change",
            Description = "Drain old oil, replace oil filter, refill with synthetic engine oil.",
            ServiceCategoryId = Cat("Oil Change"),
            Price = 1500, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },

        // ================= TIRES / WHEELS =================
        new Service
        {
            ServiceName = "Replace Front Tires (Pair)",
            Description = "Replace 2 front tires; check tread/wear pattern; inflate to spec.",
            ServiceCategoryId = Cat("Tire Rotation"),
            Price = 2500, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Replace Rear Tires (Pair)",
            Description = "Replace 2 rear tires; check tread/wear pattern; inflate to spec.",
            ServiceCategoryId = Cat("Tire Rotation"),
            Price = 2500, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Tire Rotation Service",
            Description = "Rotate tires to ensure even wear and longer tire life.",
            ServiceCategoryId = Cat("Tire Rotation"),
            Price = 1200, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Wheel Balancing (4 wheels)",
            Description = "Balance 4 wheels to reduce vibration at speed.",
            ServiceCategoryId = Cat("Tire Rotation"),
            Price = 1200, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Wheel Alignment (4 wheels)",
            Description = "Adjust camber/caster/toe to correct steering pull and uneven wear.",
            ServiceCategoryId = Cat("Steering Repair"),
            Price = 1800, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Tire Puncture Repair (Front)",
            Description = "Repair puncture on front tire (patch/plug depending on condition).",
            ServiceCategoryId = Cat("Tire Rotation"),
            Price = 600, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Tire Puncture Repair (Rear)",
            Description = "Repair puncture on rear tire (patch/plug depending on condition).",
            ServiceCategoryId = Cat("Tire Rotation"),
            Price = 600, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },

        // ================= BATTERY / ELECTRICAL =================
        new Service
        {
            ServiceName = "Battery Health Check",
            Description = "Test battery capacity/CCA and inspect terminals & charging condition.",
            ServiceCategoryId = Cat("Battery Check"),
            Price = 1100, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Battery Replacement",
            Description = "Replace battery, clean terminals, and test charging system.",
            ServiceCategoryId = Cat("Battery Check"),
            Price = 2600, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Alternator Replacement",
            Description = "Replace alternator and verify charging voltage and belt condition.",
            ServiceCategoryId = Cat("Electrical Repair"),
            Price = 3800, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Starter Motor Replacement",
            Description = "Replace starter motor and check wiring/ground connections.",
            ServiceCategoryId = Cat("Electrical Repair"),
            Price = 3600, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },

        // ================= BRAKES (tách trước/sau) =================
        new Service
        {
            ServiceName = "Replace Front Brake Pads",
            Description = "Replace front brake pads; inspect discs and calipers.",
            ServiceCategoryId = Cat("Brake Repair"),
            Price = 1800, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Replace Rear Brake Pads",
            Description = "Replace rear brake pads; inspect discs and calipers.",
            ServiceCategoryId = Cat("Brake Repair"),
            Price = 1800, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Replace Front Brake Discs (Pair)",
            Description = "Replace front brake discs/rotors (pair) and check pad contact surface.",
            ServiceCategoryId = Cat("Brake Repair"),
            Price = 2400, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Replace Rear Brake Discs (Pair)",
            Description = "Replace rear brake discs/rotors (pair) and check pad contact surface.",
            ServiceCategoryId = Cat("Brake Repair"),
            Price = 2400, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Brake Fluid Flush & Bleed",
            Description = "Flush brake fluid and bleed to remove air; improve pedal feel.",
            ServiceCategoryId = Cat("Fluid Refill"),
            Price = 1400, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },

        // Nâng cao (multi-partcategory)
        new Service
        {
            ServiceName = "Front Brake Overhaul",
            Description = "Front brake overhaul: pads + discs + caliper service + fluid bleed.",
            ServiceCategoryId = Cat("Brake Repair"),
            Price = 5200, EstimatedDuration = 5, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Rear Brake Overhaul",
            Description = "Rear brake overhaul: pads + discs + caliper service + fluid bleed.",
            ServiceCategoryId = Cat("Brake Repair"),
            Price = 5000, EstimatedDuration = 5, IsActive = true, CreatedAt = now
        },

        // ================= SUSPENSION / STEERING =================
        new Service
        {
            ServiceName = "Replace Front Shock Absorbers (Pair)",
            Description = "Replace front shocks/struts (pair); inspect mounts/bushings.",
            ServiceCategoryId = Cat("Suspension Repair"),
            Price = 3200, EstimatedDuration = 4, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Replace Rear Shock Absorbers (Pair)",
            Description = "Replace rear shocks (pair); inspect bushings and links.",
            ServiceCategoryId = Cat("Suspension Repair"),
            Price = 3000, EstimatedDuration = 4, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Tie Rod End Replacement",
            Description = "Replace tie rod end(s); recommended alignment afterward.",
            ServiceCategoryId = Cat("Steering Repair"),
            Price = 2200, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Steering Rack Repair",
            Description = "Repair/replace steering rack and inspect tie rods; road test after.",
            ServiceCategoryId = Cat("Steering Repair"),
            Price = 3800, EstimatedDuration = 5, IsActive = true, CreatedAt = now
        },

        // ================= ENGINE / INTAKE / SENSORS =================
        new Service
        {
            ServiceName = "Engine Tune-Up",
            Description = "Tune-up: replace plugs/filters as needed and check ignition/fuel trims.",
            ServiceCategoryId = Cat("Engine Repair"),
            Price = 2500, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Spark Plug Replacement",
            Description = "Replace spark plugs; check gaps/condition and misfire history.",
            ServiceCategoryId = Cat("Engine Repair"),
            Price = 1600, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Ignition Coil Replacement",
            Description = "Replace faulty ignition coil(s) causing misfire; clear codes and test drive.",
            ServiceCategoryId = Cat("Electrical Repair"),
            Price = 2000, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Throttle Body Cleaning",
            Description = "Clean throttle body and inspect/clean related air intake sensors if needed.",
            ServiceCategoryId = Cat("Engine Repair"),
            Price = 1700, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Oxygen Sensor Replacement",
            Description = "Replace oxygen (O2) sensor and verify fuel trim / emission readings.",
            ServiceCategoryId = Cat("Electrical Repair"),
            Price = 2200, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },

        // ================= COOLING =================
        new Service
        {
            ServiceName = "Radiator Replacement",
            Description = "Replace radiator and pressure test system; refill coolant.",
            ServiceCategoryId = Cat("Cooling System Repair"),
            Price = 4000, EstimatedDuration = 4, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Water Pump Replacement",
            Description = "Replace water pump; refill coolant and check for leaks/overheating.",
            ServiceCategoryId = Cat("Cooling System Repair"),
            Price = 3500, EstimatedDuration = 4, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Thermostat Replacement",
            Description = "Replace thermostat to fix overheating or slow warm-up issues.",
            ServiceCategoryId = Cat("Cooling System Repair"),
            Price = 2400, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },

        // ================= HVAC =================
        new Service
        {
            ServiceName = "AC Gas Recharge & Leak Check",
            Description = "Recharge AC refrigerant and check for leaks; verify vent temperature.",
            ServiceCategoryId = Cat("HVAC Service"),
            Price = 1700, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "AC Compressor Replacement",
            Description = "Replace AC compressor and recharge system; verify pressures and cooling.",
            ServiceCategoryId = Cat("HVAC Service"),
            Price = 6500, EstimatedDuration = 6, IsActive = true, CreatedAt = now
        },

        // ================= SAFETY =================
        new Service
        {
            ServiceName = "ABS Sensor Replacement (Front)",
            Description = "Replace front ABS wheel speed sensor; clear codes and verify live data.",
            ServiceCategoryId = Cat("Safety System Repair"),
            Price = 2200, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "ABS Sensor Replacement (Rear)",
            Description = "Replace rear ABS wheel speed sensor; clear codes and verify live data.",
            ServiceCategoryId = Cat("Safety System Repair"),
            Price = 2200, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },

        // ================= INSPECTION / DIAGNOSTIC (optional but useful demo) =================
        new Service
        {
            ServiceName = "Basic Safety Inspection",
            Description = "Quick safety check: brakes/tires/lights/basic undercar check.",
            ServiceCategoryId = Cat("Safety Inspection"),
            Price = 1000, EstimatedDuration = 1, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Full Engine Diagnostic",
            Description = "OBD scan + live data review to identify engine/sensor faults.",
            ServiceCategoryId = Cat("Engine Diagnostic"),
            Price = 1600, EstimatedDuration = 2, IsActive = true, CreatedAt = now
        },
        new Service
        {
            ServiceName = "Pre-Purchase Inspection",
            Description = "Comprehensive inspection before purchase: engine, brakes, suspension, body, interior.",
            ServiceCategoryId = Cat("Pre-Purchase Inspection"),
            Price = 2500, EstimatedDuration = 3, IsActive = true, CreatedAt = now
        },


        // ================= ADVANCED SERVICES =================

                        // 🔧 Advanced Brake + Suspension (rất hay gặp)
                        new Service
                        {
                            ServiceName = "Front Suspension & Brake Refresh",
                            Description = "Refresh front suspension and brakes: shocks, brake pads, discs, and alignment check.",
                            ServiceCategoryId = Cat("Suspension Repair"),
                            Price = 7800,
                            EstimatedDuration = 7,
                            IsActive = true,
                            IsAdvanced= true,
                            CreatedAt = now
                        },

                        // ⚙️ Engine + Cooling combo (xe nóng máy, rất thực tế)
                        new Service
                        {
                            ServiceName = "Engine Cooling System Overhaul",
                            Description = "Overhaul cooling system: radiator, water pump, thermostat, hoses, and coolant.",
                            ServiceCategoryId = Cat("Cooling System Repair"),
                            Price = 8200,
                            EstimatedDuration = 7,
                            IsActive = true,
                            IsAdvanced= true,
                            CreatedAt = now
                        },

                        // 🌀 HVAC + Electrical (xe mất lạnh, hay chết lốc)
                        new Service
                        {
                            ServiceName = "Complete AC System Repair",
                            Description = "Complete AC repair: compressor replacement, refrigerant recharge, and cabin filter.",
                            ServiceCategoryId = Cat("HVAC Service"),
                            Price = 9500,
                            EstimatedDuration = 8,
                            IsActive = true,
                            IsAdvanced= true,
                            CreatedAt = now
                        }


                    };

            var existingNames = await _context.Services
            .Select(s => s.ServiceName)
            .ToListAsync();

            var toAdd = services
                .Where(s => !existingNames.Contains(s.ServiceName))
                .ToList();

            if (toAdd.Count == 0) return;

            _context.Services.AddRange(services);
            await _context.SaveChangesAsync();
        }



        private async Task SeedServicePartCategoriesAsync()
        {
            if (_context.ServicePartCategories.Any()) return;

            // Cache
            var services = await _context.Services.ToListAsync();
            var pcs = await _context.PartCategories.ToListAsync();

            Guid GetServiceId(string name) => services.First(s => s.ServiceName == name).ServiceId;
            Guid GetPcId(string name) => pcs.First(p => p.CategoryName == name).LaborCategoryId; // đổi nếu PK khác

            var now = DateTime.UtcNow;
            var mappings = new List<ServicePartCategory>();

            void Map(string serviceName, params string[] partCategoryNames)
            {
                var sid = GetServiceId(serviceName);
                foreach (var pcName in partCategoryNames.Distinct())
                {
                    mappings.Add(new ServicePartCategory
                    {
                        ServiceId = sid,
                        PartCategoryId = GetPcId(pcName),
                        CreatedAt = now
                    });
                }
            }

            // ================= OIL CHANGE =================
            Map("Basic Oil Change", "Engine - Oil", "Engine - Oil Filter");
            Map("Premium Oil Change", "Engine - Oil", "Engine - Oil Filter");

            // ================= TIRES / WHEELS =================
            // sửa lỗi logic cũ: tire rotation không map suspension
            Map("Tire Rotation Service", "Front - Tires", "Rear - Tires");
            Map("Wheel Balancing (4 wheels)", "Wheel - Balancing Weights", "Front - Tires", "Rear - Tires");
            Map("Wheel Alignment (4 wheels)", "Steering - Tie Rod End", "Front - Control Arm", "Front - Shock/Strut"); // alignment thường dính lái + treo

            Map("Replace Front Tires (Pair)", "Front - Tires");
            Map("Replace Rear Tires (Pair)", "Rear - Tires");
            Map("Tire Puncture Repair (Front)", "Front - Tires");
            Map("Tire Puncture Repair (Rear)", "Rear - Tires");

            // ================= BATTERY / ELECTRICAL =================
            Map("Battery Health Check", "Electrical - Battery");         // nếu service này chỉ check battery
            Map("Battery Replacement", "Electrical - Battery");
            Map("Alternator Replacement", "Electrical - Alternator");
            Map("Starter Motor Replacement", "Electrical - Starter Motor");

            // ================= BRAKES (tách trước/sau đúng garage) =================
            Map("Replace Front Brake Pads", "Front - Brake Pads");
            Map("Replace Rear Brake Pads", "Rear - Brake Pads");
            Map("Replace Front Brake Discs (Pair)", "Front - Brake Discs");
            Map("Replace Rear Brake Discs (Pair)", "Rear - Brake Discs");
            Map("Brake Fluid Flush & Bleed", "Brake - Fluid");

            // Service nâng cao: nhiều PartCategory
            Map("Front Brake Overhaul",
                "Front - Brake Pads",
                "Front - Brake Discs",
                "Front - Brake Calipers",
                "Brake - Fluid");

            Map("Rear Brake Overhaul",
                "Rear - Brake Pads",
                "Rear - Brake Discs",
                "Rear - Brake Calipers",
                "Brake - Fluid");

            // ================= SUSPENSION / STEERING =================
            Map("Replace Front Shock Absorbers (Pair)", "Front - Shock/Strut");
            Map("Replace Rear Shock Absorbers (Pair)", "Rear - Shock");
            Map("Tie Rod End Replacement", "Steering - Tie Rod End");
            Map("Steering Rack Repair", "Steering - Rack", "Steering - Tie Rod End");

            // ================= ENGINE / INTAKE / SENSORS =================
            Map("Engine Tune-Up", "Engine - Spark Plugs", "Engine - Air Filter", "Engine - Oil Filter"); // tune-up thực tế hay gồm lọc gió/bugi/lọc dầu
            Map("Spark Plug Replacement", "Engine - Spark Plugs");
            Map("Ignition Coil Replacement", "Engine - Ignition Coils");
            Map("Throttle Body Cleaning", "Intake - Throttle Body", "Sensors - MAF/MAP");
            Map("Oxygen Sensor Replacement", "Sensors - O2");

            // ================= COOLING =================
            Map("Radiator Replacement", "Cooling - Radiator", "Cooling - Coolant");
            Map("Water Pump Replacement", "Cooling - Water Pump", "Cooling - Coolant");
            Map("Thermostat Replacement", "Cooling - Thermostat", "Cooling - Coolant");

            // ================= HVAC =================
            Map("AC Gas Recharge & Leak Check", "HVAC - Refrigerant");
            Map("AC Compressor Replacement", "HVAC - AC Compressor", "HVAC - Refrigerant");
            // (nếu có service cabin filter)
            // Map("Cabin Filter Replacement", "HVAC - Cabin Filter");

            // ================= SAFETY =================
            Map("ABS Sensor Replacement (Front)", "Sensors - ABS Front");
            Map("ABS Sensor Replacement (Rear)", "Sensors - ABS Rear");



            // ================= ADVANCED SERVICE MAPPINGS =================

            // 🔧 Front Suspension & Brake Refresh
            Map(
                "Front Suspension & Brake Refresh",
                "Front - Shock/Strut",
                "Front - Brake Pads",
                "Front - Brake Discs",
                "Front - Brake Calipers",
                "Brake - Fluid",
                "Front - Control Arm",
                "Stabilizer Link - Front"
            );

            // ⚙️ Engine Cooling System Overhaul
            Map(
                "Engine Cooling System Overhaul",
                "Cooling - Radiator",
                "Cooling - Water Pump",
                "Cooling - Thermostat",
                "Cooling - Hoses",
                "Cooling - Coolant"
            );

            // 🌀 Complete AC System Repair
            Map(
                "Complete AC System Repair",
                "HVAC - AC Compressor",
                "HVAC - Refrigerant",
                "HVAC - Cabin Filter",
                "Electrical - Fuses/Relays"
            );



            // ================= INSPECTION / DIAGNOSTIC =================
            // Nếu bạn còn giữ các service này trong SeedServicesAsync cũ:
            // Map("Basic Safety Inspection", "Front - Brake Pads", "Rear - Brake Pads", "Front - Tires", "Rear - Tires");
            // Map("Full Engine Diagnostic", "Sensors - MAF/MAP", "Sensors - O2");

            _context.ServicePartCategories.AddRange(mappings);
            await _context.SaveChangesAsync();
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
                    var services = await _context.Services.Take(20).ToListAsync();
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


        private async Task SeedInspectionTypesAsync()
        {
            if (!_context.InspectionTypes.Any())
            {
                var inspectionTypes = new List<BusinessObject.InspectionType>
                {
                    new BusinessObject.InspectionType
                    {
                        TypeName = "Basic",
                        InspectionFee = 1000m,
                        Description = "Giá kiểm tra cơ bản",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new BusinessObject.InspectionType
                    {
                        TypeName = "Advanced",
                        InspectionFee = 2000m,
                        Description = "Giá kiểm tra dịch vụ nâng cao",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                _context.InspectionTypes.AddRange(inspectionTypes);
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
                                OrderStatusId = pendingStatus.OrderStatusId, 
                                ColorName = "Red",
                                HexCode = "#FF0000",
                                IsDefault = true 
                            },
                            new Label
                            {
                                LabelName = "In Progress",
                                Description = "Order is being worked on",
                                OrderStatusId = inProgressStatus.OrderStatusId, 
                                ColorName = "Yellow",
                                HexCode = "#FFFF00",
                                IsDefault = true 
                            },
                            new Label
                            {
                                LabelName = "Done",
                                Description = "Order completed",
                                OrderStatusId = completedStatus.OrderStatusId, 
                                ColorName = "Green",
                                HexCode = "#00FF00",
                                IsDefault = true
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
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "White",  HexCode = "#FFFFFF", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Black",  HexCode = "#000000", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Silver", HexCode = "#C0C0C0", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Gray",   HexCode = "#808080", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Red",    HexCode = "#FF0000", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Blue",   HexCode = "#0000FF", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Green",  HexCode = "#008000", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Yellow", HexCode = "#FFFF00", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Orange", HexCode = "#FFA500", CreatedAt = DateTime.UtcNow },
              //      new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Purple", HexCode = "#800080", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Brown",  HexCode = "#A52A2A", CreatedAt = DateTime.UtcNow },
            //        new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Beige",  HexCode = "#F5F5DC", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Gold",   HexCode = "#FFD700", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Pink",   HexCode = "#FFC0CB", CreatedAt = DateTime.UtcNow },
                    new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Navy",   HexCode = "#000080", CreatedAt = DateTime.UtcNow },
          //          new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Teal",   HexCode = "#008080", CreatedAt = DateTime.UtcNow },
          //          new VehicleColor { ColorID = Guid.NewGuid(), ColorName = "Maroon", HexCode = "#800000", CreatedAt = DateTime.UtcNow }
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
        private async Task SeedVehicleModelColorsAsync()
        {
            var models = await _context.VehicleModels.ToListAsync();
            var colors = await _context.VehicleColors.ToListAsync();
            if (models.Count == 0 || colors.Count == 0) return;

            var existingPairs = await _context.VehicleModelColors
                .Select(x => new { x.ModelID, x.ColorID })
                .ToListAsync();
            var set = new HashSet<(Guid, Guid)>(existingPairs.Select(e => (e.ModelID, e.ColorID)));

            var preferredNames = new[] { "White", "Black", "Silver", "Blue", "Red" };
            var preferred = colors.Where(c => preferredNames.Contains(c.ColorName)).ToList();
            if (preferred.Count == 0) preferred = colors.Take(Math.Min(5, colors.Count)).ToList();

            var toAdd = new List<VehicleModelColor>();
            foreach (var m in models)
            {
                var chosen = preferred.Count > 0 ? preferred : colors;
                foreach (var c in chosen)
                {
                    var key = (m.ModelID, c.ColorID);
                    if (!set.Contains(key))
                    {
                        toAdd.Add(new VehicleModelColor { ModelID = m.ModelID, ColorID = c.ColorID });
                        set.Add(key);
                    }
                }
            }

            if (toAdd.Count > 0)
            {
                _context.VehicleModelColors.AddRange(toAdd);
                await _context.SaveChangesAsync();
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
                DiscountType = DiscountType.Fixed,
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
                        var selectedServices = services.Take(10).ToList();

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

        private async Task SeedPriceEmergenciesAsync()
        {
            if (!_context.PriceEmergencies.Any())
            {
                var now = DateTime.UtcNow;
                var prices = new List<PriceEmergency>
                {
                    new PriceEmergency { PriceId = Guid.NewGuid(), BasePrice = 50000m, PricePerKm = 10000m, DateCreated = now.AddDays(-14) },
                    new PriceEmergency { PriceId = Guid.NewGuid(), BasePrice = 60000m, PricePerKm = 12000m, DateCreated = now.AddDays(-7) },
                    new PriceEmergency { PriceId = Guid.NewGuid(), BasePrice = 70000m, PricePerKm = 15000m, DateCreated = now }
                };
                _context.PriceEmergencies.AddRange(prices);
                await _context.SaveChangesAsync();
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

                // Lấy các service từ database - use FirstOrDefaultAsync and check for null
                var basicOilChange = await _context.Services.FirstOrDefaultAsync(s => s.ServiceName == "Basic Oil Change");
                var brakePadReplacement = await _context.Services.FirstOrDefaultAsync(s => s.ServiceName == "Brake Pad Replacement");
                var engineTuneUp = await _context.Services.FirstOrDefaultAsync(s => s.ServiceName == "Engine Tune-Up");
                var fullEngineDiagnostic = await _context.Services.FirstOrDefaultAsync(s => s.ServiceName == "Full Engine Diagnostic");
                var tireRotation = await _context.Services.FirstOrDefaultAsync(s => s.ServiceName == "Tire Rotation Service");

                // If specific services not found, get any available services
                var availableServices = await _context.Services.Take(5).ToListAsync();
                if (availableServices.Count == 0)
                {
                    Console.WriteLine("⚠️ No services found in database. Skipping RepairOrder seeding.");
                    return;
                }

                // Use found services or fallback to available ones
                basicOilChange ??= availableServices.ElementAtOrDefault(0);
                brakePadReplacement ??= availableServices.ElementAtOrDefault(1);
                engineTuneUp ??= availableServices.ElementAtOrDefault(2);
                fullEngineDiagnostic ??= availableServices.ElementAtOrDefault(3);
                tireRotation ??= availableServices.ElementAtOrDefault(4) ?? availableServices.First();

                // Lấy các parts từ database - use FirstOrDefaultAsync and check for null
                var oilFilterMedium = await _context.Parts.FirstOrDefaultAsync(p => p.Name == "Oil Filter (Medium)");
                var airFilterCheap = await _context.Parts.FirstOrDefaultAsync(p => p.Name == "Air Filter (Cheap)");
                var brakePadCheap = await _context.Parts.FirstOrDefaultAsync(p => p.Name == "Brake Pad (Cheap)");
                var brakeDiscMedium = await _context.Parts.FirstOrDefaultAsync(p => p.Name == "Brake Disc (Medium)");
                var sparkPlugExpensive = await _context.Parts.FirstOrDefaultAsync(p => p.Name == "Spark Plug (Expensive)");
                var shockAbsorberCheap = await _context.Parts.FirstOrDefaultAsync(p => p.Name == "Shock Absorber (Cheap)");

                // If specific parts not found, get any available parts
                var availableParts = await _context.Parts.Take(6).ToListAsync();
                if (availableParts.Count == 0)
                {
                    Console.WriteLine("⚠️ No parts found in database. Skipping RepairOrder seeding.");
                    return;
                }

                // Use found parts or fallback to available ones
                oilFilterMedium ??= availableParts.ElementAtOrDefault(0);
                airFilterCheap ??= availableParts.ElementAtOrDefault(1);
                brakePadCheap ??= availableParts.ElementAtOrDefault(2);
                brakeDiscMedium ??= availableParts.ElementAtOrDefault(3);
                sparkPlugExpensive ??= availableParts.ElementAtOrDefault(4);
                shockAbsorberCheap ??= availableParts.ElementAtOrDefault(5) ?? availableParts.First();

                // Lấy branch và status
                var branch = await _context.Branches.FirstOrDefaultAsync();
                if (branch == null)
                {
                    Console.WriteLine("⚠️ No branches found in database. Skipping RepairOrder seeding.");
                    return;
                }

                var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Pending");
                var inProgressStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "In Progress");
                var completedStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Completed");

                if (pendingStatus == null || inProgressStatus == null || completedStatus == null)
                {
                    Console.WriteLine("⚠️ Required order statuses not found. Skipping RepairOrder seeding.");
                    return;
                }

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
        // Thêm using nếu cần:
        // using System.Globalization;


        private async Task SeedManyCustomersAndRepairOrdersAsync(int customerCount = 10, int totalOrdersTarget = 500)
        {
            // Nếu đã có nhiều dữ liệu thì không seed nữa (bảo vệ)
            if (_context.RepairOrders.Any() || _context.Users.Count() > 50) // adjust thresholds as needed
            {
                Console.WriteLine("Already have RepairOrders or plenty of users - skipping bulk seeding.");
                return;
            }

            var rand = new Random();

            // Ensure we have required pools
            var services = await _context.Services.ToListAsync();
            var parts = await _context.Parts.ToListAsync();
            var branches = await _context.Branches.ToListAsync();
            var statuses = await _context.OrderStatuses.ToListAsync();
            var technicians = await _context.Technicians.ToListAsync();
            var vehicleBrands = await _context.VehicleBrands.ToListAsync();
            var vehicleColors = await _context.VehicleColors.ToListAsync();
            var vehicleModels = await _context.VehicleModels.ToListAsync();

            if (!services.Any() || !parts.Any() || !branches.Any() || !statuses.Any() || !technicians.Any() || !vehicleBrands.Any())
            {
                throw new Exception("Missing required seed data (services/parts/branches/technicians/etc.). Run other seeders first.");
            }

            var createdCustomerIds = new List<string>();
            var createdVehicleIds = new List<Guid>();

            // 1. Create customers
            for (int i = 0; i < customerCount; i++)
            {
                var phone = $"0910000{100 + i}";
                var existing = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
                ApplicationUser user;
                if (existing == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = phone,
                        PhoneNumber = phone,
                        PhoneNumberConfirmed = true,
                        FirstName = $"Customer{i + 1}",
                        LastName = "Demo",
                        Email = $"{phone}@demo.local",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var password = _configuration["AdminUser:Password"] ?? "String@1";
                    var result = await _userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        Console.WriteLine($"Create customer {phone} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        continue;
                    }

                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                else
                {
                    user = existing;
                }

                createdCustomerIds.Add(user.Id);

                // 1.a Create 1-3 vehicles for this customer
                int vehiclePerCust = rand.Next(1, 4);
                for (int v = 0; v < vehiclePerCust; v++)
                {
                    var brand = vehicleBrands[rand.Next(vehicleBrands.Count)];
                    var modelsForBrand = vehicleModels.Where(m => m.BrandID == brand.BrandID).ToList();
                    var model = modelsForBrand.Any() ? modelsForBrand[rand.Next(modelsForBrand.Count)] : vehicleModels[rand.Next(vehicleModels.Count)];
                    var color = vehicleColors[rand.Next(vehicleColors.Count)];

                    var vehicle = new Vehicle
                    {
                        BrandId = brand.BrandID,
                        UserId = user.Id,
                        ModelId = model.ModelID,
                        ColorId = color.ColorID,
                        LicensePlate = $"{rand.Next(10, 99)}A{rand.Next(10000, 99999)}",
                        VIN = Guid.NewGuid().ToString().Substring(0, 17),
                        Year = 2015 + rand.Next(0, 8),
                        Odometer = rand.Next(0, 200000),
                        LastServiceDate = DateTime.UtcNow.AddDays(-rand.Next(0, 400)),
                        NextServiceDate = DateTime.UtcNow.AddDays(rand.Next(1, 180)),
                        WarrantyStatus = rand.Next(0, 2) == 0 ? "Expired" : "Active",
                        CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(0, 400))
                    };

                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync(); // need id
                    createdVehicleIds.Add(vehicle.VehicleId);
                }
            }

            // 2. Create many RepairOrders distributed over last 24 months
            var startDate = DateTime.UtcNow.AddMonths(-24);
            int createdOrders = 0;
            var batchSaveInterval = 100;

            var allCustomers = await _userManager.Users.Where(u => createdCustomerIds.Contains(u.Id)).ToListAsync();
            var allVehicles = await _context.Vehicles.Where(v => createdVehicleIds.Contains(v.VehicleId)).ToListAsync();

            var jobList = new List<Job>();
            var repairList = new List<Repair>();
            var jobPartsList = new List<JobPart>();
            var jobTechniciansList = new List<JobTechnician>();
            var repairOrdersList = new List<RepairOrder>();
            var repairOrderServicesList = new List<RepairOrderService>();
            var quotationsList = new List<Quotation>();
            var quotationServicesList = new List<QuotationService>();
            var quotationServicePartsList = new List<QuotationServicePart>();

            while (createdOrders < totalOrdersTarget)
            {
                // pick random date between startDate and now
                var span = (DateTime.UtcNow - startDate).TotalDays;
                var randDays = rand.NextDouble() * span;
                var receiveDate = startDate.AddDays(randDays);

                var customer = allCustomers[rand.Next(allCustomers.Count)];
                var vehicle = allVehicles[rand.Next(allVehicles.Count)];
                var branch = branches[rand.Next(branches.Count)];
                var status = statuses[rand.Next(statuses.Count)];
                var roType = (RoType)(rand.Next(Enum.GetNames(typeof(RoType)).Length)); // assume enum exists

                var estimatedRepairTime = rand.Next(1, 8);
                var estAmount = services.OrderBy(s => rand.Next()).Take(rand.Next(1, 4)).Sum(s => s.Price);

                // Create per-order temporary lists so we can decide paid/completed after jobs are created
                var currentJobs = new List<Job>();
                var currentRepairs = new List<Repair>();
                var currentJobParts = new List<JobPart>();
                var currentJobTechnicians = new List<JobTechnician>();
                var currentRepairOrderServices = new List<RepairOrderService>();
                var currentQuotations = new List<Quotation>();
                var currentQuotationServices = new List<QuotationService>();
                var currentQuotationServiceParts = new List<QuotationServicePart>();

                //  --- CREATE REPAIR REQUEST FIRST (to satisfy FK) ---
                var repairRequest = new RepairRequest
                {
                    RepairRequestID = Guid.NewGuid(),
                    BranchId = Guid.Parse("8B1CEB17-019E-41FD-A4F0-7E91941007C8"),
                    UserID = customer.Id,
                    VehicleID = vehicle.VehicleId,
                    Description = $"Auto-generated repair request for {customer.FirstName}",
                    CreatedAt = receiveDate,
                    UpdatedAt = receiveDate.AddHours(rand.Next(0, 48)),
                    Status = RepairRequestStatus.Accept  // hoặc cột status bạn đang dùng
                };

                // add to batch list
                _context.RepairRequests.Add(repairRequest);
                // không SaveChanges đây, để batch flush cùng RepairOrder


                var ro = new RepairOrder
                {

                    RepairOrderId = Guid.NewGuid(),
                    ReceiveDate = receiveDate,
                    RoType = roType,
                    EstimatedCompletionDate = receiveDate.AddDays(rand.Next(1, 10)),
                    // CompletionDate will be set later if all jobs completed
                    Cost = 0, // set later if paid
                    EstimatedAmount = estAmount,
                    PaidAmount = 0, // set later if paid
                    PaidStatus = PaidStatus.Pending, // default, may change after job creation
                    EstimatedRepairTime = estimatedRepairTime,
                    Note = $"Auto-generated order for stats ({receiveDate.ToString("yyyy-MM-dd")})",
                    BranchId = branch.BranchId,
                    StatusId = status.OrderStatusId,
                    VehicleId = vehicle.VehicleId,
                    UserId = customer.Id,
                    RepairRequestId = repairRequest.RepairRequestID,
                    CreatedAt = receiveDate,
                    UpdatedAt = receiveDate.AddDays(rand.Next(0, 5))
                };

                repairOrdersList.Add(ro);

                // pick 1-3 services for this RO
                var chosenServices = services.OrderBy(s => rand.Next()).Take(rand.Next(1, 4)).ToList();
                foreach (var s in chosenServices)
                {
                    var ros = new RepairOrderService
                    {
                        RepairOrderServiceId = Guid.NewGuid(),
                        RepairOrderId = ro.RepairOrderId,
                        ServiceId = s.ServiceId,
                        CreatedAt = receiveDate
                    };
                    repairOrderServicesList.Add(ros);
                    currentRepairOrderServices.Add(ros);
                }

                // create 1-3 jobs corresponding to services (and also related repairs, parts, technicians)
                foreach (var s in chosenServices)
                {
                    var job = new Job
                    {
                        JobId = Guid.NewGuid(),
                        ServiceId = s.ServiceId,
                        RepairOrderId = ro.RepairOrderId,
                        JobName = s.ServiceName,
                        Status = (JobStatus)rand.Next(Enum.GetNames(typeof(JobStatus)).Length),
                        Deadline = receiveDate.AddDays(rand.Next(0, 10)),
                        TotalAmount = s.Price,
                        Note = "Auto-generated job",
                        CreatedAt = receiveDate,
                        UpdatedAt = receiveDate,
                        AssignedAt = rand.Next(0, 2) == 0 ? (DateTime?)receiveDate.AddHours(rand.Next(1, 48)) : null,
                        AssignedByManagerId = customer.Id
                    };

                    currentJobs.Add(job);
                    jobList.Add(job);

                    // attach parts 0-3 per job
                    var partsForJob = parts.OrderBy(p => rand.Next()).Take(rand.Next(0, 3)).ToList();
                    foreach (var p in partsForJob)
                    {
                        var jp = new JobPart
                        {
                            JobPartId = Guid.NewGuid(),
                            JobId = job.JobId,
                            PartId = p.PartId,
                            Quantity = rand.Next(1, 4),
                            UnitPrice = p.Price,
                            CreatedAt = receiveDate
                        };
                        jobPartsList.Add(jp);
                        currentJobParts.Add(jp);
                    }

                    // assign 1-2 technicians
                    var techsForJob = technicians.OrderBy(t => rand.Next()).Take(rand.Next(1, Math.Min(3, technicians.Count))).ToList();
                    foreach (var t in techsForJob)
                    {
                        var jt = new JobTechnician
                        {
                            JobTechnicianId = Guid.NewGuid(),
                            JobId = job.JobId,
                            TechnicianId = t.TechnicianId,
                            CreatedAt = receiveDate
                        };
                        jobTechniciansList.Add(jt);
                        currentJobTechnicians.Add(jt);
                    }

                    // some jobs create Repairs entries (completed/in-progress)
                    var repair = new Repair
                    {
                        RepairId = Guid.NewGuid(),
                        JobId = job.JobId,
                        Description = $"Repair record for {job.JobName}",
                        StartTime = job.AssignedAt ?? receiveDate,
                        EndTime = job.Status == JobStatus.Completed ? (DateTime?)(job.AssignedAt?.AddHours(rand.Next(1, 6)) ?? receiveDate.AddHours(rand.Next(1, 6))) : null,
                        ActualTime = job.Status == JobStatus.Completed ? TimeSpan.FromHours(rand.Next(1, 6)) : (TimeSpan?)null,
                        EstimatedTime = TimeSpan.FromHours(rand.Next(1, 6)),
                        Notes = "Auto-generated repair note"
                    };
                    repairList.Add(repair);
                    currentRepairs.Add(repair);
                }

                // occasional quotation for pending orders
                if (rand.NextDouble() < 0.2) // 20% orders have quotations
                {
                    var q = new Quotation
                    {
                        QuotationId = Guid.NewGuid(),
                        RepairOrderId = ro.RepairOrderId,
                        UserId = customer.Id,
                        VehicleId = vehicle.VehicleId,
                        CreatedAt = receiveDate,
                        SentToCustomerAt = receiveDate.AddDays(rand.Next(0, 2)),
                        Status = QuotationStatus.Sent,
                        TotalAmount = estAmount,
                        DiscountAmount = rand.Next(0, 200000),
                        Note = "Auto-generated quotation",
                        ExpiresAt = receiveDate.AddDays(7)
                    };
                    quotationsList.Add(q);
                    currentQuotations.Add(q);

                    // add 1-2 services
                    var qServices = chosenServices.Take(rand.Next(1, chosenServices.Count + 1)).ToList();
                    foreach (var s in qServices)
                    {
                        var qs = new QuotationService
                        {
                            QuotationServiceId = Guid.NewGuid(),
                            QuotationId = q.QuotationId,
                            ServiceId = s.ServiceId,
                            IsSelected = true,
                            Price = s.Price
                        };
                        quotationServicesList.Add(qs);
                        currentQuotationServices.Add(qs);

                        // maybe add parts
                        var qParts = parts.OrderBy(p => rand.Next()).Take(rand.Next(0, 2)).ToList();
                        foreach (var p in qParts)
                        {
                            var qsp = new QuotationServicePart
                            {
                                QuotationServicePartId = Guid.NewGuid(),
                                QuotationServiceId = qs.QuotationServiceId,
                                PartId = p.PartId,
                                IsSelected = true,
                                Price = p.Price,
                                Quantity = rand.Next(1, 3)
                            };
                            quotationServicePartsList.Add(qsp);
                            currentQuotationServiceParts.Add(qsp);
                        }
                    }
                }

                // --- NEW: decide payment/completion AFTER jobs/repairs created for this RO ---
                bool allJobsCompleted = currentJobs.Count > 0 && currentJobs.All(j => j.Status == JobStatus.Completed);
                bool allRepairsHaveEnd = currentRepairs.Count == currentJobs.Count && currentRepairs.All(r => r.EndTime.HasValue);

                if (allJobsCompleted && allRepairsHaveEnd)
                {
                    // completed: set completion date to latest repair end
                    var latestEnd = currentRepairs.Max(r => r.EndTime.Value);
                    ro.CompletionDate = latestEnd;
                    ro.Cost = estAmount;
                    ro.PaidAmount = estAmount;
                    ro.PaidStatus = PaidStatus.Paid;
                    // make sure UpdatedAt reflects completion
                    ro.UpdatedAt = latestEnd;
                }
                else
                {
                    // NOT fully completed => NEVER set Paid.
                    // Decide Pending or Partial (partial = customer paid some deposit, but job(s) still not finished)
                    var paidDecision = rand.Next(0, 3); // 0 unpaid,1 partial,2 unpaid (treated same as 0)
                    if (paidDecision == 1)
                    {
                        // Partial payment allowed but order is not marked Paid and CompletionDate stays null
                        ro.PaidAmount = estAmount / 2;
                        ro.Cost = 0; // final cost not set until completion
                        ro.PaidStatus = PaidStatus.Partial;
                    }
                    else
                    {
                        ro.PaidAmount = 0;
                        ro.Cost = 0;
                        ro.PaidStatus = PaidStatus.Pending;
                    }
                    ro.CompletionDate = null;
                }

                createdOrders++;

                // flush in batches
                if (createdOrders % batchSaveInterval == 0 || createdOrders == totalOrdersTarget)
                {
                    // Add and save batches
                    _context.RepairOrders.AddRange(repairOrdersList);
                    _context.RepairOrderServices.AddRange(repairOrderServicesList);
                    _context.Jobs.AddRange(jobList);
                    _context.Repairs.AddRange(repairList);
                    _context.JobParts.AddRange(jobPartsList);
                    _context.JobTechnicians.AddRange(jobTechniciansList);
                    _context.Quotations.AddRange(quotationsList);
                    _context.QuotationServices.AddRange(quotationServicesList);
                    _context.QuotationServiceParts.AddRange(quotationServicePartsList);

                    await _context.SaveChangesAsync();

                    // clear lists to free memory
                    repairOrdersList.Clear();
                    repairOrderServicesList.Clear();
                    jobList.Clear();
                    repairList.Clear();
                    jobPartsList.Clear();
                    jobTechniciansList.Clear();
                    quotationsList.Clear();
                    quotationServicesList.Clear();
                    quotationServicePartsList.Clear();

                    Console.WriteLine($"Seeded {createdOrders} repair orders so far...");
                }

                // safety break if loop unexpectedly long
                if (createdOrders >= totalOrdersTarget) break;
            }

            Console.WriteLine($"Bulk seeding finished - total repair orders created: {createdOrders}");
        }

    }

}
