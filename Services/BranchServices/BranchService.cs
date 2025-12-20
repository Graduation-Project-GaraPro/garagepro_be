using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Branches;
using BusinessObject.Customers;
using BusinessObject.Enums;
using DataAccessLayer;
using Dtos.Branches;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.BranchRepositories;
using Repositories.ServiceRepositories;
using Services.GeocodingServices;

namespace Services.BranchServices
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IOperatingHourRepository _operatingHourRepo;
        private readonly IUserRepository _userRepository;
        private readonly IGeocodingService _geocodingService;
        private readonly MyAppDbContext _context;
        private readonly IMapper _mapper;

        public BranchService(
            IBranchRepository branchRepo,
            IMapper mapper,
            IServiceRepository serviceRepo,
            IUserRepository userRepository,
            IGeocodingService geocodingService,
            IOperatingHourRepository operatingHourRepo,
            MyAppDbContext context)
        {
            _branchRepo = branchRepo;
            _serviceRepo = serviceRepo;
            _userRepository = userRepository;
            _operatingHourRepo = operatingHourRepo;
            _geocodingService = geocodingService;
            _context = context;
            _mapper = mapper;
        }

        
        public async Task<BranchReadDto?> CreateBranchAsync(BranchCreateDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {

                if (dto.ArrivalWindowMinutes % 30 != 0)
                {
                    throw new ApplicationException("Arrival window minutes must be in increments of 30 minutes (30, 60, 90, etc.).");
                }

                
                bool isNameDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchName.ToLower().Trim() == dto.BranchName.ToLower().Trim()
                );

                bool isAddressDuplicate = await _branchRepo.ExistsAsync(
                    b => b.Street.ToLower().Trim() == dto.Street.ToLower().Trim()
                      && b.Commune.ToLower().Trim() == dto.Commune.ToLower().Trim()
                      && b.Province.ToLower().Trim() == dto.Province.ToLower().Trim()
                     
                );



                if (isNameDuplicate)
                    throw new ApplicationException("A branch with the same name already exists.");

                if (isAddressDuplicate)
                    throw new ApplicationException("A branch with the same address already exists.");

                var fullAddress = $"{dto.Street}, {dto.Commune}, {dto.Province}";
                var (lat, lng, formattedAddress) = await _geocodingService.GetCoordinatesAsync(fullAddress);

                
                var days = dto.OperatingHours
                 .Select(o => o.DayOfWeek)
                 .ToList();

                    
                    if (days.Count > 7)
                    {
                        throw new ApplicationException("Cannot provide more than 7 operating hours (Monday to Sunday).");
                    }

                    
                    if (days.Distinct().Count() != 7 || !Enumerable.Range(1, 7).All(d => days.Contains((BusinessObject.Enums.DayOfWeekEnum)d)))
                    {
                        throw new ApplicationException("Operating hours must cover all 7 days (Monday to Sunday) with no duplicates.");
                    }

                    
                    if (!dto.OperatingHours.Any(o => o.IsOpen))
                    {
                        throw new ApplicationException("At least one day must be open (IsOpen = true).");
                    }

                    
                    foreach (var op in dto.OperatingHours)
                    {
                        if (op.IsOpen && (!op.OpenTime.HasValue || !op.CloseTime.HasValue))
                        {
                            throw new ApplicationException($"Operating hours for {op.DayOfWeek} must have both OpenTime and CloseTime when IsOpen is true.");
                        }
                    }

                    
                    if (dto.OperatingHours.Any(o => (int)o.DayOfWeek < 1 || (int)o.DayOfWeek > 7))
                    {
                        throw new ApplicationException("Operating hours can only be set from Monday to Sunday.");
                    }

                
                var existingServiceIds = await _serviceRepo.Query()
                    .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                    .Select(s => s.ServiceId)
                    .ToListAsync();

                var missingServices = dto.ServiceIds.Except(existingServiceIds).ToList();
                if (missingServices.Any())
                {
                    throw new ApplicationException($"Some service IDs do not exist: {string.Join(", ", missingServices)}");
                }

               

                
                var allowedUsers = await _userRepository.GetManagersAndTechniciansAsync();

                var validStaffs = allowedUsers
                    .Where(u => dto.StaffIds != null && dto.StaffIds.Contains(u.Id))
                    .ToList();

                if (dto.StaffIds != null && dto.StaffIds.Any() && validStaffs.Count != dto.StaffIds.Count)
                {
                    var invalidStaffIds = dto.StaffIds.Except(validStaffs.Select(u => u.Id));
                    if (invalidStaffIds.Any())
                    {

                        var invalidStaffNames = await _context.Users
                            .Where(u => invalidStaffIds.Contains(u.Id))
                            .Select(u => u.FirstName + " " + u.LastName)
                            .ToListAsync();

                        throw new ApplicationException(
                            $"Some staff are currently working on a job: {string.Join(", ", invalidStaffNames)}"
                        );
                    }
                }


                
                var branchId = Guid.NewGuid();

                var branch = new Branch
                {
                    BranchId = branchId,
                    BranchName = dto.BranchName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    Street = dto.Street,
                    Commune = dto.Commune,
                    Province = dto.Province,
                    Description = dto.Description,
                    ArrivalWindowMinutes = dto.ArrivalWindowMinutes,
                    MaxBookingsPerWindow = dto.MaxBookingsPerWindow,
                    
                    IsActive = true,
                    Latitude = lat,
                    Longitude = lng,
                    CreatedAt = DateTime.UtcNow,
                    BranchServices = existingServiceIds
                        .Select(sid => new BusinessObject.Branches.BranchService
                        {
                            BranchId = branchId,
                            ServiceId = sid
                        }).ToList(),
                    OperatingHours = dto.OperatingHours
                        .Select(oh => new OperatingHour
                        {
                            BranchId = branchId,
                            DayOfWeek = oh.DayOfWeek,
                            OpenTime = oh.OpenTime,
                            CloseTime = oh.CloseTime,
                            IsOpen = oh.IsOpen
                        }).ToList()
                };

                // Assign valid staffs to branch
                foreach (var user in validStaffs)
                {
                    user.BranchId = branchId;
                    branch.Staffs.Add(user);
                }

                await _branchRepo.AddAsync(branch);
                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();

                return _mapper.Map<BranchReadDto>(branch);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new ApplicationException("Error creating branch: " + ex.Message, ex);
            }
        }



        // UPDATE
        public async Task<BranchReadDto?> UpdateBranchAsync(BranchUpdateDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = await _branchRepo.GetBranchWithRelationsAsync(dto.BranchId);
                if (branch == null) return null;

                if (dto.ArrivalWindowMinutes % 30 != 0)
                {
                    throw new ApplicationException("Arrival window minutes must be in increments of 30 minutes (30, 60, 90, etc.).");
                }

                // === Check duplicate name/address ===
                bool isNameDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchId != dto.BranchId &&
                         b.BranchName.ToLower().Trim() == dto.BranchName.ToLower().Trim()
                );

                bool isAddressDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchId != dto.BranchId &&
                         b.Street.ToLower().Trim() == dto.Street.ToLower().Trim() &&
                         b.Commune.ToLower().Trim() == dto.Commune.ToLower().Trim() &&
                         b.Province.ToLower().Trim() == dto.Province.ToLower().Trim()
                );

                if (isNameDuplicate) throw new ApplicationException("Branch name already exists.");
                if (isAddressDuplicate) throw new ApplicationException("Branch address already exists.");

                var fullAddress = $"{dto.Street}, {dto.Commune}, {dto.Province}";
                var (lat, lng, formattedAddress) = await _geocodingService.GetCoordinatesAsync(fullAddress);

                // === Validate operating hours ===
                var days = dto.OperatingHours
                    .Select(o => o.DayOfWeek)
                    .ToList();

                // Check không gửi quá 7 ngày
                if (days.Count > 7)
                {
                    throw new ApplicationException("Cannot provide more than 7 operating hours (Monday to Sunday).");
                }

                // Check đủ 7 ngày không trùng
                if (days.Distinct().Count() != 7 ||
                    !Enumerable.Range(1, 7).All(d => days.Contains((BusinessObject.Enums.DayOfWeekEnum)d)))
                {
                    throw new ApplicationException("Operating hours must cover all 7 days (Monday to Sunday) with no duplicates.");
                }

                if (!dto.OperatingHours.Any(o => o.IsOpen))
                {
                    throw new ApplicationException("At least one day must be open (IsOpen = true).");
                }

                foreach (var op in dto.OperatingHours)
                {
                    if (op.IsOpen && (!op.OpenTime.HasValue || !op.CloseTime.HasValue))
                    {
                        throw new ApplicationException($"Operating hours for {op.DayOfWeek} must have both OpenTime and CloseTime when IsOpen is true.");
                    }
                }

                // ensure only Monday -> Sunday
                if (dto.OperatingHours.Any(o => (int)o.DayOfWeek < 1 || (int)o.DayOfWeek > 7))
                {
                    throw new ApplicationException("Operating hours can only be set from Monday to Sunday.");
                }

                // === Validate services ===
                var existingServiceIds = await _serviceRepo.Query()
                    .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                    .Select(s => s.ServiceId)
                    .ToListAsync();

                var missingServices = dto.ServiceIds.Except(existingServiceIds).ToList();
                if (missingServices.Any())
                    throw new ApplicationException($"Some service IDs do not exist: {string.Join(", ", missingServices)}");

                
                if (dto.StaffIds != null)
                {
                    // Loại trùng lặp
                    var requestedStaffIds = dto.StaffIds.Distinct().ToList();

                    // Lấy danh sách user rảnh (Manager + Technician idle)
                    var allowedUsers = await _userRepository.GetManagersAndTechniciansAsync();

                    // Staff hiện tại của branch (có thể đang bận, nhưng vẫn hợp lệ nếu giữ nguyên)
                    var currentStaffUsers = branch.Staffs.ToList();

                    // allowedIds = user rảnh + staff hiện tại của branch
                    var allowedIds = allowedUsers
                        .Select(u => u.Id)
                        .Concat(currentStaffUsers.Select(u => u.Id))
                        .ToHashSet();

                    // 1) Validate ID có thuộc allowedIds không
                    var invalidStaffIds = requestedStaffIds
                        .Where(id => !allowedIds.Contains(id))
                        .ToList();

                    if (invalidStaffIds.Any())
                    {
                        
                        var invalidStaffNames = await _context.Users
                            .Where(u => invalidStaffIds.Contains(u.Id))
                            .Select(u => u.FirstName +" "+ u.LastName)  
                            .ToListAsync();

                        throw new ApplicationException(
                            $"Some staff are currently working on a job: {string.Join(", ", invalidStaffNames)}"
                        );
                    }

                    // 2) Chuẩn bị diff add/remove
                    var currentStaffIds = currentStaffUsers.Select(s => s.Id).ToHashSet();

                    // Staff cần remove: đang có trong branch nhưng không nằm trong requestedStaffIds
                    var staffsToRemove = currentStaffUsers
                        .Where(s => !requestedStaffIds.Contains(s.Id))
                        .ToList();

                    foreach (var staff in staffsToRemove)
                    {
                        staff.BranchId = null;
                        branch.Staffs.Remove(staff);
                    }

                    // Staff cần thêm mới: có trong requestedStaffIds nhưng chưa có trong branch
                    var idsToAdd = requestedStaffIds
                        .Where(id => !currentStaffIds.Contains(id))
                        .ToList();

                    // Những user được add mới PHẢI đến từ allowedUsers (user rảnh)
                    var usersToAdd = allowedUsers
                        .Where(u => idsToAdd.Contains(u.Id))
                        .ToList();

                    foreach (var user in usersToAdd)
                    {
                        user.BranchId = branch.BranchId;
                        branch.Staffs.Add(user);
                    }
                }

                // === Update basic fields ===
                branch.BranchName = dto.BranchName;
                branch.PhoneNumber = dto.PhoneNumber;
                branch.Email = dto.Email;
                branch.Street = dto.Street;
                branch.Commune = dto.Commune;
                branch.Province = dto.Province;
                branch.ArrivalWindowMinutes = dto.ArrivalWindowMinutes;
                branch.MaxBookingsPerWindow = dto.MaxBookingsPerWindow;
                branch.Description = dto.Description;
                branch.IsActive = dto.IsActive;
                branch.UpdatedAt = DateTime.UtcNow;
                branch.Latitude = lat;
                branch.Longitude = lng;

                
                var currentServiceIds = branch.BranchServices.Select(bs => bs.ServiceId).ToList();

                foreach (var bs in branch.BranchServices
                             .Where(bs => !existingServiceIds.Contains(bs.ServiceId))
                             .ToList())
                {
                    branch.BranchServices.Remove(bs);
                }

                // Add services mới
                foreach (var sid in existingServiceIds.Except(currentServiceIds))
                {
                    branch.BranchServices.Add(new BusinessObject.Branches.BranchService
                    {
                        BranchId = branch.BranchId,
                        ServiceId = sid
                    });
                }

                await EnsureNoConflictingRepairRequestsAsync(branch, dto.OperatingHours.ToList());

                // === Update Operating Hours (diff) ===
                foreach (var ohDto in dto.OperatingHours)
                {
                    var existing = branch.OperatingHours.FirstOrDefault(o => o.DayOfWeek == ohDto.DayOfWeek);
                    if (existing != null)
                    {
                        existing.OpenTime = ohDto.OpenTime;
                        existing.CloseTime = ohDto.CloseTime;
                        existing.IsOpen = ohDto.IsOpen;
                    }
                    else
                    {
                        branch.OperatingHours.Add(new OperatingHour
                        {
                            BranchId = branch.BranchId,
                            DayOfWeek = ohDto.DayOfWeek,
                            OpenTime = ohDto.OpenTime,
                            CloseTime = ohDto.CloseTime,
                            IsOpen = ohDto.IsOpen
                        });
                    }
                }

                // Remove operating hours không có trong DTO
                foreach (var oh in branch.OperatingHours
                             .Where(o => !dto.OperatingHours.Any(d => d.DayOfWeek == o.DayOfWeek))
                             .ToList())
                {
                    branch.OperatingHours.Remove(oh);
                }

                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();

                return _mapper.Map<BranchReadDto>(branch);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new ApplicationException("Error updating branch: " + ex.Message, ex);
            }
        }





        public async Task UpdateIsActiveForManyAsync(IEnumerable<Guid> branchIds, bool isActive)
        {
            try
            {
                await _branchRepo.UpdateIsActiveForManyAsync(branchIds, isActive);
            }
            catch (Exception ex)
            {

                throw new ApplicationException("An error occurred while updating branch statuses.", ex);
            }
        }



        // DELETE
        public async Task DeleteManyAsync(IEnumerable<Guid> branchIds)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
               
                var branches = await _branchRepo.Query()
                    .Include(b => b.RepairOrders)
                    .Include(b => b.RepairRequests)
                    .Where(b => branchIds.Contains(b.BranchId))
                    .ToListAsync();

                
                var notFoundIds = branchIds.Except(branches.Select(b => b.BranchId)).ToList();
                if (notFoundIds.Any())
                {
                    throw new ApplicationException($"The following branch IDs do not exist: {string.Join(", ", notFoundIds)}");
                }

                // Check branch có RepairOrders hoặc RepairRequests
                var blockedBranches = branches
                    .Where(b => (b.RepairOrders != null && b.RepairOrders.Any()) ||
                                (b.RepairRequests != null && b.RepairRequests.Any()))
                    .ToList();

                if (blockedBranches.Any())
                {
                    var blockedIds = blockedBranches.Select(b => b.BranchId);
                    throw new ApplicationException($"Cannot delete branches because they have associated Repair Orders or Repair Requests: {string.Join(", ", blockedIds)}");
                }

                
                await _branchRepo.DeleteManyAsync(branchIds);
                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new ApplicationException("Unable to delete some branches because they are being referenced by other data.", ex);
            }
            catch (Exception ex)
            {
                //throw new ApplicationException("An error occurred while deleting branches.", ex);
                throw new ApplicationException(ex.Message);
            }
        }


        public async Task<bool> DeleteBranchAsync(Guid branchId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = await _branchRepo.Query()
                    .Include(b => b.RepairOrders)
                    .Include(b => b.RepairRequests)
                    .FirstOrDefaultAsync(b => b.BranchId == branchId);

                if (branch == null) return false;

                // Check nếu branch đang có RepairOrders hoặc RepairRequests
                if ((branch.RepairOrders != null && branch.RepairOrders.Any()) ||
                    (branch.RepairRequests != null && branch.RepairRequests.Any()))
                {
                    throw new ApplicationException("Cannot delete branch because it has associated Repair Orders or Repair Requests.");
                }

                await _branchRepo.DeleteAsync(branch);
                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new ApplicationException(ex.Message);
            }
        }

        // GET BY ID
        public async Task<BranchReadDto?> GetBranchByIdAsync(Guid id)
        {
            var branch = await _branchRepo.GetBranchWithRelationsAsync(id);
            return branch == null ? null : _mapper.Map<BranchReadDto>(branch);
        }

        
        public async Task<IEnumerable<BranchReadDto>> GetAllBranchesAsync()
        {
            var branches = await _branchRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<BranchReadDto>>(branches);
        }
        public async Task<IEnumerable<Branch>> GetAllBranchesBasicAsync()
        {
            var branches = await _branchRepo.GetAllAsync();
            if(branches != null)
            {
                branches = branches.Where(b=>b.IsActive==true); 
            }    
            return branches;
        }


        public async Task<(IEnumerable<BranchReadDto> Branches, int TotalCount)>
        GetAllBranchesAsync(int page, int pageSize, string? search, string? Province, bool? isActive)
        {
            var query = _branchRepo.Query(); // trả về IQueryable<Branch>

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.BranchName.Contains(search));
            }

            if (!string.IsNullOrEmpty(Province))
            {
                query = query.Where(b => b.Province == Province);
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var branches = await query
                 .OrderBy(b => b.BranchName) // or CreatedAt, Name, etc.
                 .Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .AsSplitQuery()
                 .ToListAsync();

            return (_mapper.Map<IEnumerable<BranchReadDto>>(branches), totalCount);
        }

        public async Task<IEnumerable<object>> GetTechniciansWithWorkloadByBranchAsync(Guid branchId)
        {
            // Get all technicians in the branch
            var technicians = await _context.Technicians
                .Include(t => t.User)
                .Include(t => t.JobTechnicians)
                    .ThenInclude(jt => jt.Job)
                .Where(t => t.User.BranchId == branchId)
                .ToListAsync();

            var result = technicians.Select(t => new
            {
                Id = t.UserId,
                TechnicianId = t.TechnicianId,
                FullName = $"{t.User.FirstName} {t.User.LastName}".Trim(),
                Email = t.User.Email,
                IsActive = t.User.IsActive,
                CreatedAt = t.User.CreatedAt,
                LastLogin = t.User.LastLogin,
                BranchId = t.User.BranchId,
                InProgressTasks = t.JobTechnicians.Count(jt => jt.Job.Status == BusinessObject.Enums.JobStatus.InProgress),
                PendingTasks = t.JobTechnicians.Count(jt => jt.Job.Status == BusinessObject.Enums.JobStatus.Pending),
                TotalActiveTasks = t.JobTechnicians.Count(jt => 
                    jt.Job.Status == BusinessObject.Enums.JobStatus.InProgress || 
                    jt.Job.Status == BusinessObject.Enums.JobStatus.Pending)
            }).ToList();

            return result;
        }

        private async Task EnsureNoConflictingRepairRequestsAsync(
            Branch branch,
            IList<OperatingHourDto> newOperatingHours)
        {
            var now = DateTime.UtcNow;

            // Chỉ lấy các request đã được accept và còn trong tương lai
            var futureAcceptedRequests = await _context.RepairRequests
                .Where(r => r.BranchId == branch.BranchId
                            && r.Status == RepairRequestStatus.Accept
                            && r.RequestDate >= now)
                .ToListAsync();

            foreach (var request in futureAcceptedRequests)
            {
                // Map từ System.DayOfWeek sang DayOfWeekEnum custom
                DayOfWeekEnum dayEnum = request.RequestDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => DayOfWeekEnum.Monday,
                    DayOfWeek.Tuesday => DayOfWeekEnum.Tuesday,
                    DayOfWeek.Wednesday => DayOfWeekEnum.Wednesday,
                    DayOfWeek.Thursday => DayOfWeekEnum.Thursday,
                    DayOfWeek.Friday => DayOfWeekEnum.Friday,
                    DayOfWeek.Saturday => DayOfWeekEnum.Saturday,
                    DayOfWeek.Sunday => DayOfWeekEnum.Sunday,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var oldOh = branch.OperatingHours
                    .FirstOrDefault(o => o.DayOfWeek == dayEnum);

                var newOh = newOperatingHours
                    .FirstOrDefault(o => o.DayOfWeek == dayEnum);

                // Nếu trước đó không có giờ mở cửa cho ngày này thì bỏ qua
                if (oldOh == null || !oldOh.IsOpen || !oldOh.OpenTime.HasValue || !oldOh.CloseTime.HasValue)
                    continue;

                var bookingTime = request.RequestDate.TimeOfDay;

                bool wasWithinOld =
                    bookingTime >= oldOh.OpenTime.Value &&
                    bookingTime <= oldOh.CloseTime.Value;

                // Giờ mới: nếu không set hoặc IsOpen=false thì coi như đóng
                bool isWithinNew = newOh != null &&
                                   newOh.IsOpen &&
                                   newOh.OpenTime.HasValue &&
                                   newOh.CloseTime.HasValue &&
                                   bookingTime >= newOh.OpenTime.Value &&
                                   bookingTime <= newOh.CloseTime.Value;

                
                if (wasWithinOld && !isWithinNew)
                {
                    
                    var msg =
                        $"Cannot update operating hours because there is an accepted repair request " +
                        $"on {request.RequestDate:dddd, yyyy-MM-dd 'at' HH:mm} " +
                        $"that would fall outside the new opening hours.";

                    throw new ApplicationException(msg);
                }
            }
        }
        //emeer
        public async Task<Branch> GetBranchByEmergencyAsync(Guid id)
        {
            var branch = await _branchRepo.GetByIdAsync(id);
            return branch;
        }
    }
}
