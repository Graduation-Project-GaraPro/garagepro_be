using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Branches;
using DataAccessLayer;
using Dtos.Branches;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.BranchRepositories;
using Repositories.ServiceRepositories;

namespace Services.BranchServices
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IOperatingHourRepository _operatingHourRepo;
        private readonly IUserRepository _userRepository;

        private readonly MyAppDbContext _context;
        private readonly IMapper _mapper;

        public BranchService(
            IBranchRepository branchRepo,
            IMapper mapper,
            IServiceRepository serviceRepo,
            IUserRepository userRepository,
            IOperatingHourRepository operatingHourRepo,
            MyAppDbContext context)
        {
            _branchRepo = branchRepo;
            _serviceRepo = serviceRepo;
            _userRepository = userRepository;
            _operatingHourRepo = operatingHourRepo;
            _context = context;
            _mapper = mapper;
        }

        // CREATE
        // CREATE
        public async Task<BranchReadDto?> CreateBranchAsync(BranchCreateDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // === Check duplicate when CREATE ===
                bool isNameDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchName.ToLower().Trim() == dto.BranchName.ToLower().Trim()
                );

                bool isAddressDuplicate = await _branchRepo.ExistsAsync(
                    b => b.Street.ToLower().Trim() == dto.Street.ToLower().Trim()
                      && b.Ward.ToLower().Trim() == dto.Ward.ToLower().Trim()
                      && b.District.ToLower().Trim() == dto.District.ToLower().Trim()
                      && b.City.ToLower().Trim() == dto.City.ToLower().Trim()
                );

                if (isNameDuplicate)
                {
                    throw new ApplicationException("A branch with the same name already exists.");
                }

                if (isAddressDuplicate)
                {
                    throw new ApplicationException("A branch with the same address already exists.");
                }




                var days = dto.OperatingHours
                            .Select(o => o.DayOfWeek)
                            .Distinct()
                            .OrderBy(d => d)
                            .ToList();

                // Kiểm tra đủ 7 ngày
                if (days.Count != 7 || !Enumerable.Range(1, 7).All(d => days.Contains((BusinessObject.Enums.DayOfWeekEnum)d)))
                {
                    throw new ApplicationException("Operating hours must cover all 7 days (Monday to Sunday).");
                }

                // Kiểm tra nếu IsOpen == true thì OpenTime và CloseTime không được null
                foreach (var op in dto.OperatingHours)
                {
                    if (op.IsOpen)
                    {
                        if (!op.OpenTime.HasValue || !op.CloseTime.HasValue)
                        {
                            throw new ApplicationException($"Operating hours for {op.DayOfWeek} must have both OpenTime and CloseTime when IsOpen is true.");
                        }
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
                    Ward = dto.Ward,
                    District = dto.District,
                    City = dto.City,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    BranchServices = dto.ServiceIds
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

                if (dto.StaffIds != null && dto.StaffIds.Any())
                {
                    var Users = await _userRepository.GetAllAsync();
                    var staffs = Users.Where(u => dto.StaffIds.Contains(u.Id));

                    foreach (var user in staffs)
                    {
                        user.BranchId = branchId;
                        branch.Staffs.Add(user);
                    }
                }

                await _branchRepo.AddAsync(branch);
                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();

                return _mapper.Map<BranchReadDto>(branch);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new ApplicationException("Error creating branch " + ex.Message, ex);
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

                // Check duplicate name
                bool isNameDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchId != dto.BranchId &&
                         b.BranchName.ToLower().Trim() == dto.BranchName.ToLower().Trim()
                );

                // Check duplicate address
                bool isAddressDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchId != dto.BranchId &&
                         b.Street.ToLower().Trim() == dto.Street.ToLower().Trim() &&
                         b.Ward.ToLower().Trim() == dto.Ward.ToLower().Trim() &&
                         b.District.ToLower().Trim() == dto.District.ToLower().Trim() &&
                         b.City.ToLower().Trim() == dto.City.ToLower().Trim()
                );

                if (isNameDuplicate)
                {
                    throw new ApplicationException("Branch name already exists.");
                }

                if (isAddressDuplicate)
                {
                    throw new ApplicationException("Branch address already exists.");
                }

                // Validate operating hours: phải đủ 7 ngày

                var days = dto.OperatingHours
                            .Select(o => o.DayOfWeek)
                            .Distinct()
                            .OrderBy(d => d)
                            .ToList();

                // Kiểm tra đủ 7 ngày
                if (days.Count != 7 || !Enumerable.Range(1, 7).All(d => days.Contains((BusinessObject.Enums.DayOfWeekEnum)d)))
                {
                    throw new ApplicationException("Operating hours must cover all 7 days (Monday to Sunday).");
                }

                // Kiểm tra nếu IsOpen == true thì OpenTime và CloseTime không được null
                foreach (var op in dto.OperatingHours)
                {
                    if (op.IsOpen)
                    {
                        if (!op.OpenTime.HasValue || !op.CloseTime.HasValue)
                        {
                            throw new ApplicationException($"Operating hours for {op.DayOfWeek} must have both OpenTime and CloseTime when IsOpen is true.");
                        }
                    }
                }

                // Update các field cơ bản
                branch.BranchName = dto.BranchName;
                branch.PhoneNumber = dto.PhoneNumber;
                branch.Email = dto.Email;
                branch.Street = dto.Street;
                branch.Ward = dto.Ward;
                branch.District = dto.District;
                branch.City = dto.City;
                branch.Description = dto.Description;
                branch.IsActive = dto.IsActive;
                branch.UpdatedAt = DateTime.UtcNow;

                // ========================
                // Update Services (diff)
                // ========================
                var currentServiceIds = branch.BranchServices.Select(bs => bs.ServiceId).ToList();
                var newServiceIds = dto.ServiceIds;

                // Xoá service không còn
                foreach (var bs in branch.BranchServices.Where(bs => !newServiceIds.Contains(bs.ServiceId)).ToList())
                {
                    branch.BranchServices.Remove(bs);
                }

                // Thêm service mới
                foreach (var sid in newServiceIds.Except(currentServiceIds))
                {
                    branch.BranchServices.Add(new BusinessObject.Branches.BranchService
                    {
                        BranchId = branch.BranchId,
                        ServiceId = sid
                    });
                }

                // ========================
                // Update Operating Hours (diff)
                // ========================
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

                // Xoá operating hour không còn trong DTO
                foreach (var oh in branch.OperatingHours.Where(o => !dto.OperatingHours.Any(d => d.DayOfWeek == o.DayOfWeek)).ToList())
                {
                    branch.OperatingHours.Remove(oh);
                }

                // ========================
                // Update Staffs (diff)
                // ========================
                if (dto.StaffIds != null)
                {
                    var currentStaffIds = branch.Staffs.Select(s => s.Id).ToList();
                    var newStaffIds = dto.StaffIds.ToList();

                    // Staff bị remove → set BranchId = null
                    foreach (var staff in branch.Staffs.Where(s => !newStaffIds.Contains(s.Id)).ToList())
                    {
                        staff.BranchId = null;
                        branch.Staffs.Remove(staff);
                    }

                    // Staff mới → gán BranchId
                    var users = await _userRepository.GetAllAsync();
                    var newStaffs = users.Where(u => newStaffIds.Except(currentStaffIds).Contains(u.Id));
                    foreach (var user in newStaffs)
                    {
                        user.BranchId = branch.BranchId;
                        branch.Staffs.Add(user);
                    }
                }

                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();

                return _mapper.Map<BranchReadDto>(branch);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new ApplicationException("Error updating branch", ex);
            }
        }



        // DELETE
        public async Task<bool> DeleteBranchAsync(Guid branchId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = await _branchRepo.GetByIdAsync(branchId);
                if (branch == null) return false;

                await _branchRepo.DeleteAsync(branch);
                await _branchRepo.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new ApplicationException("Error deleting branch", ex);
            }
        }

        // GET BY ID
        public async Task<BranchReadDto?> GetBranchByIdAsync(Guid id)
        {
            var branch = await _branchRepo.GetBranchWithRelationsAsync(id);
            return branch == null ? null : _mapper.Map<BranchReadDto>(branch);
        }

        // GET ALL
        public async Task<IEnumerable<BranchReadDto>> GetAllBranchesAsync()
        {
            var branches = await _branchRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<BranchReadDto>>(branches);
        }


        public async Task<(IEnumerable<BranchReadDto> Branches, int TotalCount)>
        GetAllBranchesAsync(int page, int pageSize, string? search, string? city, bool? isActive)
        {
            var query = _branchRepo.Query(); // trả về IQueryable<Branch>

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.BranchName.Contains(search));
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(b => b.City == city);
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var branches = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (_mapper.Map<IEnumerable<BranchReadDto>>(branches), totalCount);
        }
    }



}
