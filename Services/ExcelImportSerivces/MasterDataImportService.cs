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


namespace Services.ExcelImportSerivces
{
    public class MasterDataImportService : IMasterDataImportService
    {
        private readonly MyAppDbContext _context;
        private readonly ILogger<MasterDataImportService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly List<ImportErrorDetail> _errors = new();


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

        public MasterDataImportService(
            MyAppDbContext context,
            ILogger<MasterDataImportService> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
            )
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
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

                // Thứ tự xử lý
                await ImportBranchesAsync(package);
                await ImportOperatingHoursAsync(package);
                await ImportStaffAsync(package);
                await ImportParentCategoriesAsync(package);
                await ImportServiceCategoriesAsync(package);
                await ImportServicesAsync(package);
                await ImportPartCategoriesAsync(package);
                await ImportPartsAsync(package);

                await _context.SaveChangesAsync();

                if (_errors.Any())
                {
                    return ImportResult.Fail(
                        "Import failed because one or more errors were found. No data has been saved.",
                        _errors);
                }


                return ImportResult.Ok("Import master data thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi import Excel master data");
                return ImportResult.Fail($"Lỗi khi import Excel: {ex.Message}");
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
            var ws = package.Workbook.Worksheets["Branch"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var branchName = ws.Cells[row, 1].Text?.Trim();
                var phone = ws.Cells[row, 2].Text?.Trim();
                var email = ws.Cells[row, 3].Text?.Trim();
                var street = ws.Cells[row, 4].Text?.Trim();
                var commune = ws.Cells[row, 5].Text?.Trim();
                var province = ws.Cells[row, 6].Text?.Trim();
                var latText = ws.Cells[row, 7].Text?.Trim();
                var lonText = ws.Cells[row, 8].Text?.Trim();
                var description = ws.Cells[row, 9].Text?.Trim();
                var isActiveTxt = ws.Cells[row, 10].Text?.Trim();
                var arrivalTxt = ws.Cells[row, 11].Text?.Trim();
                var maxBookTxt = ws.Cells[row, 12].Text?.Trim();
                var maxWipTxt = ws.Cells[row, 13].Text?.Trim();

                if (string.IsNullOrWhiteSpace(branchName))
                    continue;

                double.TryParse(latText, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat);
                double.TryParse(lonText, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon);
                int.TryParse(arrivalTxt, out var arrival);
                int.TryParse(maxBookTxt, out var maxBooking);
                int.TryParse(maxWipTxt, out var maxWip);

                bool isActive = true;
                if (!string.IsNullOrEmpty(isActiveTxt))
                    bool.TryParse(isActiveTxt, out isActive);

                var key = Normalize(branchName);

                if (!_branchCache.TryGetValue(key, out var branch))
                {
                    branch = new Branch
                    {
                        BranchId = Guid.NewGuid(),
                        BranchName = branchName,
                        PhoneNumber = phone,
                        Email = email,
                        Street = street,
                        Commune = commune,
                        Province = province,
                        Latitude = lat,
                        Longitude = lon,
                        Description = description,
                        IsActive = isActive,
                        ArrivalWindowMinutes = arrival == 0 ? 30 : arrival,
                        MaxBookingsPerWindow = maxBooking == 0 ? 6 : maxBooking,
                        MaxConcurrentWip = maxWip == 0 ? 8 : maxWip,
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
                    branch.Latitude = lat;
                    branch.Longitude = lon;
                    branch.Description = description ?? branch.Description;
                    branch.IsActive = isActive;
                    branch.ArrivalWindowMinutes = arrival == 0 ? branch.ArrivalWindowMinutes : arrival;
                    branch.MaxBookingsPerWindow = maxBooking == 0 ? branch.MaxBookingsPerWindow : maxBooking;
                    branch.MaxConcurrentWip = maxWip == 0 ? branch.MaxConcurrentWip : maxWip;
                    branch.UpdatedAt = DateTime.UtcNow;

                    _context.Branches.Update(branch);
                }
            }
        }

