using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.ResultExcelReads;
using BusinessObject;
using Google;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DataAccessLayer;

using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using BusinessObject.Branches;
using BusinessObject.Enums;
using BusinessObject.Authentication;
using Microsoft.AspNetCore.Identity;
using BusinessObject.Roles;
using BusinessObject.InspectionAndRepair;
using Services.GeocodingServices;
using System.Text.RegularExpressions;
using Services.EmailSenders;


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
                return ImportResult.Fail("File không hợp lệ.");

            try
            {
                //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage.License.SetNonCommercialPersonal("Garage Pro");
                await InitCachesAsync();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);

                var templateErrors = ValidateTemplate(package);
                if (templateErrors.Any())
                {
                    return ImportResult.Fail(
                        "Excel template is invalid. Please fix the template and try again.",
                        templateErrors);
                }
                var staffWelcomeEmails = new List<StaffWelcomeEmailInfo>();

                await ImportBranchesAsync(package);
                await ImportOperatingHoursAsync(package);
                await ImportStaffAsync(package, staffWelcomeEmails);
                await ImportParentCategoriesAsync(package);
                await ImportServiceCategoriesAsync(package);
                await ImportServicesAsync(package);
                await ImportPartCategoriesAsync(package);
                await ImportPartsAsync(package);


                if (_errors.Any())
                {
                    return ImportResult.Fail(
                        "Import failed because one or more errors were found. No data has been saved.",
                        _errors);
                }
                await _context.SaveChangesAsync();

                await SendStaffWelcomeEmailsAsync(staffWelcomeEmails);

                return ImportResult.Ok("Import master data Success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi import Excel master data");
                return ImportResult.Fail($"Import Excel Fail: {ex.Message}");
            }
        }

        #region Init Caches

        private async Task InitCachesAsync()
        {
            var serviceCategories = await _context.ServiceCategories.ToListAsync();
            var services = await _context.Services.ToListAsync();
            var branchServices = await _context.BranchServices.ToListAsync();

            var partCategories = await _context.PartCategories.ToListAsync();
            var parts = await _context.Parts.ToListAsync();
            var servicePartCats = await _context.ServicePartCategories.ToListAsync();
            var branches = await _context.Branches.ToListAsync();
            var operatingHours = await _context.OperatingHours.ToListAsync();
            var users = await _userManager.Users.ToListAsync();
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
            _partCategoryCache = partCategories
                .GroupBy(pc => Normalize(pc.CategoryName))
                .ToDictionary(g => g.Key, g => g.First());

            _partCache = parts
                .GroupBy(p => Normalize(p.Name))
                .ToDictionary(g => g.Key, g => g.First());

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
                        MaxConcurrentWip = 2,
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
                    branch.MaxConcurrentWip = 2;
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

            // Pre-scan duplicate usernames (normalized)
            // Count username occurrences
            var userNameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Count email occurrences
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

                // ===== Required fields (no field can be empty) =====

                
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

                // Branch required
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

                // IsActive required
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

                // Role must be Manager or Technician
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

                    // Duplicate username in sheet (pre-calculated)
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

                // Parse IsActive
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

                // Resolve Branch (branchName is required, already checked above)
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
                        FullName = fullName!,
                        FirstName = firstName!,
                        LastName = lastName,
                        BranchId = branchId,
                        IsActive = isActive,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user, "Password123!");

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
                            FullName = user.FullName!,
                            UserName = user.UserName!
                        });
                    }

                    _staffCache[uKey] = user;
                }
                else
                {
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.Email = email ?? user.Email;
                    user.PhoneNumber = phone ?? user.PhoneNumber;
                    user.FullName = fullName ?? user.FullName;
                    user.FirstName = fullName ?? user.FirstName;
                    user.BranchId = branchId;
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.UtcNow;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        AddError(
                            sheetName,
                            $"Failed to update user '{userName}': {string.Join("; ", updateResult.Errors.Select(e => e.Description))}.",
                            row,
                            null,
                            "STAFF_UPDATE_USER_FAILED"
                        );
                        continue;
                    }
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

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 7))
                    continue;

                bool hasError = false;

                var categoryName = ws.Cells[row, 1].Text?.Trim(); // A
                var serviceName = ws.Cells[row, 2].Text?.Trim(); // B
                var description = ws.Cells[row, 3].Text?.Trim(); // C
                var priceText = ws.Cells[row, 4].Text?.Trim(); // D
                var durationText = ws.Cells[row, 5].Text?.Trim(); // E
                var isAdvText = ws.Cells[row, 6].Text?.Trim(); // F
                var branchName = ws.Cells[row, 7].Text?.Trim(); // G

                // ===== Required fields =====
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    AddError(
                        sheetName,
                        "Service Category Name is required.",
                        row,
                        "A",
                        "SERVICE_CATEGORY_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    AddError(
                        sheetName,
                        "Service Name is required.",
                        row,
                        "B",
                        "SERVICE_NAME_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    AddError(
                        sheetName,
                        "Description is required.",
                        row,
                        "C",
                        "SERVICE_DESCRIPTION_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(priceText))
                {
                    AddError(
                        sheetName,
                        "Price is required.",
                        row,
                        "D",
                        "SERVICE_PRICE_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(durationText))
                {
                    AddError(
                        sheetName,
                        "Estimated Duration is required.",
                        row,
                        "E",
                        "SERVICE_DURATION_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(isAdvText))
                {
                    AddError(
                        sheetName,
                        "'Is Advanced' is required.",
                        row,
                        "F",
                        "SERVICE_IS_ADVANCED_REQUIRED"
                    );
                    hasError = true;
                }

                if (hasError)
                    continue;

                // ===== Parse numeric & boolean =====
                if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    AddError(
                        sheetName,
                        $"Invalid number format for 'Price': '{priceText}'.",
                        row,
                        "D",
                        "SERVICE_INVALID_PRICE"
                    );
                    hasError = true;
                }

                if (!decimal.TryParse(durationText, NumberStyles.Any, CultureInfo.InvariantCulture, out var duration))
                {
                    AddError(
                        sheetName,
                        $"Invalid number format for 'Estimated Duration': '{durationText}'.",
                        row,
                        "E",
                        "SERVICE_INVALID_DURATION"
                    );
                    hasError = true;
                }

                bool isAdvanced = false;
                if (!bool.TryParse(isAdvText, out isAdvanced))
                {
                    AddError(
                        sheetName,
                        $"Invalid boolean value for 'Is Advanced': '{isAdvText}'. Expected 'true' or 'false'.",
                        row,
                        "F",
                        "SERVICE_INVALID_IS_ADVANCED"
                    );
                    hasError = true;
                }

                // ===== Check ServiceCategory exists in system =====
                Guid serviceCategoryId = Guid.Empty;

                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var normCatName = Normalize(categoryName);
                    var cat = _categoryCache.Values
                        .FirstOrDefault(c => Normalize(c.CategoryName) == normCatName);

                    if (cat == null)
                    {
                        AddError(
                            sheetName,
                            $"Service Category '{categoryName}' does not exist in the system.",
                            row,
                            "A",
                            "SERVICE_CATEGORY_NOT_FOUND"
                        );
                        hasError = true;
                    }
                    else
                    {
                        serviceCategoryId = cat.ServiceCategoryId;
                    }
                }

                // ===== Check Branch exists if provided =====
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
                            $"Branch '{branchName}' does not exist in the system.",
                            row,
                            "G",
                            "SERVICE_BRANCH_NOT_FOUND"
                        );
                        hasError = true;
                    }
                }

                if (hasError)
                    continue;

                // ===== Create / Update Service =====
                var sKey = Normalize(serviceName!);
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

                // ===== Branch mapping (if branchId != null) =====
                if (branchId.HasValue)
                {
                    var bsKey = GetBranchServiceKey(branchId.Value, service.ServiceId);

                    if (!_branchServiceCache.ContainsKey(bsKey))
                    {
                        var branchService = new BranchService
                        {
                            BranchId = branchId.Value,
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

            // === Pre-scan duplicate PartCategoryName ===
            var partCategoryNameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 3))
                    continue;

                var nameScan = ws.Cells[row, 1].Text?.Trim();
                if (string.IsNullOrWhiteSpace(nameScan))
                    continue;

                var keyScan = Normalize(nameScan);
                if (partCategoryNameCounts.ContainsKey(keyScan))
                    partCategoryNameCounts[keyScan]++;
                else
                    partCategoryNameCounts[keyScan] = 1;
            }

            var duplicatePartCategoryNames = new HashSet<string>(
                partCategoryNameCounts.Where(x => x.Value > 1).Select(x => x.Key),
                StringComparer.OrdinalIgnoreCase
            );

            // === Main import ===
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(ws, row, 3))
                    continue;

                bool hasError = false;

                var partCategoryName = ws.Cells[row, 1].Text?.Trim(); // A
                var description = ws.Cells[row, 2].Text?.Trim(); // B
                var serviceName = ws.Cells[row, 3].Text?.Trim(); // C

                // ===== Required fields =====
                if (string.IsNullOrWhiteSpace(partCategoryName))
                {
                    AddError(
                        sheetName,
                        "Part Category Name is required.",
                        row,
                        "A",
                        "PART_CATEGORY_NAME_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    AddError(
                        sheetName,
                        "Description is required.",
                        row,
                        "B",
                        "PART_CATEGORY_DESCRIPTION_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    AddError(
                        sheetName,
                        "Service Name is required.",
                        row,
                        "C",
                        "PART_CATEGORY_SERVICE_NAME_REQUIRED"
                    );
                    hasError = true;
                }

                if (hasError)
                    continue;

                var pcKey = Normalize(partCategoryName!);

                // Duplicate PartCategoryName in sheet
                if (duplicatePartCategoryNames.Contains(pcKey))
                {
                    AddError(
                        sheetName,
                        $"Duplicate part category name '{partCategoryName}' found in the sheet.",
                        row,
                        "A",
                        "PART_CATEGORY_DUPLICATE_NAME"
                    );
                    hasError = true;
                }

                // Check ServiceName exists in system
                Service? svc = null;
                Guid? serviceId = null;

                if (!string.IsNullOrWhiteSpace(serviceName))
                {
                    var sKey = Normalize(serviceName);
                    if (_serviceCache.TryGetValue(sKey, out svc))
                    {
                        serviceId = svc.ServiceId;
                    }
                    else
                    {
                        AddError(
                            sheetName,
                            $"Service '{serviceName}' does not exist in the system but is referenced by Part Category '{partCategoryName}'.",
                            row,
                            "C",
                            "PART_CATEGORY_SERVICE_NOT_FOUND"
                        );
                        hasError = true;
                    }
                }

                if (hasError)
                    continue;

                // Create / update PartCategory
                if (!_partCategoryCache.TryGetValue(pcKey, out var partCategory))
                {
                    partCategory = new PartCategory
                    {
                        LaborCategoryId = Guid.NewGuid(),
                        CategoryName = partCategoryName!,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PartCategories.Add(partCategory);
                    _partCategoryCache[pcKey] = partCategory;
                }
                else
                {
                    partCategory.Description = description ?? partCategory.Description;
                    partCategory.UpdatedAt = DateTime.UtcNow;
                    _context.PartCategories.Update(partCategory);
                }

                // Link with Service (ServicePartCategory) – only if service exists
                if (svc != null && serviceId.HasValue)
                {
                    // Current PartCategories of this Service
                    var existingPartCategoryIds = _spcCache.Values
                        .Where(x => x.ServiceId == serviceId.Value)
                        .Select(x => x.PartCategoryId)
                        .Distinct()
                        .ToList();

                    var currentCount = existingPartCategoryIds.Count;

                    // Rule: if IsAdvanced = false => only one PartCategory is allowed
                    if (!svc.IsAdvanced && currentCount >= 1)
                    {
                        // If current PartCategory already linked -> ensure link exists in cache/db, but do not create duplicate
                        if (existingPartCategoryIds.Contains(partCategory.LaborCategoryId))
                        {
                            var spcKeyExisting = GetSpcKey(serviceId.Value, partCategory.LaborCategoryId);
                            if (!_spcCache.ContainsKey(spcKeyExisting))
                            {
                                var spcExisting = new ServicePartCategory
                                {
                                    ServicePartCategoryId = Guid.NewGuid(),
                                    ServiceId = serviceId.Value,
                                    PartCategoryId = partCategory.LaborCategoryId,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.ServicePartCategories.Add(spcExisting);
                                _spcCache[spcKeyExisting] = spcExisting;
                            }

                            // Do not report error, do not create new record
                            continue;
                        }

                        // Non-advanced service already has another PartCategory linked
                        AddError(
                            sheetName,
                            $"Service '{serviceName}' has IsAdvanced = false and can only be linked to one Part Category. Existing Part Category cannot be replaced by '{partCategoryName}'.",
                            row,
                            "C",
                            "PART_CATEGORY_TOO_MANY_FOR_NON_ADVANCED_SERVICE"
                        );

                        continue;
                    }

                    var spcKey = GetSpcKey(serviceId.Value, partCategory.LaborCategoryId);

                    if (!_spcCache.ContainsKey(spcKey))
                    {
                        var spc = new ServicePartCategory
                        {
                            ServicePartCategoryId = Guid.NewGuid(),
                            ServiceId = serviceId.Value,
                            PartCategoryId = partCategory.LaborCategoryId,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.ServicePartCategories.Add(spc);
                        _spcCache[spcKey] = spc;
                    }
                }
            }
        }


        private async Task ImportPartsAsync(ExcelPackage package)
        {
            const string sheetName = "Part";
            var ws = package.Workbook.Worksheets[sheetName];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                // Bỏ qua dòng trống hoàn toàn
                if (IsRowEmpty(ws, row, 3))
                    continue;

                bool hasError = false;

                var partCategoryName = ws.Cells[row, 1].Text?.Trim(); // A
                var partName = ws.Cells[row, 2].Text?.Trim(); // B
                var priceText = ws.Cells[row, 3].Text?.Trim(); // C

                // ===== Required fields =====
                if (string.IsNullOrWhiteSpace(partCategoryName))
                {
                    AddError(
                        sheetName,
                        "Part Category Name is required.",
                        row,
                        "A",
                        "PART_CATEGORY_NAME_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(partName))
                {
                    AddError(
                        sheetName,
                        "Part Name is required.",
                        row,
                        "B",
                        "PART_NAME_REQUIRED"
                    );
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(priceText))
                {
                    AddError(
                        sheetName,
                        "Price is required.",
                        row,
                        "C",
                        "PART_PRICE_REQUIRED"
                    );
                    hasError = true;
                }

                if (hasError)
                    continue;

                // ===== Parse price =====
                if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    AddError(
                        sheetName,
                        $"Invalid number format for 'Price': '{priceText}'.",
                        row,
                        "C",
                        "PART_INVALID_PRICE"
                    );
                    continue;
                }

                // ===== Check PartCategory exists in system =====
                PartCategory? partCategory = null;
                var pcKey = Normalize(partCategoryName!);

                if (!_partCategoryCache.TryGetValue(pcKey, out partCategory))
                {
                    AddError(
                        sheetName,
                        $"Part Category '{partCategoryName}' does not exist in the system.",
                        row,
                        "A",
                        "PART_CATEGORY_NOT_FOUND"
                    );
                    continue;
                }

                // ===== Create / Update Part =====
                var pKey = Normalize(partName!);

                if (!_partCache.TryGetValue(pKey, out var part))
                {
                    part = new Part
                    {
                        PartId = Guid.NewGuid(),
                        PartCategoryId = partCategory.LaborCategoryId,
                        Name = partName!,
                        Price = price,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Parts.Add(part);
                    _partCache[pKey] = part;
                }
                else
                {
                    part.PartCategoryId = partCategory.LaborCategoryId;
                    part.Price = price;
                    part.UpdatedAt = DateTime.UtcNow;

                    _context.Parts.Update(part);
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
                        "EstimatedDuration","IsAdvanced","BranchName"
                    },
                ["PartCategory"] = new[]
                            {
                        "PartCategoryName","Description","ServiceName"
                    },
                ["Part"] = new[]
                            {
                        "PartCategoryName","PartName","Price"
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
