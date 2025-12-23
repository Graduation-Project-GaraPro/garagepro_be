using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Enums;
using BusinessObject.InspectionAndRepair;
using BusinessObject.ResultExcelReads;
using BusinessObject.Roles;
using BusinessObject.Vehicles;
using DataAccessLayer;
using Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Services.EmailSenders;
using Services.GeocodingServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Services.ExcelImportSerivces
{
    public class MasterDataImportService : IMasterDataImportService
    {
        private readonly MyAppDbContext _context;
        private readonly ILogger<MasterDataImportService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly List<ImportErrorDetail> _errors = new();
        private readonly IGeocodingService _geocodingService;
        private readonly IEmailSender _emailSender;
        // Caches
        private Dictionary<string, ServiceCategory> _categoryCache = null!;
        private Dictionary<string, Service> _serviceCache = null!;
        private Dictionary<string, BranchService> _branchServiceCache;
        private Dictionary<string, PartCategory> _partCategoryCache = null!;
        private Dictionary<string, Part> _partCache = null!;
        private Dictionary<string, ServicePartCategory> _spcCache = null!;
        private Dictionary<string, Branch> _branchCache = null!;
        private Dictionary<string, OperatingHour> _operatingHourCache = null!;
        private Dictionary<string, ApplicationUser> _staffCache = null!;

        private Dictionary<string, VehicleBrand> _brandCache = null!;
        private Dictionary<string, VehicleModel> _modelCache = null!;
        private Dictionary<string, List<PartCategory>> _partCategoriesByNameCache = null!;
        private Dictionary<string, PartInventory> _partInventoryCache = null!;

        private Dictionary<string, PartInventory> _inventoryCache;

        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new(@"^[0-9]{8,15}$", RegexOptions.Compiled);
        public MasterDataImportService(
            MyAppDbContext context,
            ILogger<MasterDataImportService> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IGeocodingService geocodingService,
            IEmailSender emailSender
            )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _geocodingService = geocodingService;
            _emailSender = emailSender;
        }

        public async Task<ImportResult> ImportFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ImportResult.Fail("File Not valid.");

            ExcelPackage.License.SetNonCommercialPersonal("Garage Pro");
            await InitCachesAsync();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);

            var templateErrors = ValidateTemplate(package);
            if (templateErrors.Any())
                return ImportResult.Fail("Excel template is invalid. Please fix the template and try again.", templateErrors);

            var staffWelcomeEmails = new List<StaffWelcomeEmailInfo>();

            await using var tx = await _context.Database.BeginTransactionAsync(); 

            try
            {
                await ImportBranchesAsync(package);
                await ImportOperatingHoursAsync(package);
                await ImportParentCategoriesAsync(package);
                await ImportServiceCategoriesAsync(package);
                await ImportServicesAsync(package);
                await ImportPartCategoriesAsync(package);
                await ImportPartsAsync(package);
                await ImportStaffAsync(package, staffWelcomeEmails);

                if (_errors.Any())
                {
                    await tx.RollbackAsync(); 
                    return ImportResult.Fail(
                        "Import failed because one or more errors were found. No data has been saved.",
                        _errors);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync(); 
                await SendStaffWelcomeEmailsAsync(staffWelcomeEmails);
                return ImportResult.Ok("Import master data Success.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error When import Excel master data");
                return ImportResult.Fail($"Import Excel Fail: {ex.Message}");
            }
        }


        #region Init Caches

        private async Task InitCachesAsync()
        {
            var serviceCategories = await _context.ServiceCategories.AsNoTracking().ToListAsync();
            var services = await _context.Services.AsNoTracking().ToListAsync();
            var branchServices = await _context.BranchServices.AsNoTracking().ToListAsync();

            var partCategories = await _context.PartCategories.AsNoTracking().ToListAsync();
            var parts = await _context.Parts.AsNoTracking().ToListAsync();
            var servicePartCats = await _context.ServicePartCategories.AsNoTracking().ToListAsync();

            var branches = await _context.Branches.AsNoTracking().ToListAsync();
            var operatingHours = await _context.OperatingHours.AsNoTracking().ToListAsync();

            
            var users = await _userManager.Users.AsNoTracking().ToListAsync();

            var brands = await _context.VehicleBrands.AsNoTracking().ToListAsync();
            var models = await _context.VehicleModels.AsNoTracking().ToListAsync();

            

            _branchCache = branches
                .GroupBy(b => Normalize(b.BranchName))
                .ToDictionary(g => g.Key, g => g.First());

            _operatingHourCache = operatingHours
                .GroupBy(o => GetOperatingHourKey(o.BranchId, o.DayOfWeek))
                .ToDictionary(g => g.Key, g => g.First());

            _staffCache = users
                .GroupBy(u => Normalize(u.UserName))
                .ToDictionary(g => g.Key, g => g.First());

            _categoryCache = serviceCategories
                .GroupBy(c => GetCategoryKey(c.CategoryName, c.ParentServiceCategoryId))
                .ToDictionary(g => g.Key, g => g.First());

            _serviceCache = services
                .GroupBy(s => Normalize(s.ServiceName))
                .ToDictionary(g => g.Key, g => g.First());

            _branchServiceCache = branchServices
                .GroupBy(bs => GetBranchServiceKey(bs.BranchId, bs.ServiceId))
                .ToDictionary(g => g.Key, g => g.First());

            _brandCache = brands
                .GroupBy(b => GetBrandKey(b.BrandName))
                .ToDictionary(g => g.Key, g => g.First());

            _modelCache = models
                .GroupBy(m => GetModelKey(m.BrandID, m.ModelName))
                .ToDictionary(g => g.Key, g => g.First());

            _partCategoryCache = partCategories
                .GroupBy(pc => GetPartCategoryKey(pc.ModelId, pc.CategoryName))
                .ToDictionary(g => g.Key, g => g.First());

            _partCategoriesByNameCache = partCategories
                .GroupBy(pc => Normalize(pc.CategoryName))
                .ToDictionary(g => g.Key, g => g.ToList());

            _partCache = parts
                .GroupBy(p => GetPartKey(p.PartCategoryId, p.Name))
                .ToDictionary(g => g.Key, g => g.First());

            
            _inventoryCache = await _context.PartInventories
                .AsNoTracking()
                .ToDictionaryAsync(
                    i => $"{i.PartId:N}|{i.BranchId:N}",
                    i => i
                );

            _spcCache = servicePartCats
                .GroupBy(x => GetSpcKey(x.ServiceId, x.PartCategoryId))
                .ToDictionary(g => g.Key, g => g.First());
        }


        #endregion

        #region Import Sheets




        private async Task ImportBranchesAsync(ExcelPackage package)
        {
            const string sheetName = "Branch";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            // === Pre-scan duplicates ===
            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 10)) continue;

                var name = ws.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var keyScan = Normalize(name);
                if (nameCounts.ContainsKey(keyScan))
                    nameCounts[keyScan]++;
                else
                    nameCounts[keyScan] = 1;
            }

            // === Main import ===
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 10))
                    continue;

                bool hasError = false;

                var branchName = ws.Cells[row, 1].Text?.Trim(); // A
                var phone = ws.Cells[row, 2].Text?.Trim(); // B
                var email = ws.Cells[row, 3].Text?.Trim(); // C
                var street = ws.Cells[row, 4].Text?.Trim(); // D
                var commune = ws.Cells[row, 5].Text?.Trim(); // E
                var province = ws.Cells[row, 6].Text?.Trim(); // F
                var description = ws.Cells[row, 7].Text?.Trim(); // G
                var isActiveTxt = ws.Cells[row, 8].Text?.Trim(); // H
                var arrivalTxt = ws.Cells[row, 9].Text?.Trim(); // I
                var maxBookTxt = ws.Cells[row, 10].Text?.Trim(); // J

               

                if (string.IsNullOrWhiteSpace(branchName))
                {
                    AddError(sheetName, "Branch Name is required.", row, "A", "BRANCH_NAME_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    AddError(sheetName, "Phone Number is required.", row, "B", "BRANCH_PHONE_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    AddError(sheetName, "Email is required.", row, "C", "BRANCH_EMAIL_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(street))
                {
                    AddError(sheetName, "Street is required.", row, "D", "BRANCH_STREET_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(commune))
                {
                    AddError(sheetName, "Commune is required.", row, "E", "BRANCH_COMMUNE_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(province))
                {
                    AddError(sheetName, "Province is required.", row, "F", "BRANCH_PROVINCE_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    AddError(sheetName, "Description is required.", row, "G", "BRANCH_DESCRIPTION_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(isActiveTxt))
                {
                    AddError(sheetName, "'Is Active' is required.", row, "H", "BRANCH_IS_ACTIVE_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(arrivalTxt))
                {
                    AddError(sheetName, "'Arrival Window Minutes' is required.", row, "I", "BRANCH_ARRIVAL_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(maxBookTxt))
                {
                    AddError(sheetName, "'Max Bookings Per Window' is required.", row, "J", "BRANCH_MAX_BOOKINGS_REQUIRED");
                    hasError = true;
                }

                // Nếu thiếu required field thì khỏi check tiếp
                if (hasError)
                    continue;

                var key = Normalize(branchName!);

                // Duplicate check
                if (nameCounts.TryGetValue(key, out var count) && count > 1)
                {
                    AddError(
                        sheetName,
                        $"Duplicate branch name '{branchName}' found in the sheet.",
                        row,
                        "A",
                        "BRANCH_DUPLICATE_NAME"
                    );
                    hasError = true;
                }

               

                int arrival = 0;
                if (!int.TryParse(arrivalTxt, out arrival))
                {
                    AddError(
                        sheetName,
                        $"Invalid integer for 'Arrival Window Minutes': '{arrivalTxt}'.",
                        row,
                        "I",
                        "BRANCH_INVALID_ARRIVAL_WINDOW"
                    );
                    hasError = true;
                }

                int maxBooking = 0;
                if (!int.TryParse(maxBookTxt, out maxBooking))
                {
                    AddError(
                        sheetName,
                        $"Invalid integer for 'Max Bookings Per Window': '{maxBookTxt}'.",
                        row,
                        "J",
                        "BRANCH_INVALID_MAX_BOOKINGS"
                    );
                    hasError = true;
                }

                // === Boolean validation ===
                bool isActive = true;
                if (!bool.TryParse(isActiveTxt, out isActive))
                {
                    AddError(
                        sheetName,
                        $"Invalid boolean value for 'Is Active': '{isActiveTxt}'. Expected 'true' or 'false'.",
                        row,
                        "H",
                        "BRANCH_INVALID_IS_ACTIVE"
                    );
                    hasError = true;
                }

                // Phone format
                if (!PhoneRegex.IsMatch(phone!))
                {
                    AddError(
                        sheetName,
                        $"Invalid phone number format: '{phone}'. Only digits allowed (8–15 digits).",
                        row,
                        "B",
                        "BRANCH_INVALID_PHONE"
                    );
                    hasError = true;
                }

                // Email format
                if (!EmailRegex.IsMatch(email!))
                {
                    AddError(
                        sheetName,
                        $"Invalid email format: '{email}'.",
                        row,
                        "C",
                        "BRANCH_INVALID_EMAIL"
                    );
                    hasError = true;
                }

                if (hasError)
                    continue;

                // === Build address for geocoding ===
                var address = $"{street} {commune} {province}".Trim();

                double latitude = 0;
                double longitude = 0;
                string formattedAddress = address;

                // === Call geocoding API ===
                try
                {
                    var geo = await _geocodingService.GetCoordinatesAsync(address);
                    latitude = geo.lat;
                    longitude = geo.lng;
                    formattedAddress = geo.formattedAddress;
                }
                catch (Exception ex)
                {
                    AddError(
                        sheetName,
                        $"Failed to geocode address '{address}': {ex.Message}",
                        row,
                        "D-F",
                        "BRANCH_GEOCODING_FAILED"
                    );
                    continue;
                }

                // Default values if 0
                if (arrival == 0) arrival = 30;
                if (maxBooking == 0) maxBooking = 6;

               
                if (!_branchCache.TryGetValue(key, out var branch))
                {
                    branch = new Branch
                    {
                        BranchId = Guid.NewGuid(),
                        BranchName = branchName!,
                        PhoneNumber = phone,
                        Email = email,
                        Street = street,
                        Commune = commune,
                        Province = province,
                        Latitude = latitude,
                        Longitude = longitude,
                       
                        Description = description,
                        IsActive = isActive,
                        ArrivalWindowMinutes = arrival,
                        MaxBookingsPerWindow = maxBooking,
                        
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Branches.Add(branch);
                    _branchCache[key] = branch;
                }
                else
                {
                    branch.PhoneNumber = phone ?? branch.PhoneNumber;
                    branch.Email = email ?? branch.Email;
                    branch.Street = street ?? branch.Street;
                    branch.Commune = commune ?? branch.Commune;
                    branch.Province = province ?? branch.Province;
                    branch.Latitude = latitude;
                    branch.Longitude = longitude;
                    
                    branch.Description = description ?? branch.Description;
                    branch.IsActive = isActive;
                    branch.ArrivalWindowMinutes = arrival;
                    branch.MaxBookingsPerWindow = maxBooking;
                   
                    branch.UpdatedAt = DateTime.UtcNow;

                    _context.Branches.Update(branch);
                }
            }
        }




        private async Task ImportStaffAsync(ExcelPackage package, List<StaffWelcomeEmailInfo> emailJobs)
        {
            const string sheetName = "Staff";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

           
            var userNameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var emailCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 7)) continue;

                
                var userNameScan = ws.Cells[row, 1].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(userNameScan))
                {
                    var key = Normalize(userNameScan);
                    if (userNameCounts.ContainsKey(key))
                        userNameCounts[key]++;
                    else
                        userNameCounts[key] = 1;
                }

                
                var emailScan = ws.Cells[row, 2].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(emailScan))
                {
                    var emailKey = Normalize(emailScan);
                    if (emailCounts.ContainsKey(emailKey))
                        emailCounts[emailKey]++;
                    else
                        emailCounts[emailKey] = 1;
                }
            }

            
            var duplicateUserNames = new HashSet<string>(
                userNameCounts.Where(x => x.Value > 1).Select(x => x.Key),
                StringComparer.OrdinalIgnoreCase
            );

            var duplicateEmails = new HashSet<string>(
                emailCounts.Where(x => x.Value > 1).Select(x => x.Key),
                StringComparer.OrdinalIgnoreCase
            );

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 7))
                    continue;

                bool hasError = false;

                var userName = ws.Cells[row, 1].Text?.Trim();
                var email = ws.Cells[row, 2].Text?.Trim();
                var phone = ws.Cells[row, 3].Text?.Trim();
                var fullName = ws.Cells[row, 4].Text?.Trim();
                var roleName = ws.Cells[row, 5].Text?.Trim();
                var branchName = ws.Cells[row, 6].Text?.Trim();
                var isActiveTx = ws.Cells[row, 7].Text?.Trim();

                

                
                if (string.IsNullOrWhiteSpace(userName))
                {
                    AddError(
                        sheetName,
                        "User Name is required.",
                        row,
                        "A",
                        "STAFF_USERNAME_REQUIRED"
                    );
                    hasError = true;
                }

                
                if (string.IsNullOrWhiteSpace(email))
                {
                    AddError(
                        sheetName,
                        "Email is required.",
                        row,
                        "B",
                        "STAFF_EMAIL_REQUIRED"
                    );
                    hasError = true;
                }

                
                if (string.IsNullOrWhiteSpace(phone))
                {
                    AddError(
                        sheetName,
                        "Phone Number is required.",
                        row,
                        "C",
                        "STAFF_PHONE_REQUIRED"
                    );
                    hasError = true;
                }

                
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    AddError(
                        sheetName,
                        "Full Name is required.",
                        row,
                        "D",
                        "STAFF_FULLNAME_REQUIRED"
                    );
                    hasError = true;
                }

                // Role required
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    AddError(
                        sheetName,
                        "Role is required.",
                        row,
                        "E",
                        "STAFF_ROLE_REQUIRED"
                    );
                    hasError = true;
                }

              
                if (string.IsNullOrWhiteSpace(branchName))
                {
                    AddError(
                        sheetName,
                        "Branch is required.",
                        row,
                        "F",
                        "STAFF_BRANCH_REQUIRED"
                    );
                    hasError = true;
                }

                
                if (string.IsNullOrWhiteSpace(isActiveTx))
                {
                    AddError(
                        sheetName,
                        "'Is Active' is required.",
                        row,
                        "G",
                        "STAFF_IS_ACTIVE_REQUIRED"
                    );
                    hasError = true;
                }

               
                if (!string.IsNullOrWhiteSpace(roleName) &&
                    !string.Equals(roleName, "Manager", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(roleName, "Technician", StringComparison.OrdinalIgnoreCase))
                {
                    AddError(
                        sheetName,
                        $"Invalid role '{roleName}'. Only 'Manager' and 'Technician' are allowed.",
                        row,
                        "E",
                        "STAFF_INVALID_ROLE"
                    );
                    hasError = true;
                }

                string uKey = null!;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    uKey = Normalize(userName);

                   
                    if (duplicateUserNames.Contains(uKey))
                    {
                        AddError(
                            sheetName,
                            $"Duplicate user name '{userName}' found in the sheet.",
                            row,
                            "A",
                            "STAFF_DUPLICATE_USERNAME"
                        );
                        hasError = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var eKey = Normalize(email);
                    if (duplicateEmails.Contains(eKey))
                    {
                        AddError(
                            sheetName,
                            $"Duplicate email '{email}' found in the sheet.",
                            row,
                            "B",
                            "STAFF_DUPLICATE_EMAIL"
                        );
                        hasError = true;
                    }
                }

                // Email format
                if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email))
                {
                    AddError(
                        sheetName,
                        $"Invalid email format: '{email}'.",
                        row,
                        "B",
                        "STAFF_INVALID_EMAIL"
                    );
                    hasError = true;
                }

                // Phone format
                if (!string.IsNullOrWhiteSpace(phone) && !PhoneRegex.IsMatch(phone))
                {
                    AddError(
                        sheetName,
                        $"Invalid phone number format: '{phone}'. Only digits allowed (8–15 digits).",
                        row,
                        "C",
                        "STAFF_INVALID_PHONE"
                    );
                    hasError = true;
                }

                
                bool isActive = true;
                if (!string.IsNullOrWhiteSpace(isActiveTx))
                {
                    if (!bool.TryParse(isActiveTx, out isActive))
                    {
                        AddError(
                            sheetName,
                            $"Invalid boolean value for 'Is Active': '{isActiveTx}'. Expected 'true' or 'false'.",
                            row,
                            "G",
                            "STAFF_INVALID_IS_ACTIVE"
                        );
                        hasError = true;
                    }
                }

                
                Guid? branchId = null;
                if (!string.IsNullOrWhiteSpace(branchName))
                {
                    var bKey = Normalize(branchName);
                    if (_branchCache.TryGetValue(bKey, out var branch))
                    {
                        branchId = branch.BranchId;
                    }
                    else
                    {
                        AddError(
                            sheetName,
                            $"Branch '{branchName}' does not exist in the Branch sheet.",
                            row,
                            "F",
                            "STAFF_BRANCH_NOT_FOUND"
                        );
                        hasError = true;
                    }
                }

                if (hasError)
                    continue;

                ApplicationUser user;

                var (firstName, lastName) = SplitFullName(fullName!);
                

                // Create or update user
                if (!_staffCache.TryGetValue(uKey, out user!))
                {
                    user = new ApplicationUser
                    {
                        UserName = userName!,
                        Email = email,
                        PhoneNumber = phone,
                        FirstName = firstName,
                        LastName = lastName,
                        BranchId = branchId,
                        IsActive = isActive,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user, "Admin123!");

                    if (!createResult.Succeeded)
                    {
                        AddError(
                            sheetName,
                            $"Failed to create user '{userName}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}.",
                            row,
                            null,
                            "STAFF_CREATE_USER_FAILED"
                        );
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        emailJobs.Add(new StaffWelcomeEmailInfo
                        {
                            Email = user.Email,
                            FullName = $"{user.FirstName} {user.LastName}".Trim(),
                            UserName = user.UserName!
                        });
                    }

                    _staffCache[uKey] = user;
                }
                else
                {                 
                        AddError(
                            sheetName,
                            $"User '{userName}' already exists.",
                            row,
                            null,
                            "STAFF_UPDATE_USER_FAILED"
                        );
                        continue;                   
                }

                // Assign role
                if (!await _roleManager.RoleExistsAsync(roleName!))
                {
                    AddError(
                        sheetName,
                        $"Role '{roleName}' does not exist.",
                        row,
                        "E",
                        "STAFF_ROLE_NOT_FOUND"
                    );
                }
                else
                {
                    if (!await _userManager.IsInRoleAsync(user, roleName!))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, roleName!);
                        if (!roleResult.Succeeded)
                        {
                            AddError(
                                sheetName,
                                $"Failed to assign role '{roleName}' to user '{userName}': {string.Join("; ", roleResult.Errors.Select(e => e.Description))}.",
                                row,
                                "E",
                                "STAFF_ASSIGN_ROLE_FAILED"
                            );
                        }
                    }
                }

                // Technician record
                if (string.Equals(roleName, "Technician", StringComparison.OrdinalIgnoreCase))
                {
                    var tech = await _context.Technicians
                        .FirstOrDefaultAsync(t => t.UserId == user.Id);

                    if (tech == null)
                    {
                        tech = new Technician
                        {
                            TechnicianId = Guid.NewGuid(),
                            UserId = user.Id,
                            Quality = 0,
                            Speed = 0,
                            Efficiency = 0,
                            Score = 0
                        };

                        _context.Technicians.Add(tech);
                    }
                    // else: update logic if needed
                }
            }
        }



        private async Task ImportOperatingHoursAsync(ExcelPackage package)
        {
            const string sheetName = "BranchOperatingHour";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            // Track which days each branch has operating hours for.
            // Only branches that exist in _branchCache and appear in this sheet will be added.
            var branchDays = new Dictionary<Guid, HashSet<DayOfWeekEnum>>();

            for (int row = 2; row <= rowCount; row++)
            {
                var branchName = ws.Cells[row, 1].Text?.Trim();
                var dayName = ws.Cells[row, 2].Text?.Trim();
                var isOpenText = ws.Cells[row, 3].Text?.Trim();
                var openTime = ws.Cells[row, 4].Text?.Trim();
                var closeTime = ws.Cells[row, 5].Text?.Trim();

                // Branch name required
                if (string.IsNullOrWhiteSpace(branchName))
                {
                    AddError(
                        sheetName,
                        "Branch Name cannot be empty.",
                        row,
                        "A",
                        "OPERATING_HOUR_BRANCH_NAME_REQUIRED"
                    );
                    continue;
                }

                // Day of week required
                if (string.IsNullOrWhiteSpace(dayName))
                {
                    AddError(
                        sheetName,
                        "Day of week cannot be empty.",
                        row,
                        "B",
                        "OPERATING_HOUR_DAY_REQUIRED"
                    );
                    continue;
                }

                var branchKey = Normalize(branchName);

                if (!_branchCache.TryGetValue(branchKey, out var branch))
                {
                    // Ignore branches that do not exist in the Branch sheet
                    AddError(
                        sheetName,
                        $"Branch '{branchName}' does not exist in the Branch sheet.",
                        row,
                        "A",
                        "OPERATING_HOUR_BRANCH_NOT_FOUND"
                    );
                    continue;
                }

                // Parse day of week
                if (!Enum.TryParse(dayName, true, out DayOfWeekEnum dayOfWeek))
                {
                    AddError(
                        sheetName,
                        $"Invalid day of week: '{dayName}'.",
                        row,
                        "B",
                        "OPERATING_HOUR_INVALID_DAY"
                    );
                    continue;
                }

                // Parse IsOpen
                bool isOpen = false;
                if (!string.IsNullOrWhiteSpace(isOpenText))
                {
                    if (!bool.TryParse(isOpenText, out isOpen))
                    {
                        AddError(
                            sheetName,
                            $"Invalid boolean value for 'Is Open': '{isOpenText}'. Expected 'true' or 'false'.",
                            row,
                            "C",
                            "OPERATING_HOUR_INVALID_IS_OPEN"
                        );
                        continue;
                    }
                }

                TimeSpan? ot = null;
                TimeSpan? ct = null;

                if (isOpen)
                {
                    // Open time required and must be a valid TimeSpan
                    if (string.IsNullOrWhiteSpace(openTime) ||
                        !TimeSpan.TryParse(openTime, out var t1))
                    {
                        AddError(
                            sheetName,
                            $"Invalid time format for 'Open Time': '{openTime}'. Expected HH:mm.",
                            row,
                            "D",
                            "OPERATING_HOUR_INVALID_OPEN_TIME"
                        );
                        continue;
                    }

                    // Close time required and must be a valid TimeSpan
                    if (string.IsNullOrWhiteSpace(closeTime) ||
                        !TimeSpan.TryParse(closeTime, out var t2))
                    {
                        AddError(
                            sheetName,
                            $"Invalid time format for 'Close Time': '{closeTime}'. Expected HH:mm.",
                            row,
                            "E",
                            "OPERATING_HOUR_INVALID_CLOSE_TIME"
                        );
                        continue;
                    }

                    if (t2 <= t1)
                    {
                        AddError(
                            sheetName,
                            "Close Time must be later than Open Time.",
                            row,
                            "E",
                            "OPERATING_HOUR_INVALID_TIME_RANGE"
                        );
                        continue;
                    }

                    ot = t1;
                    ct = t2;
                }

                var key = GetOperatingHourKey(branch.BranchId, dayOfWeek);

                if (!_operatingHourCache.TryGetValue(key, out var hour))
                {
                    hour = new OperatingHour
                    {
                        Id = Guid.NewGuid(),
                        BranchId = branch.BranchId,
                        DayOfWeek = dayOfWeek,
                        IsOpen = isOpen,
                        OpenTime = ot,
                        CloseTime = ct
                    };

                    _context.OperatingHours.Add(hour);
                    _operatingHourCache[key] = hour;
                }
                else
                {
                    hour.IsOpen = isOpen;
                    hour.OpenTime = ot;
                    hour.CloseTime = ct;

                    _context.OperatingHours.Update(hour);
                }

                // Mark that this branch has this day configured
                if (!branchDays.TryGetValue(branch.BranchId, out var days))
                {
                    days = new HashSet<DayOfWeekEnum>();
                    branchDays[branch.BranchId] = days;
                }

                days.Add(dayOfWeek);
            }

            // Required 7 days: Monday to Sunday
            var requiredDays = new[]
            {
                DayOfWeekEnum.Monday,
                DayOfWeekEnum.Tuesday,
                DayOfWeekEnum.Wednesday,
                DayOfWeekEnum.Thursday,
                DayOfWeekEnum.Friday,
                DayOfWeekEnum.Saturday,
                DayOfWeekEnum.Sunday
            };

            // Only check branches that appear in this OperatingHour sheet (branchDays keys)
            foreach (var kvp in branchDays)
            {
                var branchId = kvp.Key;
                var daysForBranch = kvp.Value;

                var missingDays = requiredDays
                    .Where(d => !daysForBranch.Contains(d))
                    .ToList();

                if (missingDays.Count == 0)
                    continue;

                // Find the branch object to get the name (from _branchCache values)
                var branch = _branchCache.Values.FirstOrDefault(b => b.BranchId == branchId);
                var branchName = branch?.BranchName ?? branchId.ToString();

                var missingDaysText = string.Join(", ", missingDays);

                // Single aggregated error per branch
                AddError(
                    sheetName,
                    $"Branch '{branchName}' is missing operating hours for the following days: {missingDaysText}.",
                    null,
                    null,
                    "OPERATING_HOUR_MISSING_DAYS"
                );
            }
        }




        private async Task ImportParentCategoriesAsync(ExcelPackage package)
        {
            const string sheetName = "ParentCategory";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            // Pre-scan duplicate parent category names
            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 3))
                    continue;

                var parentNameScan = ws.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(parentNameScan))
                    continue;

                var keyScan = GetCategoryKey(parentNameScan, null);
                if (nameCounts.ContainsKey(keyScan))
                    nameCounts[keyScan]++;
                else
                    nameCounts[keyScan] = 1;
            }

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 3))
                    continue;

                bool hasError = false;

                var parentName = ws.Cells[row, 1].Text?.Trim(); // A
                var description = ws.Cells[row, 2].Text?.Trim(); // B
                var isActiveTxt = ws.Cells[row, 3].Text?.Trim(); // C

                // Parent name required
                if (string.IsNullOrWhiteSpace(parentName))
                {
                    AddError(
                        sheetName,
                        "Parent Category Name is required.",
                        row,
                        "A",
                        "PARENT_CATEGORY_NAME_REQUIRED"
                    );
                    continue; // không cần xử lý thêm row này
                }

                var key = GetCategoryKey(parentName, null);

                // Duplicate name check
                if (nameCounts.TryGetValue(key, out var count) && count > 1)
                {
                    AddError(
                        sheetName,
                        $"Duplicate parent category name '{parentName}' found in the sheet.",
                        row,
                        "A",
                        "PARENT_CATEGORY_DUPLICATE_NAME"
                    );
                    hasError = true;
                }

                // Parse IsActive
                bool isActive = true;
                if (!string.IsNullOrWhiteSpace(isActiveTxt))
                {
                    if (!bool.TryParse(isActiveTxt, out isActive))
                    {
                        AddError(
                            sheetName,
                            $"Invalid boolean value for 'Is Active': '{isActiveTxt}'. Expected 'true' or 'false'.",
                            row,
                            "C",
                            "PARENT_CATEGORY_INVALID_IS_ACTIVE"
                        );
                        hasError = true;
                    }
                }

                if (hasError)
                    continue;

               
                if (!_categoryCache.TryGetValue(key, out var parentCat))
                {
                    parentCat = new ServiceCategory
                    {
                        ServiceCategoryId = Guid.NewGuid(),
                        CategoryName = parentName,
                        ParentServiceCategoryId = null,
                        Description = description,
                        IsActive = isActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ServiceCategories.Add(parentCat);
                    _categoryCache[key] = parentCat;
                }
                else
                {
                    parentCat.Description = description ?? parentCat.Description;
                    parentCat.IsActive = isActive;
                    parentCat.UpdatedAt = DateTime.UtcNow;

                    _context.ServiceCategories.Update(parentCat);
                }
            }
        }


        private async Task ImportServiceCategoriesAsync(ExcelPackage package)
        {
            const string sheetName = "ServiceCategory";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            // === Pre-scan duplicate CategoryName ===
            var categoryNameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 4))
                    continue;

                var categoryNameScan = ws.Cells[row, 2].Text?.Trim();
                if (string.IsNullOrWhiteSpace(categoryNameScan))
                    continue;

                var keyScan = Normalize(categoryNameScan);
                if (categoryNameCounts.ContainsKey(keyScan))
                    categoryNameCounts[keyScan]++;
                else
                    categoryNameCounts[keyScan] = 1;
            }

            var duplicateCategoryNames = new HashSet<string>(
                categoryNameCounts.Where(x => x.Value > 1).Select(x => x.Key),
                StringComparer.OrdinalIgnoreCase
            );

            // === MAIN IMPORT ===
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 4))
                    continue;

                bool hasError = false;

                var parentName = ws.Cells[row, 1].Text?.Trim(); // A
                var categoryName = ws.Cells[row, 2].Text?.Trim(); // B
                var description = ws.Cells[row, 3].Text?.Trim(); // C
                var isActiveTxt = ws.Cells[row, 4].Text?.Trim(); // D

                // ==== REQUIRED FIELDS ====
                if (string.IsNullOrWhiteSpace(parentName))
                {
                    AddError(sheetName, "Parent Category Name is required.", row, "A", "SERVICE_CAT_PARENT_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    AddError(sheetName, "Category Name is required.", row, "B", "SERVICE_CAT_NAME_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    AddError(sheetName, "Description is required.", row, "C", "SERVICE_CAT_DESC_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(isActiveTxt))
                {
                    AddError(sheetName, "'Is Active' is required.", row, "D", "SERVICE_CAT_IS_ACTIVE_REQUIRED");
                    hasError = true;
                }

                if (hasError) continue;

                // ==== DUPLICATE CHECK ====
                var normalizedKey = Normalize(categoryName!);
                if (duplicateCategoryNames.Contains(normalizedKey))
                {
                    AddError(
                        sheetName,
                        $"Duplicate category name '{categoryName}' found in the sheet.",
                        row,
                        "B",
                        "SERVICE_CAT_DUPLICATE"
                    );
                    hasError = true;
                }

               
                bool isActive = true;
                if (!bool.TryParse(isActiveTxt, out isActive))
                {
                    AddError(
                        sheetName,
                        $"Invalid boolean value for 'Is Active': '{isActiveTxt}'. Expected 'true' or 'false'.",
                        row,
                        "D",
                        "SERVICE_CAT_INVALID_IS_ACTIVE"
                    );
                    hasError = true;
                }

                if (hasError) continue;

                // ==== VALIDATE Parent Category exists in system ====
                var parentKey = GetCategoryKey(parentName!, null);

                if (!_categoryCache.TryGetValue(parentKey, out var parentCat))
                {
                    AddError(
                        sheetName,
                        $"Parent Category '{parentName}' does not exist in the system.",
                        row,
                        "A",
                        "SERVICE_CAT_PARENT_NOT_FOUND"
                    );
                    continue;
                }

                var parentId = parentCat.ServiceCategoryId;

                // ==== CHILD CATEGORY PROCESS ====
                var key = GetCategoryKey(categoryName!, parentId);

                if (!_categoryCache.TryGetValue(key, out var cat))
                {
                    cat = new ServiceCategory
                    {
                        ServiceCategoryId = Guid.NewGuid(),
                        CategoryName = categoryName!,
                        ParentServiceCategoryId = parentId,
                        Description = description,
                        IsActive = isActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ServiceCategories.Add(cat);
                    _categoryCache[key] = cat;
                }
                else
                {
                    cat.Description = description ?? cat.Description;
                    cat.IsActive = isActive;
                    cat.UpdatedAt = DateTime.UtcNow;

                    _context.ServiceCategories.Update(cat);
                }
            }
        }

        private async Task ImportServicesAsync(ExcelPackage package)
        {
            const string sheetName = "Service";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;
            int colCount = ws.Dimension.Columns;

            for (int row = 2; row <= rowCount; row++)
            {
                // Chỉ cần check 6 cột bắt buộc (branch là động/tuỳ chọn)
                if (IsRowEmpty(ws, row, 6))
                    continue;

                bool hasError = false;

                var categoryName = ws.Cells[row, 1].Text?.Trim(); // A
                var serviceName = ws.Cells[row, 2].Text?.Trim(); // B
                var description = ws.Cells[row, 3].Text?.Trim(); // C
                var priceText = ws.Cells[row, 4].Text?.Trim(); // D
                var durationText = ws.Cells[row, 5].Text?.Trim(); // E
                var isAdvText = ws.Cells[row, 6].Text?.Trim(); // F

                // ===== Required fields =====
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    AddError(sheetName, "Service Category Name is required.", row, "A", "SERVICE_CATEGORY_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    AddError(sheetName, "Service Name is required.", row, "B", "SERVICE_NAME_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    AddError(sheetName, "Description is required.", row, "C", "SERVICE_DESCRIPTION_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(priceText))
                {
                    AddError(sheetName, "Price is required.", row, "D", "SERVICE_PRICE_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(durationText))
                {
                    AddError(sheetName, "Estimated Duration is required.", row, "E", "SERVICE_DURATION_REQUIRED");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(isAdvText))
                {
                    AddError(sheetName, "'Is Advanced' is required.", row, "F", "SERVICE_IS_ADVANCED_REQUIRED");
                    hasError = true;
                }

                if (hasError) continue;

                // ===== Parse numeric & boolean =====
                if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    AddError(sheetName, $"Invalid number format for 'Price': '{priceText}'.", row, "D", "SERVICE_INVALID_PRICE");
                    continue;
                }

                if (!decimal.TryParse(durationText, NumberStyles.Any, CultureInfo.InvariantCulture, out var duration))
                {
                    AddError(sheetName, $"Invalid number format for 'Estimated Duration': '{durationText}'.", row, "E", "SERVICE_INVALID_DURATION");
                    continue;
                }

                if (!bool.TryParse(isAdvText, out var isAdvanced))
                {
                    AddError(sheetName, $"Invalid boolean value for 'Is Advanced': '{isAdvText}'. Expected 'true' or 'false'.", row, "F", "SERVICE_INVALID_IS_ADVANCED");
                    continue;
                }

               
                Guid serviceCategoryId = Guid.Empty;
                var normCatName = Normalize(categoryName);
                var cat = _categoryCache.Values.FirstOrDefault(c => Normalize(c.CategoryName) == normCatName);
                if (cat == null)
                {
                    AddError(sheetName, $"Service Category '{categoryName}' does not exist in the system.", row, "A", "SERVICE_CATEGORY_NOT_FOUND");
                    continue;
                }
                serviceCategoryId = cat.ServiceCategoryId;

             
                var branchIds = new HashSet<Guid>();

                for (int col = 7; col <= colCount; col++)
                {
                    var header = ws.Cells[1, col].Text?.Trim();
                    
                    if (string.IsNullOrWhiteSpace(header) ||
                        !(header.StartsWith("Branch", StringComparison.OrdinalIgnoreCase) ||
                          header.StartsWith("BranchName", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var branchName = ws.Cells[row, col].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(branchName)) continue;

                    var bKey = Normalize(branchName);
                    if (_branchCache.TryGetValue(bKey, out var branch))
                    {
                        branchIds.Add(branch.BranchId); // tự loại trùng
                    }
                    else
                    {
                        AddError(
                            sheetName,
                            $"Branch '{branchName}' does not exist in the system.",
                            row,
                            ws.Cells[1, col].Address, 
                            "SERVICE_BRANCH_NOT_FOUND"
                        );
                        hasError = true;
                    }
                }

                if (hasError) continue;

               
                var sKey = Normalize(serviceName);
                if (!_serviceCache.TryGetValue(sKey, out var service))
                {
                    service = new Service
                    {
                        ServiceId = Guid.NewGuid(),
                        ServiceCategoryId = serviceCategoryId,
                        ServiceName = serviceName!,
                        Description = description,
                        Price = price,
                        EstimatedDuration = duration,
                        IsAdvanced = isAdvanced,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Services.Add(service);
                    _serviceCache[sKey] = service;
                }
                else
                {
                    service.ServiceCategoryId = serviceCategoryId;
                    service.Description = description ?? service.Description;
                    service.Price = price;
                    service.EstimatedDuration = duration;
                    service.IsAdvanced = isAdvanced;
                    service.UpdatedAt = DateTime.UtcNow;

                    _context.Services.Update(service);
                }

                // ===== Map Service ↔ nhiều Branch =====
                foreach (var branchId in branchIds)
                {
                    var bsKey = GetBranchServiceKey(branchId, service.ServiceId);

                    if (!_branchServiceCache.ContainsKey(bsKey))
                    {
                        var branchService = new BranchService
                        {
                            BranchId = branchId,
                            ServiceId = service.ServiceId
                        };

                        _context.BranchServices.Add(branchService);
                        _branchServiceCache[bsKey] = branchService;
                    }
                }
            }
        }



        private async Task ImportPartCategoriesAsync(ExcelPackage package)
        {
            const string sheetName = "PartCategory";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            var allModels = _modelCache.Values.ToList();

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 3)) continue;

                var categoryName = ws.Cells[row, 1].Text?.Trim();
                var description = ws.Cells[row, 2].Text?.Trim();
                var serviceName = ws.Cells[row, 3].Text?.Trim();

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    AddError(sheetName, "PartCategoryName is required.", row, "A", "PC_NAME_REQUIRED");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    AddError(sheetName, "ServiceName is required.", row, "C", "PC_SERVICE_REQUIRED");
                    continue;
                }

                
                var sKey = Normalize(serviceName);
                if (!_serviceCache.TryGetValue(sKey, out var service))
                {
                    AddError(sheetName,
                        $"Service '{serviceName}' not found.",
                        row, "C", "PC_SERVICE_NOT_FOUND");
                    continue;
                }

                
                if (!service.IsAdvanced)
                {
                    var oldSpcs = _spcCache.Values
                        .Where(x => x.ServiceId == service.ServiceId)
                        .ToList();

                    foreach (var old in oldSpcs)
                    {
                        _context.ServicePartCategories.Remove(old);
                        _spcCache.Remove(GetSpcKey(old.ServiceId, old.PartCategoryId));
                    }
                }

                
                foreach (var model in allModels)
                {
                    var pcKey = GetPartCategoryKey(model.ModelID, categoryName);

                    if (!_partCategoryCache.TryGetValue(pcKey, out var partCategory))
                    {
                        partCategory = new PartCategory
                        {
                            LaborCategoryId = Guid.NewGuid(),
                            ModelId = model.ModelID,
                            CategoryName = categoryName,
                            Description = description,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PartCategories.Add(partCategory);
                        _partCategoryCache[pcKey] = partCategory;
                    }
                    else
                    {
                        // Update description nếu có
                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            partCategory.Description = description;
                            partCategory.UpdatedAt = DateTime.UtcNow;
                        }
                    }

                    // ===== MAP Service ↔ PartCategory =====
                    var spcKey = GetSpcKey(service.ServiceId, partCategory.LaborCategoryId);
                    if (_spcCache.ContainsKey(spcKey)) continue;

                    var spc = new ServicePartCategory
                    {
                        ServicePartCategoryId = Guid.NewGuid(),
                        ServiceId = service.ServiceId,
                        PartCategoryId = partCategory.LaborCategoryId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ServicePartCategories.Add(spc);
                    _spcCache[spcKey] = spc;
                }
            }
        }






        private async Task ImportPartsAsync(ExcelPackage package)
        {
            const string sheetName = "Part";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;
            int colCount = ws.Dimension.Columns;

            var excelPartDuplicateCheck = new HashSet<string>();

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 6)) continue;

                bool hasError = false;

                var brandName = ws.Cells[row, 1].Text?.Trim();
                var modelName = ws.Cells[row, 2].Text?.Trim();
                var categoryName = ws.Cells[row, 3].Text?.Trim();
                var partName = ws.Cells[row, 4].Text?.Trim();
                var priceText = ws.Cells[row, 5].Text?.Trim();
                var warrantyText = ws.Cells[row, 6].Text?.Trim();

                // ===== Required validation =====
                if (string.IsNullOrWhiteSpace(brandName)) { AddError(sheetName, "BrandName is required", row, "A", "REQ_BRAND"); hasError = true; }
                if (string.IsNullOrWhiteSpace(modelName)) { AddError(sheetName, "ModelName is required", row, "B", "REQ_MODEL"); hasError = true; }
                if (string.IsNullOrWhiteSpace(categoryName)) { AddError(sheetName, "PartCategoryName is required", row, "C", "REQ_CATEGORY"); hasError = true; }
                if (string.IsNullOrWhiteSpace(partName)) { AddError(sheetName, "PartName is required", row, "D", "REQ_PART"); hasError = true; }
                if (string.IsNullOrWhiteSpace(priceText)) { AddError(sheetName, "Price is required", row, "E", "REQ_PRICE"); hasError = true; }

                if (hasError) continue;

                // ===== Duplicate in Excel =====
                var excelDupKey = $"{Normalize(brandName)}|{Normalize(modelName)}|{Normalize(categoryName)}|{Normalize(partName)}";
                if (!excelPartDuplicateCheck.Add(excelDupKey))
                {
                    AddError(sheetName,
                        $"Duplicate PartName '{partName}' in same Category within Excel.",
                        row, "D", "DUP_PART_EXCEL");
                    continue;
                }

                // ===== Parse price =====
                if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    AddError(sheetName, $"Invalid price '{priceText}'", row, "E", "INVALID_PRICE");
                    continue;
                }

                int? warrantyMonths = null;
                if (!string.IsNullOrWhiteSpace(warrantyText))
                {
                    if (!int.TryParse(warrantyText, out var wm))
                    {
                        AddError(sheetName, $"Invalid WarrantyMonths '{warrantyText}'", row, "F", "INVALID_WARRANTY");
                        continue;
                    }
                    warrantyMonths = wm;
                }

                // ===== Brand =====
                var bKey = Normalize(brandName);
                if (!_brandCache.TryGetValue(bKey, out var brand))
                {
                    AddError(sheetName, $"Brand '{brandName}' not found", row, "A", "BRAND_NOT_FOUND");
                    continue;
                }

                // ===== Model =====
                var mKey = GetModelKey(brand.BrandID, Normalize(modelName));
                if (!_modelCache.TryGetValue(mKey, out var model))
                {
                    AddError(sheetName, $"Model '{modelName}' not found", row, "B", "MODEL_NOT_FOUND");
                    continue;
                }

                // ===== Category =====
                var pcKey = $"{model.ModelID:N}|{Normalize(categoryName)}";
                if (!_partCategoryCache.TryGetValue(pcKey, out var category))
                {
                    AddError(sheetName, $"PartCategory '{categoryName}' not found", row, "C", "CATEGORY_NOT_FOUND");
                    continue;
                }
                
                // ===== Part =====
                var pKey = $"{category.LaborCategoryId:N}|{Normalize(partName)}";
                if (!_partCache.TryGetValue(pKey, out var part))
                {
                    part = new Part
                    {
                        PartId = Guid.NewGuid(),
                        PartCategoryId = category.LaborCategoryId,
                        Name = partName,
                        Price = price,
                        WarrantyMonths = warrantyMonths,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Parts.Add(part);
                    _partCache[pKey] = part;
                }
                else
                {
                    part.Price = price;
                    part.WarrantyMonths = warrantyMonths;
                    part.UpdatedAt = DateTime.UtcNow;
                    _context.Parts.Update(part);

                }

                // ===== Inventory (dynamic BranchName_X + Stock_X) =====
                for (int col = 7; col <= colCount; col += 2)
                {

                    if (col + 1 > colCount)
                    {
                        AddError(sheetName,
                            "Missing Stock column for the last Branch column (expected Stock_n right after BranchName_n).",
                            1, ws.Cells[1, col].Address, "MISSING_STOCK_HEADER");

                        hasError = true;
                        break;
                    }

                    var branchName = ws.Cells[row, col].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(branchName)) continue;

                    var stockText = ws.Cells[row, col + 1].Text?.Trim();
                    if (!int.TryParse(stockText, out var stock))
                    {
                        AddError(sheetName, $"Invalid stock '{stockText}'", row, ws.Cells[1, col + 1].Address, "INVALID_STOCK");
                        continue;
                    }

                    var brKey = Normalize(branchName);
                    if (!_branchCache.TryGetValue(brKey, out var branch))
                    {
                        AddError(sheetName, $"Branch '{branchName}' not found", row, ws.Cells[1, col].Address, "BRANCH_NOT_FOUND");
                        continue;
                    }

                    var invKey = $"{part.PartId:N}|{branch.BranchId:N}";

                    if (!_inventoryCache.TryGetValue(invKey, out var inventory))
                    {
                        inventory = new PartInventory
                        {
                            PartInventoryId = Guid.NewGuid(),
                            PartId = part.PartId,
                            BranchId = branch.BranchId,
                            Stock = stock,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PartInventories.Add(inventory);
                        _inventoryCache[invKey] = inventory;
                    }
                    else
                    {
                        inventory.Stock = stock;
                        inventory.UpdatedAt = DateTime.UtcNow;
                        _context.PartInventories.Update(inventory);
                    }




                }
            }
        }





        


        private List<ImportErrorDetail> ValidateTemplate(ExcelPackage package)
        {
            var errors = new List<ImportErrorDetail>();

            var requiredSheets = new Dictionary<string, string[]>
            {
                ["Branch"] = new[]
                {
                        "BranchName","PhoneNumber","Email","Street","Commune",
                        "Province","Description",
                        "IsActive","ArrivalWindowMinutes","MaxBookingsPerWindow"
                    },
                ["BranchOperatingHour"] = new[]
                            {
                        "BranchName","DayOfWeek","IsOpen","OpenTime","CloseTime"
                    },
                ["Staff"] = new[]
                            {
                        "UserName","Email","PhoneNumber","FullName","Role","BranchName","IsActive"
                    },
                ["ParentCategory"] = new[]
                            {
                        "ParentCategoryName","Description","IsActive"
                    },
                ["ServiceCategory"] = new[]
                            {
                        "ParentCategoryName","CategoryName","Description","IsActive"
                    },
                ["Service"] = new[]
                            {
                        "CategoryName","ServiceName","Description","Price",
                        "EstimatedDuration","IsAdvanced"
                    },
                ["PartCategory"] = new[]
                {
                    "PartCategoryName","Description","ServiceName"
                },

                ["Part"] = new[]
                {
                    "BrandName","ModelName","PartCategoryName","PartName","Price","WarrantyMonths"
                }
                ,
                ["Part"] = new[]
                {
                    "BrandName","ModelName","PartCategoryName","PartName","Price","WarrantyMonths"
                }
            };

            foreach (var kv in requiredSheets)
            {
                var sheetName = kv.Key;
                var expectedHeaders = kv.Value;

                var ws = package.Workbook.Worksheets[sheetName];
                if (ws == null)
                {
                    errors.Add(new ImportErrorDetail(
                        sheetName,
                        $"Required sheet '{sheetName}' is missing.",
                        null,
                        null,
                        "MissingSheet"));
                    continue;
                }

                if (string.Equals(sheetName, "Service", StringComparison.OrdinalIgnoreCase))
                {
                    var branchCols = new Dictionary<int, int>(); 

                    for (int col = expectedHeaders.Length + 1; col <= ws.Dimension.Columns; col++)
                    {
                        var header = ws.Cells[1, col].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(header)) continue;

                       
                        var m = Regex.Match(header, @"^(Branch)_(?<idx>\d+)$", RegexOptions.IgnoreCase);
                        if (!m.Success)
                        {
                            errors.Add(new ImportErrorDetail(
                                sheetName,
                                $"Invalid header '{header}'. Branch columns must be 'Branch_1','Branch_2',...).",
                                1,
                                $"Column {col}",
                                "InvalidHeader"));
                            continue;
                        }

                        var idx = int.Parse(m.Groups["idx"].Value, CultureInfo.InvariantCulture);
                        if (idx <= 0)
                        {
                            errors.Add(new ImportErrorDetail(
                                sheetName,
                                $"Invalid header '{header}'. Index must be >= 1.",
                                1,
                                $"Column {col}",
                                "InvalidHeader"));
                            continue;
                        }

                        
                        if (branchCols.ContainsKey(idx))
                        {
                            errors.Add(new ImportErrorDetail(
                                sheetName,
                                $"Duplicate branch header for index {idx}. Use only one of 'Branch_{idx}'.",
                                1,
                                $"Column {col}",
                                "DuplicateHeader"));
                            continue;
                        }

                        branchCols[idx] = col;
                    }

                   
                    if (branchCols.Count > 0)
                    {
                        var ordered = branchCols.Keys.OrderBy(x => x).ToList();
                        for (int expected = 1; expected <= ordered.Count; expected++)
                        {
                            if (!branchCols.ContainsKey(expected))
                            {
                                errors.Add(new ImportErrorDetail(
                                    sheetName,
                                    $"Missing branch header for index {expected}. Expected 'Branch_{expected}' or 'BranchName_{expected}'.",
                                    1,
                                    null,
                                    "MissingHeader"));
                            }
                        }
                    }
                }


                if (string.Equals(sheetName, "Part", StringComparison.OrdinalIgnoreCase))
                {
                    
                    var branchCols = new Dictionary<int, int>(); // idx -> col
                    var stockCols = new Dictionary<int, int>(); // idx -> col

                    for (int col = expectedHeaders.Length + 1; col <= ws.Dimension.Columns; col++)
                    {
                        var header = ws.Cells[1, col].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(header)) continue;

                        var mBranch = Regex.Match(header, @"^Branch(Name)?_(?<idx>\d+)$", RegexOptions.IgnoreCase);
                        var mStock = Regex.Match(header, @"^Stock_(?<idx>\d+)$", RegexOptions.IgnoreCase);

                        if (mBranch.Success)
                        {
                            var idx = int.Parse(mBranch.Groups["idx"].Value, CultureInfo.InvariantCulture);
                            if (idx <= 0)
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Invalid header '{header}'. Index must be >= 1 (e.g., BranchName_1).",
                                    1, $"Column {col}", "InvalidHeader"));
                                continue;
                            }

                            if (branchCols.ContainsKey(idx))
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Duplicate branch header for index {idx}.",
                                    1, $"Column {col}", "DuplicateHeader"));
                                continue;
                            }

                            branchCols[idx] = col;
                            continue;
                        }

                        if (mStock.Success)
                        {
                            var idx = int.Parse(mStock.Groups["idx"].Value, CultureInfo.InvariantCulture);
                            if (idx <= 0)
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Invalid header '{header}'. Index must be >= 1 (e.g., Stock_1).",
                                    1, $"Column {col}", "InvalidHeader"));
                                continue;
                            }

                            if (stockCols.ContainsKey(idx))
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Duplicate stock header for index {idx}.",
                                    1, $"Column {col}", "DuplicateHeader"));
                                continue;
                            }

                            stockCols[idx] = col;
                            continue;
                        }

                        
                        errors.Add(new ImportErrorDetail(
                            sheetName,
                            $"Invalid header '{header}'. Columns must be 'BranchName_1'/'Branch_1' and 'Stock_1', 'BranchName_2'/'Branch_2' and 'Stock_2', ...",
                            1,
                            $"Column {col}",
                            "InvalidHeader"));
                    }

                    
                    if (branchCols.Count > 0 || stockCols.Count > 0)
                    {
                        var allIdx = branchCols.Keys.Union(stockCols.Keys).OrderBy(x => x).ToList();

                       
                        int expectedIdx = 1;
                        foreach (var idx in allIdx)
                        {
                            if (idx != expectedIdx)
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Missing index {expectedIdx}. Expected 'BranchName_{expectedIdx}'/'Branch_{expectedIdx}' and 'Stock_{expectedIdx}'.",
                                    1, null, "MissingHeader"));
                              
                                expectedIdx = idx; 
                            }
                            expectedIdx++;
                        }

                        
                        foreach (var idx in allIdx)
                        {
                            if (!branchCols.ContainsKey(idx))
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Missing header 'BranchName_{idx}' (or 'Branch_{idx}') for 'Stock_{idx}'.",
                                    1, null, "MissingHeader"));
                                continue;
                            }
                            if (!stockCols.ContainsKey(idx))
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Missing header 'Stock_{idx}' for 'BranchName_{idx}' (or 'Branch_{idx}').",
                                    1, null, "MissingHeader"));
                                continue;
                            }

                            
                            var bCol = branchCols[idx];
                            var sCol = stockCols[idx];

                            if (sCol != bCol + 1)
                            {
                                errors.Add(new ImportErrorDetail(sheetName,
                                    $"Invalid pair order. 'BranchName_{idx} must be immediately followed by 'Stock_{idx}'.",
                                    1, $"Columns {bCol}-{sCol}", "InvalidHeaderOrder"));
                            }
                        }
                    }
                }



                if (ws.Dimension == null || ws.Dimension.Columns < expectedHeaders.Length)
                {
                    errors.Add(new ImportErrorDetail(
                        sheetName,
                        $"Sheet '{sheetName}' does not contain the required number of columns. Expected at least {expectedHeaders.Length} columns.",
                        1,
                        null,
                        "InsufficientColumns"));
                    continue;
                }

                for (int col = 1; col <= expectedHeaders.Length; col++)
                {
                    var header = ws.Cells[1, col].Text?.Trim();
                    if (!string.Equals(header, expectedHeaders[col - 1], StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(new ImportErrorDetail(
                            sheetName,
                            $"Invalid header. Expected '{expectedHeaders[col - 1]}' but found '{header}'.",
                            1,
                            $"Column {col}",
                            "InvalidHeader"));
                    }
                }
            }

            return errors;
        }



        #endregion

        #region Helpers


        private (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (string.Empty, string.Empty);

            var parts = fullName
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return (parts[0], string.Empty);

            var firstName = parts[^1]; // từ cuối
            var lastName = string.Join(" ", parts[..^1]); // từ đầu đến trước từ cuối

            return (firstName, lastName);
        }

        private bool IsRowEmpty(ExcelWorksheet ws, int row, int colCount)
        {
            for (int col = 1; col <= colCount; col++)
            {
                if (!string.IsNullOrWhiteSpace(ws.Cells[row, col].Text?.Trim()))
                    return false;
            }
            return true; 
        }

        private static string Normalize(string? input)
            => input?.Trim().ToLowerInvariant() ?? string.Empty;

        private static string GetCategoryKey(string? name, Guid? parentId)
            => $"{Normalize(name)}|{(parentId.HasValue ? parentId.Value.ToString() : "root")}";

        private static string GetSpcKey(Guid serviceId, Guid partCategoryId)
            => $"{serviceId:N}|{partCategoryId:N}";

        private static string GetBranchServiceKey(Guid branchId, Guid serviceId)
        => $"{branchId:N}|{serviceId:N}";

        private static string GetOperatingHourKey(Guid branchId, DayOfWeekEnum day)
         => $"{branchId:N}|{(int)day}";

        private static string GetBranchKey(string branchName)
            => Normalize(branchName);
        private static string GetBrandKey(string brandName)
        => Normalize(brandName);

        private static string GetModelKey(Guid brandId, string modelName)
            => $"{brandId:N}|{Normalize(modelName)}";

        private static string GetPartCategoryKey(Guid modelId, string categoryName)
            => $"{modelId:N}|{Normalize(categoryName)}";

        // part key ưu tiên PartCode, fallback PartName
        private static string GetPartKey(Guid partCategoryId,  string partName)
            => $"{partCategoryId:N}|{Normalize(partName)}";

        private static string GetPartInventoryKey(Guid partId, Guid branchId)
            => $"{partId:N}|{branchId:N}";

        private async Task SendStaffWelcomeEmailsAsync(IEnumerable<StaffWelcomeEmailInfo> staffEmails)
        {
            foreach (var staff in staffEmails)
            {
                try
                {
                    var subject = "Your staff account has been created";
                    var body = $@"
                <p>Hi {staff.FullName},</p>
                <p>Your staff account has been created in the system.</p>
                <p>
                    <b>Username:</b> {staff.UserName}<br/>
                    <b>Email:</b> {staff.Email}
                    <b>Password:</b> Admin123!
                </p>
                <p>Please log in and change your password as soon as possible.</p>";

                    await _emailSender.SendEmailAsync(staff.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", staff.Email);
                    // Không throw, tránh làm fail import sau khi đã thành công
                }
            }
        }
        private void AddError(string sheetName, string message, int? row = null, string? column = null, string? errorCode = null)
        {
            var error = new ImportErrorDetail(sheetName, message, row, column, errorCode);
            _errors.Add(error);
            _logger.LogWarning(error.ToString());
        }

        private class StaffWelcomeEmailInfo
        {
            public string Email { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string UserName { get; set; } = null!;
        }

        #endregion
    }
}