        private async Task ImportStaffAsync(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets["Staff"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var userName = ws.Cells[row, 1].Text?.Trim();
                var email = ws.Cells[row, 2].Text?.Trim();
                var phone = ws.Cells[row, 3].Text?.Trim();
                var fullName = ws.Cells[row, 4].Text?.Trim();
                var roleName = ws.Cells[row, 5].Text?.Trim();
                var branchName = ws.Cells[row, 6].Text?.Trim();
                var isActiveTx = ws.Cells[row, 7].Text?.Trim();

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roleName))
                    continue;

                bool isActive = true;
                if (!string.IsNullOrEmpty(isActiveTx))
                    bool.TryParse(isActiveTx, out isActive);

                // Tìm Branch nếu có
                Guid? branchId = null;
                if (!string.IsNullOrWhiteSpace(branchName))
                {
                    var bKey = Normalize(branchName);
                    if (_branchCache.TryGetValue(bKey, out var branch))
                        branchId = branch.BranchId;
                }

                var uKey = Normalize(userName);
                ApplicationUser user;

                // 1️⃣ Nếu user chưa tồn tại → tạo mới
                if (!_staffCache.TryGetValue(uKey, out user))
                {
                    user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        PhoneNumber = phone,
                        FullName = fullName ?? string.Empty,
                        FirstName = fullName ?? string.Empty,
                        LastName = string.Empty,
                        BranchId = branchId,
                        IsActive = isActive,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Password mặc định (nhớ đổi sau)
                    var createResult = await _userManager.CreateAsync(user, "Password123!");

                    if (!createResult.Succeeded)
                    {
                        _logger.LogWarning("Không tạo được user {UserName}: {Errors}",
                            userName,
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        continue;
                    }

                    _staffCache[uKey] = user;
                }
                else
                {
                    // 2️⃣ Nếu user đã tồn tại → cập nhật
                    user.Email = email ?? user.Email;
                    user.PhoneNumber = phone ?? user.PhoneNumber;
                    user.FullName = fullName ?? user.FullName;
                    user.BranchId = branchId;
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.UtcNow;

                    await _userManager.UpdateAsync(user);
                }

                // 3️⃣ Gán Role (Technician / Manager)
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning("Role {RoleName} chưa tồn tại, bỏ qua gán role cho {UserName}", roleName, userName);
                }
                else
                {
                    if (!await _userManager.IsInRoleAsync(user, roleName))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                        if (!roleResult.Succeeded)
                        {
                            _logger.LogWarning("Không gán được role {RoleName} cho user {UserName}: {Errors}",
                                roleName, userName,
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // 4️⃣ Nếu role = Technician → tạo/ update Technician record
                if (string.Equals(roleName, "Technician", StringComparison.OrdinalIgnoreCase))
                {
                    // Kiểm tra đã có Technician chưa
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
                    else
                    {
                        // Nếu muốn update gì thêm (vd. reset điểm) thì xử lý tại đây
                    }
                }
            }
        }

        private async Task ImportOperatingHoursAsync(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets["BranchOperatingHour"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var branchName = ws.Cells[row, 1].Text?.Trim();
                var dayName = ws.Cells[row, 2].Text?.Trim();
                var isOpenText = ws.Cells[row, 3].Text?.Trim();
                var openTime = ws.Cells[row, 4].Text?.Trim();
                var closeTime = ws.Cells[row, 5].Text?.Trim();

                if (string.IsNullOrWhiteSpace(branchName) ||
                    string.IsNullOrWhiteSpace(dayName))
                    continue;

                var branchKey = Normalize(branchName);

                if (!_branchCache.TryGetValue(branchKey, out var branch))
                {
                    // Nếu branch chưa import được → bỏ qua hoặc log warning
                    _logger.LogWarning("Branch '{BranchName}' chưa tồn tại khi import OperatingHour", branchName);
                    continue;
                }

                if (!Enum.TryParse(dayName, true, out DayOfWeekEnum dayOfWeek))
                {
                    _logger.LogWarning("DayOfWeek không hợp lệ ở row {Row}: {Value}", row, dayName);
                    continue;
                }

                bool isOpen = false;
                bool.TryParse(isOpenText, out isOpen);

                TimeSpan? ot = null;
                TimeSpan? ct = null;

                if (isOpen)
                {
                    if (TimeSpan.TryParse(openTime, out var t1))
                        ot = t1;

                    if (TimeSpan.TryParse(closeTime, out var t2))
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
            }
        }



        private async Task ImportParentCategoriesAsync(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets["ParentCategory"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var parentName = ws.Cells[row, 1].Text?.Trim();
                var description = ws.Cells[row, 2].Text?.Trim();
                var isActiveTxt = ws.Cells[row, 3].Text?.Trim();

                if (string.IsNullOrWhiteSpace(parentName))
                    continue;

                bool isActive = true;
                if (!string.IsNullOrEmpty(isActiveTxt))
                    bool.TryParse(isActiveTxt, out isActive);

                var key = GetCategoryKey(parentName, null);

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
            var ws = package.Workbook.Worksheets["ServiceCategory"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var parentName = ws.Cells[row, 1].Text?.Trim();
                var categoryName = ws.Cells[row, 2].Text?.Trim();
                var description = ws.Cells[row, 3].Text?.Trim();
                var isActiveTxt = ws.Cells[row, 4].Text?.Trim();

                if (string.IsNullOrWhiteSpace(categoryName) &&
                    string.IsNullOrWhiteSpace(parentName))
                    continue;

                bool isActive = true;
                if (!string.IsNullOrEmpty(isActiveTxt))
                    bool.TryParse(isActiveTxt, out isActive);

                // Xử lý cha
                ServiceCategory? parentCat = null;
                Guid? parentId = null;

                if (!string.IsNullOrWhiteSpace(parentName))
                {
                    var parentKey = GetCategoryKey(parentName, null);
                    if (!_categoryCache.TryGetValue(parentKey, out parentCat))
                    {
                        parentCat = new ServiceCategory
                        {
                            ServiceCategoryId = Guid.NewGuid(),
                            CategoryName = parentName,
                            ParentServiceCategoryId = null,
                            Description = null,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.ServiceCategories.Add(parentCat);
                        _categoryCache[parentKey] = parentCat;
                    }

                    parentId = parentCat.ServiceCategoryId;
                }

                if (string.IsNullOrWhiteSpace(parentName))
                    parentId = null;

                var finalName = categoryName ?? parentName!;
                var key = GetCategoryKey(finalName, parentId);

                if (!_categoryCache.TryGetValue(key, out var cat))
                {
                    cat = new ServiceCategory
                    {
                        ServiceCategoryId = Guid.NewGuid(),
                        CategoryName = finalName,
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
            var ws = package.Workbook.Worksheets["Service"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var categoryName = ws.Cells[row, 1].Text?.Trim();
                var serviceName = ws.Cells[row, 2].Text?.Trim();
                var description = ws.Cells[row, 3].Text?.Trim();
                var priceText = ws.Cells[row, 4].Text?.Trim();
                var durationText = ws.Cells[row, 5].Text?.Trim();
                var isAdvText = ws.Cells[row, 6].Text?.Trim();
                var branchName = ws.Cells[row, 7].Text?.Trim();

                if (string.IsNullOrWhiteSpace(serviceName))
                    continue;

                decimal price = 0;
                decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out price);

                decimal duration = 0;
                decimal.TryParse(durationText, NumberStyles.Any, CultureInfo.InvariantCulture, out duration);

                bool isAdvanced = false;
                if (!string.IsNullOrEmpty(isAdvText))
                    bool.TryParse(isAdvText, out isAdvanced);

                // Tìm ServiceCategory
                Guid serviceCategoryId = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var cat = _categoryCache.Values
                        .FirstOrDefault(c => Normalize(c.CategoryName) == Normalize(categoryName));

                    if (cat == null)
                    {
                        cat = new ServiceCategory
                        {
                            ServiceCategoryId = Guid.NewGuid(),
                            CategoryName = categoryName,
                            ParentServiceCategoryId = null,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var key = GetCategoryKey(cat.CategoryName, cat.ParentServiceCategoryId);
                        _categoryCache[key] = cat;
                        _context.ServiceCategories.Add(cat);
                    }

                    serviceCategoryId = cat.ServiceCategoryId;
                }

               


                var sKey = Normalize(serviceName);
                if (!_serviceCache.TryGetValue(sKey, out var service))
                {
                    service = new Service
                    {
                        ServiceId = Guid.NewGuid(),
                        ServiceCategoryId = serviceCategoryId,
                        ServiceName = serviceName,
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

                // Branch (nếu có)

                if (!string.IsNullOrWhiteSpace(branchName))
                {
                    var bKey = Normalize(branchName);

                    if (_branchCache.TryGetValue(bKey, out var branch))
                    {
                        var bsKey = GetBranchServiceKey(branch.BranchId, service.ServiceId);

                        if (!_branchServiceCache.ContainsKey(bsKey))
                        {
                            var branchService = new BranchService
                            {
                                BranchId = branch.BranchId,
                                ServiceId = service.ServiceId
                            };

                            _context.BranchServices.Add(branchService);
                            _branchServiceCache[bsKey] = branchService;
                        }
                        // nếu đã có rồi thì bỏ qua, tránh duplicate
                    }
                    else
                    {
                        // Tuỳ bạn: log warning hoặc tạo branch mới
                        _logger.LogWarning(
                            "Không tìm thấy Branch '{BranchName}' khi gán cho Service '{ServiceName}'",
                            branchName, serviceName);
                    }
                }



            }
        }

        private async Task ImportPartCategoriesAsync(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets["PartCategory"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var partCategoryName = ws.Cells[row, 1].Text?.Trim();
                var description = ws.Cells[row, 2].Text?.Trim();
                var serviceName = ws.Cells[row, 3].Text?.Trim();

                if (string.IsNullOrWhiteSpace(partCategoryName))
                    continue;

                var pcKey = Normalize(partCategoryName);

                if (!_partCategoryCache.TryGetValue(pcKey, out var partCategory))
                {
                    partCategory = new PartCategory
                    {
                        LaborCategoryId = Guid.NewGuid(),
                        CategoryName = partCategoryName,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PartCategories.Add(partCategory);
                    _partCategoryCache[pcKey] = partCategory;
                }
                else
                {
                    partCategory.UpdatedAt = DateTime.UtcNow;
                    _context.PartCategories.Update(partCategory);
                }

                // Nối với Service (ServicePartCategory)
                if (!string.IsNullOrWhiteSpace(serviceName))
                {
                    var sKey = Normalize(serviceName);
                    if (_serviceCache.TryGetValue(sKey, out var svc))
                    {

                        // Lấy tất cả PartCategory hiện có của Service này
                        var existingPartCategoryIds = _spcCache.Values
                            .Where(x => x.ServiceId == svc.ServiceId)
                            .Select(x => x.PartCategoryId)
                            .Distinct()
                            .ToList();

                        // Count how many PartCategories are already linked to this Service
                        var currentCount = _spcCache.Values.Count(x => x.ServiceId == svc.ServiceId);

                        // Rule: if IsAdvanced = false => only one PartCategory is allowed
                        if (!svc.IsAdvanced && currentCount >= 1)
                        {
                            // Nếu PartCategory hiện tại TRÙNG với cái đã có -> cho qua, không tạo mới, không báo lỗi
                            if (existingPartCategoryIds.Contains(partCategory.LaborCategoryId))
                            {
                                // Đảm bảo record cụ thể trong cache tồn tại, nếu DB hơi lệch
                                var spcKeyExisting = GetSpcKey(svc.ServiceId, partCategory.LaborCategoryId);
                                if (!_spcCache.ContainsKey(spcKeyExisting))
                                {
                                    var spcExisting = new ServicePartCategory
                                    {
                                        ServicePartCategoryId = Guid.NewGuid(),
                                        ServiceId = svc.ServiceId,
                                        PartCategoryId = partCategory.LaborCategoryId,
                                        CreatedAt = DateTime.UtcNow
                                    };
                                    _context.ServicePartCategories.Add(spcExisting);
                                    _spcCache[spcKeyExisting] = spcExisting;
                                }

                                // Không báo lỗi, không tạo thêm record mới
                                continue;
                            }

                            // Nếu đến đây nghĩa là Service non-advanced đã có PartCategory khác rồi
                            AddError(
                                "PartCategory",
                                $"Service '{serviceName}' has IsAdvanced = false and can only be linked to one PartCategory. Existing PartCategory cannot be replaced by '{partCategoryName}'.",
                                row,
                                "ServiceName",
                                "TooManyPartCategoriesForNonAdvancedService");

                            // Không tạo thêm ServicePartCategory
                            continue;
                        }

                        var spcKey = GetSpcKey(svc.ServiceId, partCategory.LaborCategoryId);

                        if (!_spcCache.ContainsKey(spcKey))
                        {
                            var spc = new ServicePartCategory
                            {
                                ServicePartCategoryId = Guid.NewGuid(),
                                ServiceId = svc.ServiceId,
                                PartCategoryId = partCategory.LaborCategoryId,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.ServicePartCategories.Add(spc);
                            _spcCache[spcKey] = spc;
                        }
                    }
                    else
                    {
                        AddError(
                        "PartCategory",
                        $"Service '{serviceName}' does not exist but is referenced by PartCategory '{partCategoryName}'.",
                        row,
                        "ServiceName",
                        "ServiceNotFound");
                    }
                }
            }
        }

        private async Task ImportPartsAsync(ExcelPackage package)
        {
            var ws = package.Workbook.Worksheets["Part"];
            if (ws == null) return;

            int rowCount = ws.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var partCategoryName = ws.Cells[row, 1].Text?.Trim();
                var partName = ws.Cells[row, 2].Text?.Trim();
                var priceText = ws.Cells[row, 3].Text?.Trim();
                var stockText = ws.Cells[row, 4].Text?.Trim();
                //var branchName = ws.Cells[row, 5].Text?.Trim();

                if (string.IsNullOrWhiteSpace(partName))
                    continue;

                decimal price = 0;
                decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out price);

                int stock = 0;
                int.TryParse(stockText, out stock);

                PartCategory? partCategory = null;
                if (!string.IsNullOrWhiteSpace(partCategoryName))
                {
                    var pcKey = Normalize(partCategoryName);
                    if (!_partCategoryCache.TryGetValue(pcKey, out partCategory))
                    {
                        partCategory = new PartCategory
                        {
                            LaborCategoryId = Guid.NewGuid(),
                            CategoryName = partCategoryName,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PartCategories.Add(partCategory);
                        _partCategoryCache[pcKey] = partCategory;
                    }
                }

                

                var pKey = Normalize(partName);
                if (!_partCache.TryGetValue(pKey, out var part))
                {
                    part = new Part
                    {
                        PartId = Guid.NewGuid(),
                        PartCategoryId = partCategory?.LaborCategoryId ?? Guid.Empty,
                        
                        Name = partName,
                        Price = price,
                        Stock = stock,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Parts.Add(part);
                    _partCache[pKey] = part;
                }
                else
                {
                    part.PartCategoryId = partCategory?.LaborCategoryId ?? part.PartCategoryId;
                    
                    part.Price = price;
                    part.Stock = stock;
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
                        "Province","Latitude","Longitude","Description",
                        "IsActive","ArrivalWindowMinutes","MaxBookingsPerWindow","MaxConcurrentWip"
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
                        "PartCategoryName","PartName","Price","Stock"
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

        private void AddError(string sheetName, string message, int? row = null, string? column = null, string? errorCode = null)
        {
            var error = new ImportErrorDetail(sheetName, message, row, column, errorCode);
            _errors.Add(error);
            _logger.LogWarning(error.ToString());
        }

        #endregion
    }
}
