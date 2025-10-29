﻿using System;
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
                    throw new ApplicationException("A branch with the same name already exists.");

                if (isAddressDuplicate)
                    throw new ApplicationException("A branch with the same address already exists.");

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
                    if (days.Distinct().Count() != 7 || !Enumerable.Range(1, 7).All(d => days.Contains((BusinessObject.Enums.DayOfWeekEnum)d)))
                    {
                        throw new ApplicationException("Operating hours must cover all 7 days (Monday to Sunday) with no duplicates.");
                    }

                    // Must have at least one day open
                    if (!dto.OperatingHours.Any(o => o.IsOpen))
                    {
                        throw new ApplicationException("At least one day must be open (IsOpen = true).");
                    }

                    // Validate Open/Close time
                    foreach (var op in dto.OperatingHours)
                    {
                        if (op.IsOpen && (!op.OpenTime.HasValue || !op.CloseTime.HasValue))
                        {
                            throw new ApplicationException($"Operating hours for {op.DayOfWeek} must have both OpenTime and CloseTime when IsOpen is true.");
                        }
                    }

                    // Optional: ensure only Monday → Sunday
                    if (dto.OperatingHours.Any(o => (int)o.DayOfWeek < 1 || (int)o.DayOfWeek > 7))
                    {
                        throw new ApplicationException("Operating hours can only be set from Monday to Sunday.");
                    }

                // === Validate Services exist ===
                var existingServiceIds = await _serviceRepo.Query()
                    .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                    .Select(s => s.ServiceId)
                    .ToListAsync();

                var missingServices = dto.ServiceIds.Except(existingServiceIds).ToList();
                if (missingServices.Any())
                {
                    throw new ApplicationException($"Some service IDs do not exist: {string.Join(", ", missingServices)}");
                }

               

                // === Validate Staffs exist and are Managers/Technicians ===
                var allowedUsers = await _userRepository.GetManagersAndTechniciansAsync();

                var validStaffs = allowedUsers
                    .Where(u => dto.StaffIds != null && dto.StaffIds.Contains(u.Id))
                    .ToList();

                if (dto.StaffIds != null && dto.StaffIds.Any() && validStaffs.Count != dto.StaffIds.Count)
                {
                    var invalidStaffIds = dto.StaffIds.Except(validStaffs.Select(u => u.Id));
                    throw new ApplicationException($"Some staff IDs are invalid or not allowed: {string.Join(", ", invalidStaffIds)}");
                }


                // === Create branch ===
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

                // === Check duplicate name/address ===
                bool isNameDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchId != dto.BranchId &&
                         b.BranchName.ToLower().Trim() == dto.BranchName.ToLower().Trim()
                );

                bool isAddressDuplicate = await _branchRepo.ExistsAsync(
                    b => b.BranchId != dto.BranchId &&
                         b.Street.ToLower().Trim() == dto.Street.ToLower().Trim() &&
                         b.Ward.ToLower().Trim() == dto.Ward.ToLower().Trim() &&
                         b.District.ToLower().Trim() == dto.District.ToLower().Trim() &&
                         b.City.ToLower().Trim() == dto.City.ToLower().Trim()
                );

                if (isNameDuplicate) throw new ApplicationException("Branch name already exists.");
                if (isAddressDuplicate) throw new ApplicationException("Branch address already exists.");

                // === Validate operating hours ===
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
                if (days.Distinct().Count() != 7 || !Enumerable.Range(1, 7).All(d => days.Contains((BusinessObject.Enums.DayOfWeekEnum)d)))
                {
                    throw new ApplicationException("Operating hours must cover all 7 days (Monday to Sunday) with no duplicates.");
                }

                // Must have at least one day open
                if (!dto.OperatingHours.Any(o => o.IsOpen))
                {
                    throw new ApplicationException("At least one day must be open (IsOpen = true).");
                }

                // Validate Open/Close time
                foreach (var op in dto.OperatingHours)
                {
                    if (op.IsOpen && (!op.OpenTime.HasValue || !op.CloseTime.HasValue))
                    {
                        throw new ApplicationException($"Operating hours for {op.DayOfWeek} must have both OpenTime and CloseTime when IsOpen is true.");
                    }
                }

                // Optional: ensure only Monday → Sunday
                if (dto.OperatingHours.Any(o => (int)o.DayOfWeek < 1 || (int)o.DayOfWeek > 7))
                {
                    throw new ApplicationException("Operating hours can only be set from Monday to Sunday.");
                }

                // === Validate services exist ===
                var existingServiceIds = await _serviceRepo.Query()
                    .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                    .Select(s => s.ServiceId)
                    .ToListAsync();

                var missingServices = dto.ServiceIds.Except(existingServiceIds).ToList();
                if (missingServices.Any())
                    throw new ApplicationException($"Some service IDs do not exist: {string.Join(", ", missingServices)}");

                // === Validate staffs exist and are Managers/Technicians ===
                var allowedUsers = await _userRepository.GetManagersAndTechniciansAsync();
                var validStaffs = allowedUsers.Where(u => dto.StaffIds != null && dto.StaffIds.Contains(u.Id)).ToList();
                if (dto.StaffIds != null && dto.StaffIds.Any() && validStaffs.Count != dto.StaffIds.Count)
                {
                    var invalidStaffIds = dto.StaffIds.Except(validStaffs.Select(u => u.Id));
                    throw new ApplicationException($"Some staff IDs are invalid or not allowed: {string.Join(", ", invalidStaffIds)}");
                }

                // === Update basic fields ===
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

                // === Update Services (diff) ===
                var currentServiceIds = branch.BranchServices.Select(bs => bs.ServiceId).ToList();
                foreach (var bs in branch.BranchServices.Where(bs => !existingServiceIds.Contains(bs.ServiceId)).ToList())
                {
                    branch.BranchServices.Remove(bs);
                }
                foreach (var sid in existingServiceIds.Except(currentServiceIds))
                {
                    branch.BranchServices.Add(new BusinessObject.Branches.BranchService
                    {
                        BranchId = branch.BranchId,
                        ServiceId = sid
                    });
                }

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
                // Remove operating hours not in DTO
                foreach (var oh in branch.OperatingHours.Where(o => !dto.OperatingHours.Any(d => d.DayOfWeek == o.DayOfWeek)).ToList())
                {
                    branch.OperatingHours.Remove(oh);
                }

                // === Update Staffs (diff) ===
                var currentStaffIds = branch.Staffs.Select(s => s.Id).ToList();
                // Remove staff no longer assigned
                foreach (var staff in branch.Staffs.Where(s => !validStaffs.Select(u => u.Id).Contains(s.Id)).ToList())
                {
                    staff.BranchId = null;
                    branch.Staffs.Remove(staff);
                }
                // Add new valid staffs
                foreach (var user in validStaffs.Where(u => !currentStaffIds.Contains(u.Id)))
                {
                    user.BranchId = branch.BranchId;
                    branch.Staffs.Add(user);
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
                // Lấy tất cả branch theo ID
                var branches = await _branchRepo.Query()
                    .Include(b => b.RepairOrders)
                    .Include(b => b.RepairRequests)
                    .Where(b => branchIds.Contains(b.BranchId))
                    .ToListAsync();

                // Check branch ID tồn tại
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

                // Xóa những branch hợp lệ
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

        // GET ALL
        public async Task<IEnumerable<BranchReadDto>> GetAllBranchesAsync()
        {
            var branches = await _branchRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<BranchReadDto>>(branches);
        }
        public async Task<IEnumerable<Branch>> GetAllBranchesBasicAsync()
        {
            var branches = await _branchRepo.GetAllAsync();
            return branches;
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