using AutoMapper;
using BusinessObject;
using BusinessObject.Customers;
using BusinessObject.Enums;
using Dtos.Customers;
using Dtos.Parts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories;
using Repositories.Customers;
using Repositories.UnitOfWork;
using Services.Cloudinaries;
using Services.VehicleServices; // Add this
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.RepairRequests;
using Dtos.RepairOrder; // Add this
using Services; // Add this for IRepairOrderService



namespace Services.Customer
{
    public class RepairRequestService : IRepairRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly IRepairRequestRepository _repairRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRepairOrderService _repairOrderService; // Add this
        private readonly IVehicleService _vehicleService; // Add this


        private static readonly RepairRequestStatus[] ActiveStatuses =
            { RepairRequestStatus.Pending, RepairRequestStatus.Accept };
        private static readonly int[] ActiveOrderStatusIds = { 1, 2 };
        public RepairRequestService(
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            IMapper mapper,
            IRepairOrderService repairOrderService, // Add this
            IVehicleService vehicleService) // Add this
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _repairOrderService = repairOrderService; // Add this
            _vehicleService = vehicleService; // Add this
        }

        public async Task<IEnumerable<RepairRequestDto>> GetAllAsync()
        {
            var requests = await _unitOfWork.RepairRequests.GetAllAsync();
            return _mapper.Map<IEnumerable<RepairRequestDto>>(requests);
        }

        public async Task<object> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? vehicleId = null,
            RepairRequestStatus? status = null,
            Guid? branchId = null,
            string? userId = null)
        {
            var query = _unitOfWork.RepairRequests.GetQueryable();

            // ✅ Lọc theo UserID đang đăng nhập
            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(r => r.UserID == userId);

            // ✅ Lọc theo VehicleId
            if (vehicleId.HasValue)
                query = query.Where(r => r.VehicleID == vehicleId.Value);

            // ✅ Lọc theo Status
            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            // ✅ Lọc theo BranchId
            if (branchId.HasValue)
                query = query.Where(r => r.BranchId == branchId.Value);

            // ✅ Đếm tổng bản ghi
            var totalCount = await query.CountAsync();

            // ✅ Sort theo RequestDate (mới nhất trước)
            var data = await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RepairRequestDto
                {
                    RepairRequestID = r.RepairRequestID,
                    VehicleID = r.VehicleID,
                    UserID = r.UserID,
                    Description = r.Description,
                    BranchId = r.BranchId,
                    RequestDate = r.RequestDate,
                    CompletedDate = r.CompletedDate,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    EstimatedCost = r.EstimatedCost
                })
                .ToListAsync();

            // ✅ Kết quả phân trang
            return new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Data = data
            };
        }

        public async Task<IEnumerable<RepairRequestDto>> GetByUserIdAsync(string userId)
        {
            var requests = await _unitOfWork.RepairRequests.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<RepairRequestDto>>(requests);
        }

        // New method for managers
        public async Task<IEnumerable<ManagerRepairRequestDto>> GetForManagerAsync()
        {
            var requests = await _repairRequestRepository.GetAllAsync();
            var managerDtos = new List<ManagerRepairRequestDto>();

            foreach (var request in requests)
            {
                var customer = await _userRepository.GetByIdAsync(request.UserID);
                var vehicle = request.Vehicle;
                
                var dto = new ManagerRepairRequestDto
                {
                    RequestID = request.RepairRequestID,
                    VehicleID = request.VehicleID,
                    CustomerID = request.UserID,
                    CustomerName = customer?.FullName ?? "Unknown Customer",
                    VehicleInfo = $"{vehicle?.Brand?.BrandName ?? "Unknown"} {vehicle?.Model?.ModelName ?? "Unknown Model"}",
                    Description = request.Description,
                    RequestDate = request.RequestDate,
                    CompletedDate = request.CompletedDate,
                    ImageUrls = request.RepairImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(),
                    Services = _mapper.Map<List<RequestServiceDto>>(request.RequestServices?.ToList() ?? new List<RequestService>()),
                    CreatedAt = request.CreatedAt,
                    UpdatedAt = request.UpdatedAt
                };

                managerDtos.Add(dto);
            }

            return managerDtos;
        }

        // New method for getting a single request for managers
        public async Task<ManagerRepairRequestDto> GetManagerRequestByIdAsync(Guid id)
        {
            var request = await _repairRequestRepository.GetByIdWithDetailsAsync(id);
            if (request == null)
                return null;

            var customer = await _userRepository.GetByIdAsync(request.UserID);
            var vehicle = request.Vehicle;
            
            var dto = new ManagerRepairRequestDto
            {
                RequestID = request.RepairRequestID,
                VehicleID = request.VehicleID,
                CustomerID = request.UserID,
                CustomerName = customer?.FullName ?? "Unknown Customer",
                VehicleInfo = $"{vehicle?.Brand?.BrandName ?? "Unknown"} {vehicle?.Model?.ModelName ?? "Unknown Model"}",
                Description = request.Description,
                RequestDate = request.RequestDate,
                CompletedDate = request.CompletedDate,
                ImageUrls = request.RepairImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(),
                Services = _mapper.Map<List<RequestServiceDto>>(request.RequestServices?.ToList() ?? new List<RequestService>()),
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };

            return dto;
        }

        public async Task<RPDetailDto> GetByIdDetailsAsync(Guid id)
        {
            var request = await _unitOfWork.RepairRequests.GetByIdWithDetailsAsync(id);
            return _mapper.Map<RPDetailDto>(request);
        }

        public async Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(dto.VehicleID);
            if (vehicle == null || vehicle.UserId != userId)
                throw new Exception("This vehicle does not belong to the current user");

            var repairRequest = new RepairRequest
            {
                VehicleID = dto.VehicleID,
                UserID = userId,
                BranchId = dto.BranchId,
                Description = dto.Description,
                RequestDate = dto.RequestDate,
                EstimatedCost = 0,
                RequestServices = new List<RequestService>()
            };

            decimal totalServiceFee = 0;
            decimal totalPartsFee = 0;

            foreach (var serviceDto in dto.Services)
            {
                var service = await _unitOfWork.Services.GetByIdAsync(serviceDto.ServiceId)
                              ?? throw new Exception($"Service {serviceDto.ServiceId} not found");

                var requestService = new RequestService
                {
                    ServiceId = service.ServiceId,
                    ServiceFee = service.Price,
                    RequestParts = new List<RequestPart>()
                };

                totalServiceFee += service.Price;

                if (serviceDto.Parts != null)
                {
                    foreach (var partDto in serviceDto.Parts)
                    {
                        var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId)
                                   ?? throw new Exception($"Part {partDto.PartId} not found");

                        var requestPart = new RequestPart
                        {
                            PartId = part.PartId,
                            UnitPrice = part.Price,
                        };

                        totalPartsFee += requestPart.UnitPrice;
                        requestService.RequestParts.Add(requestPart);
                    }
                }

                repairRequest.RequestServices.Add(requestService);
            }

            repairRequest.EstimatedCost = totalServiceFee + totalPartsFee;

            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var imageUrl in dto.ImageUrls)
                {
                    repairRequest.RepairImages.Add(new RepairImage
                    {
                        ImageId = Guid.NewGuid(),
                        ImageUrl = imageUrl,
                        RepairRequestId = repairRequest.RepairRequestID
                    });
                }
            }

            await _unitOfWork.RepairRequests.AddAsync(repairRequest);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RepairRequestDto>(repairRequest);
        }

        public async Task<RepairRequestDto> UpdateRepairRequestAsync(Guid requestId, UpdateRepairRequestDto dto, string userId)
        {
            var repairRequest = await _unitOfWork.RepairRequests.GetTrackingByIdAsync(requestId)
                ?? throw new Exception("Repair request not found");

            if (repairRequest.UserID != userId)
                throw new Exception("You are not allowed to update this request");

            if (repairRequest.Status != RepairRequestStatus.Pending)
                throw new Exception("Cannot update a request that is already being processed.");

            // --- Cập nhật các trường cơ bản ---
            if (!string.IsNullOrEmpty(dto.Description))
                repairRequest.Description = dto.Description;

            if (dto.RequestDate.HasValue)
                repairRequest.RequestDate = dto.RequestDate.Value;

            // --- Cập nhật hình ảnh ---
            if (dto.ImageUrls != null)
            {
                // Lấy danh sách image hiện tại từ DB
                var currentImages = await _unitOfWork.RepairImages
                    .GetByRepairRequestIdAsync(repairRequest.RepairRequestID);

                // Xóa những image không còn trong DTO
                var imagesToRemove = currentImages
                    .Where(img => !dto.ImageUrls.Contains(img.ImageUrl))
                    .ToList();

                foreach (var img in imagesToRemove)
                    _unitOfWork.RepairImages.Remove(img);

                // Thêm những image mới mà chưa có
                var existingUrls = currentImages.Select(img => img.ImageUrl).ToHashSet();
                foreach (var url in dto.ImageUrls.Where(url => !existingUrls.Contains(url)))
                {
                    var newImage = new RepairImage
                    {
                        ImageId = Guid.NewGuid(),
                        ImageUrl = url,
                        RepairRequestId = repairRequest.RepairRequestID
                    };
                    await _unitOfWork.RepairImages.AddAsync(newImage);
                }
            }


            // --- Cập nhật services và parts ---
            if (dto.Services != null)
            {
                var currentServices = repairRequest.RequestServices.ToList();
                var dtoServiceIds = dto.Services.Select(s => s.ServiceId).ToHashSet();

                // 2a. Xóa service không còn trong DTO
                foreach (var rs in currentServices.Where(s => !dtoServiceIds.Contains(s.ServiceId)))
                {
                    repairRequest.RequestServices.Remove(rs);
                    await _unitOfWork.RequestServices.DeleteAsync(rs.RequestServiceId);
                }

                decimal totalServiceFee = 0;
                decimal totalPartsFee = 0;

                // 2b. Update hoặc thêm service mới
                foreach (var serviceDto in dto.Services)
                {
                    var existingService = repairRequest.RequestServices
                        .FirstOrDefault(s => s.ServiceId == serviceDto.ServiceId);

                    if (existingService == null)
                    {
                        // Thêm service mới
                        var service = await _unitOfWork.Services.GetByIdAsync(serviceDto.ServiceId)
                                      ?? throw new Exception($"Service {serviceDto.ServiceId} not found");

                        var requestService = new RequestService
                        {
                            RequestServiceId = Guid.NewGuid(),
                            ServiceId = service.ServiceId,
                            ServiceFee = service.Price,
                            RepairRequestId = repairRequest.RepairRequestID,
                            RequestParts = new List<RequestPart>()
                        };

                        totalServiceFee += service.Price;

                        // Thêm parts nếu có
                        if (serviceDto.Parts != null)
                        {
                            foreach (var partDto in serviceDto.Parts)
                            {
                                var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId)
                                           ?? throw new Exception($"Part {partDto.PartId} not found");

                                requestService.RequestParts.Add(new RequestPart
                                {
                                    RequestPartId = Guid.NewGuid(),
                                    PartId = part.PartId,
                                    UnitPrice = part.Price,
                                    RequestServiceId = requestService.RequestServiceId
                                });

                                totalPartsFee += part.Price;
                            }
                        }

                        repairRequest.RequestServices.Add(requestService);
                    }
                    else
                    {
                        // Update parts cho service đã tồn tại
                        var dtoParts = serviceDto.Parts ?? new List<RequestPartDto>();
                        var existingPartIds = existingService.RequestParts.Select(p => p.PartId).ToHashSet();

                        // Xóa parts không còn trong DTO
                        var partsToRemove = existingService.RequestParts
                            .Where(p => !dtoParts.Any(dp => dp.PartId == p.PartId))
                            .ToList();

                        foreach (var part in partsToRemove)
                            existingService.RequestParts.Remove(part);

                        // Thêm parts mới
                        foreach (var partDto in dtoParts)
                        {
                            if (!existingPartIds.Contains(partDto.PartId))
                            {
                                var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId)
                                           ?? throw new Exception($"Part {partDto.PartId} not found");

                                existingService.RequestParts.Add(new RequestPart
                                {
                                    RequestPartId = Guid.NewGuid(),
                                    PartId = part.PartId,
                                    UnitPrice = part.Price,
                                    RequestServiceId = existingService.RequestServiceId
                                });
                            }
                        }

                        totalServiceFee += existingService.ServiceFee;
                        totalPartsFee += existingService.RequestParts.Sum(p => p.UnitPrice);
                    }
                }

                repairRequest.EstimatedCost = totalServiceFee + totalPartsFee;
            }



            repairRequest.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RepairRequestDto>(repairRequest);
        }

        public async Task<RepairRequestDto> CreateRepairWithImageRequestAsync(CreateRepairRequestWithImageDto dto, string userId)
        {
            // 🔹 Kiểm tra quyền sở hữu xe
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(dto.VehicleID);
            if (vehicle == null || vehicle.UserId != userId)
                throw new Exception("This vehicle does not belong to the current user");


            var branch = await _unitOfWork.Branches.GetByIdAsync(dto.BranchId)
                ?? throw new Exception("Branch not found");
            if (!branch.IsActive) throw new Exception("Branch inactive");

            var windowMin = branch.ArrivalWindowMinutes > 0 ? branch.ArrivalWindowMinutes : 30;
            var (winStart, _) = WindowRange(dto.RequestDate, windowMin);

            await EnsureWithinOperatingHoursAsync(dto.BranchId, winStart, windowMin);

            // (A) Cooldown theo user
            var now = DateTimeOffset.Now.ToOffset(VietnamTime.VN_OFFSET);
            var cooldownCutoff = now - RepairRequestAppConfig.CreateCooldown;

            var recentCount = await _unitOfWork.RepairRequests.CountAsync(x =>
                x.UserID == userId && x.CreatedAt >= cooldownCutoff.UtcDateTime); // CreatedAt lưu UTC → so theo UTC

            if (recentCount > 0)
                throw new Exception($"Bạn vừa tạo yêu cầu trước đó. Vui lòng thử lại sau {(int)RepairRequestAppConfig.CreateCooldown.TotalMinutes} phút.");

            // (B) Giới hạn số request đang hoạt động của user
            var activeOfUser = await _unitOfWork.RepairRequests.CountAsync(x =>
                x.UserID == userId && ActiveStatuses.Contains(x.Status));

            if (activeOfUser >= RepairRequestAppConfig.MaxActiveRequestsPerUser)
                throw new Exception("Bạn đã đạt giới hạn số yêu cầu đang hoạt động. Hãy hoàn tất/hủy bớt yêu cầu trước.");

            // (C) Giới hạn theo vehicle trong 1 ngày (VN)
            var dayStart = new DateTimeOffset(winStart.Year, winStart.Month, winStart.Day, 0, 0, 0, VietnamTime.VN_OFFSET);
            var dayEnd = dayStart.AddDays(1);

            var vehicleDaily = await _unitOfWork.RepairRequests.CountAsync(x =>
                x.VehicleID == dto.VehicleID
                && x.ArrivalWindowStart >= dayStart
                && x.ArrivalWindowStart < dayEnd);

            if (vehicleDaily >= RepairRequestAppConfig.MaxRequestsPerVehiclePerDay)
                throw new Exception("Xe này đã đạt giới hạn số yêu cầu trong ngày. Vui lòng chọn ngày khác.");

            // (D) Chặn trùng slot: User + Vehicle + Branch + Slot + Status in (Pending,Accept)
            var dup = await _unitOfWork.RepairRequests.AnyAsync(x =>
                x.UserID == userId
                && x.VehicleID == dto.VehicleID
                && x.BranchId == dto.BranchId
                && ActiveStatuses.Contains(x.Status)
                && x.ArrivalWindowStart == winStart);

            if (dup)
                throw new Exception("Bạn đã có một yêu cầu cho khung giờ này. Vui lòng chọn khung khác.");






            // 🔹 Khởi tạo đối tượng RepairRequest
            var repairRequest = new RepairRequest
            {
                VehicleID = dto.VehicleID,
                UserID = userId,
                BranchId = dto.BranchId,
                Description = dto.Description,
                RequestDate = dto.RequestDate,
                ArrivalWindowStart = winStart,
                EstimatedCost = 0,
                Status = RepairRequestStatus.Accept,
                RequestServices = new List<RequestService>(),
                RepairImages = new List<RepairImage>()
            };

            decimal totalServiceFee = 0;
            decimal totalPartsFee = 0;

            // 🔹 Duyệt danh sách service khách chọn
            foreach (var serviceDto in dto.Services)
            {
                var service = await _unitOfWork.Services.GetByIdAsync(serviceDto.ServiceId)
                              ?? throw new Exception($"Service {serviceDto.ServiceId} not found");

                var requestService = new RequestService
                {
                    ServiceId = service.ServiceId,
                    ServiceFee = service.Price,
                    RequestParts = new List<RequestPart>()
                };

                totalServiceFee += service.Price;

                // 🔹 Nếu có parts kèm theo service
                if (serviceDto.Parts != null)
                {
                    foreach (var partDto in serviceDto.Parts)
                    {
                        var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId)
                                   ?? throw new Exception($"Part {partDto.PartId} not found");

                        var requestPart = new RequestPart
                        {
                            PartId = part.PartId,
                            UnitPrice = part.Price,
                        };

                        totalPartsFee += part.Price;
                        requestService.RequestParts.Add(requestPart);
                    }
                }

                repairRequest.RequestServices.Add(requestService);
            }

            // 🔹 Tổng chi phí ước tính
            repairRequest.EstimatedCost = totalServiceFee + totalPartsFee;

            // 🔹 Upload ảnh (nếu có)
            if (dto.Images != null && dto.Images.Any())
            {
                var uploadedUrls = await _cloudinaryService.UploadImagesAsync(dto.Images);

                foreach (var url in uploadedUrls)
                {
                    repairRequest.RepairImages.Add(new RepairImage
                    {
                        ImageId = Guid.NewGuid(),
                        ImageUrl = url
                    });
                }
            }

            // 🔹 Lưu vào database
            await _unitOfWork.RepairRequests.AddAsync(repairRequest);
            await _unitOfWork.SaveChangesAsync();

            // 🔹 Map sang DTO trả về
            return _mapper.Map<RepairRequestDto>(repairRequest);
        }

        public async Task CheckInAsync(Guid repairRequestId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var rr = await _unitOfWork.RepairRequests.GetByIdAsync(repairRequestId)
                         ?? throw new Exception("RepairRequest not found");

                // Idempotent: nếu đã Arrived thì coi như xong
                if (rr.Status == RepairRequestStatus.Arrived)
                {
                    await _unitOfWork.CommitAsync();
                    return;
                }

                if (rr.Status != RepairRequestStatus.Accept)
                    throw new Exception("Chỉ check-in các yêu cầu đã được duyệt (Accept).");

                var branch = await _unitOfWork.Branches.GetByIdAsync(rr.BranchId)
                             ?? throw new Exception("Branch not found");

                // Đếm WIP đang trong xưởng
                var activeWip = await GetActiveWipCountAsync(rr.BranchId);
                if (activeWip >= branch.MaxConcurrentWip)
                    throw new Exception("Xưởng đang đầy, vui lòng chờ gọi theo thứ tự.");

                // Cho vào xưởng
                rr.Status = RepairRequestStatus.Arrived;
                rr.UpdatedAt = DateTime.UtcNow;

                // (tuỳ chọn) tạo RepairOrder skeleton
                // var ro = new RepairOrder { RepairRequestId = rr.RepairRequestID, BranchId = rr.BranchId, ... };
                // await _unitOfWork.RepairOrders.AddAsync(ro);

                await _unitOfWork.RepairRequests.UpdateAsync(rr);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task AcceptAsync(Guid repairRequestId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var rr = await _unitOfWork.RepairRequests.GetByIdAsync(repairRequestId)
                         ?? throw new Exception("RepairRequest not found");

                if (rr.Status == RepairRequestStatus.Accept)
                {
                    await _unitOfWork.CommitAsync(); // idempotent
                    return;
                }
                if (rr.Status == RepairRequestStatus.Cancelled)
                    throw new Exception("Request was cancelled.");

                var branch = await _unitOfWork.Branches.GetByIdAsync(rr.BranchId)
                             ?? throw new Exception("Branch not found");
                if (!branch.IsActive) throw new Exception("Branch inactive.");

                var windowMin = branch.ArrivalWindowMinutes > 0 ? branch.ArrivalWindowMinutes : 30;

                // Chuẩn hoá mốc slot (VN-only, rr.ArrivalWindowStart luôn +07:00)
                var slotStart = VietnamTime.NormalizeWindow(rr.ArrivalWindowStart, windowMin);
                var slotEnd = slotStart.AddMinutes(windowMin);

                // Bảo đảm nằm trong giờ làm việc của chi nhánh cho NGÀY đó
                await EnsureWithinOperatingHoursAsync(rr.BranchId, slotStart, windowMin);

                // Đếm số Accept trong cùng slot và chi nhánh
                var approvedCount = await _unitOfWork.RepairRequests.CountAsync(x =>
                    x.BranchId == rr.BranchId
                    && x.Status == RepairRequestStatus.Accept
                    && x.ArrivalWindowStart >= slotStart
                    && x.ArrivalWindowStart < slotEnd);

                if (approvedCount >= branch.MaxBookingsPerWindow)
                    throw new Exception("Cửa sổ đến đã đủ lượt duyệt.");

                // Cập nhật trạng thái & chuẩn hoá lại ArrivalWindowStart về đầu slot
                rr.Status = RepairRequestStatus.Accept;
                rr.ArrivalWindowStart = slotStart;
                rr.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.RepairRequests.UpdateAsync(rr);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<SlotAvailabilityDto>> GetArrivalAvailabilityAsync(Guid branchId, DateOnly date)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(branchId)
                         ?? throw new Exception("Branch not found");
            if (!branch.IsActive) return Array.Empty<SlotAvailabilityDto>();

            var windowMin = branch.ArrivalWindowMinutes > 0 ? branch.ArrivalWindowMinutes : 30;

            var dow = DowUtil.ToCustomDow((DayOfWeek)date.DayOfWeek);
            var oh = await _unitOfWork.OperatingHours.SingleOrDefaultAsync(o =>
                o.BranchId == branchId && o.DayOfWeek == dow);

            if (oh == null || !oh.IsOpen || !oh.OpenTime.HasValue || !oh.CloseTime.HasValue)
                return Array.Empty<SlotAvailabilityDto>();

            var (openLocal, closeLocal) = SlotWindowUtil.BuildOpenCloseLocal(date, oh.OpenTime.Value, oh.CloseTime.Value);
            var windows = SlotWindowUtil.GenerateWindows(openLocal, closeLocal, windowMin);
            if (windows.Count == 0) return Array.Empty<SlotAvailabilityDto>();

            // Query trực tiếp vì ArrivalWindowStart luôn lưu +07:00
            var accepts = await _unitOfWork.RepairRequests.ListByConditionAsync(x =>
                x.BranchId == branchId
                && x.Status == RepairRequestStatus.Accept
                && x.ArrivalWindowStart >= openLocal
                && x.ArrivalWindowStart < closeLocal);

            var usedMap = AvailabilityUtil.GroupAcceptsBySlot(
                accepts.Select(a => a.ArrivalWindowStart), windowMin);

            return AvailabilityUtil.Build(windows, usedMap, branch.MaxBookingsPerWindow);
        }



        public async Task<bool> DeleteRepairRequestAsync(Guid id)
        {
            var result = await _unitOfWork.RepairRequests.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

       

        public async Task<IEnumerable<RequestServiceDto>> GetServicesAsync(Guid repairRequestId)
        {
            var services = await _unitOfWork.RequestServices.GetByRepairRequestIdAsync(repairRequestId);
            return _mapper.Map<IEnumerable<RequestServiceDto>>(services);
        }

        public async Task<RequestServiceDto> AddServiceAsync(RequestServiceDto dto)
        {
            var service = _mapper.Map<RequestService>(dto);
            service.RequestServiceId = Guid.NewGuid();
            await _unitOfWork.RequestServices.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<RequestServiceDto>(service);
        }

        public async Task<bool> DeleteServiceAsync(Guid requestServiceId)
        {
            var result = await _unitOfWork.RequestServices.DeleteAsync(requestServiceId);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }



        private static DateTimeOffset NormalizeWindow(DateTimeOffset t, int windowMinutes)
        {
            if (windowMinutes <= 0) windowMinutes = 30;
            var epoch = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var mins = (long)(t.ToUniversalTime() - epoch).TotalMinutes;
            var baseM = mins / windowMinutes * windowMinutes;
            return epoch.AddMinutes(baseM).ToOffset(t.Offset); // trả về cùng offset để hiển thị
        }

        private static (DateTimeOffset start, DateTimeOffset end) WindowRange(DateTimeOffset t, int windowMinutes)
        {
            var start = NormalizeWindow(t, windowMinutes);
            var end = start.AddMinutes(windowMinutes);
            return (start, end);
        }
        private async Task<int> GetActiveWipCountAsync(Guid branchId)
        {
            // Đếm số RO của chi nhánh có trạng thái chiếm WIP
            return await _unitOfWork.RepairOrders.CountAsync(ro =>
                ro.BranchId == branchId &&
                ActiveOrderStatusIds.Contains(ro.StatusId));
        }
        private async Task EnsureWithinOperatingHoursAsync(Guid branchId, DateTimeOffset windowStartLocal, int windowMinutes)
        {
            if (windowMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(windowMinutes));

            var dow = DowUtil.ToCustomDow(windowStartLocal.DayOfWeek);

            var oh = await _unitOfWork.OperatingHours.SingleOrDefaultAsync(o =>
                o.BranchId == branchId && o.DayOfWeek == dow)
                ?? throw new Exception("Chi nhánh chưa cấu hình giờ làm việc.");

            if (!oh.IsOpen || !oh.OpenTime.HasValue || !oh.CloseTime.HasValue)
                throw new Exception("Chi nhánh nghỉ vào ngày đã chọn.");

            var (openLocal, closeLocal) = SlotWindowUtil.BuildOpenCloseLocal(
                DateOnly.FromDateTime(windowStartLocal.Date), oh.OpenTime.Value, oh.CloseTime.Value);

            SlotWindowUtil.EnsureInsideOpenHours(windowStartLocal, windowMinutes, openLocal, closeLocal);
        }

        public async Task<bool> ApproveRepairRequestAsync(Guid requestId)
        {
            var repairRequest = await _unitOfWork.RepairRequests.GetByIdAsync(requestId);
            if (repairRequest == null)
                return false;

            // Only pending requests can be approved
            if (repairRequest.Status != RepairRequestStatus.Pending)
                return false;

            repairRequest.Status = RepairRequestStatus.Accept;
            repairRequest.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectRepairRequestAsync(Guid requestId)
        {
            var repairRequest = await _unitOfWork.RepairRequests.GetByIdAsync(requestId);
            if (repairRequest == null)
                return false;

            // Only pending requests can be rejected
            if (repairRequest.Status != RepairRequestStatus.Pending)
                return false;

            repairRequest.Status = RepairRequestStatus.Cancelled;
            repairRequest.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<RepairOrderDto> ConvertToRepairOrderAsync(Guid requestId, CreateRoFromRequestDto dto)
        {
            // Get the repair request
            var repairRequest = await _unitOfWork.RepairRequests.GetByIdWithDetailsAsync(requestId);
            if (repairRequest == null)
                throw new Exception("Repair request not found");
                
            // Check if request is approved
            if (repairRequest.Status != RepairRequestStatus.Accept)
                throw new Exception("Only approved repair requests can be converted to repair orders");

            // Get the customer (user) information
            var customer = await _userRepository.GetByIdAsync(repairRequest.UserID);
            if (customer == null)
                throw new Exception("Customer not found");

            // Get the vehicle information
            var vehicle = await _vehicleService.GetVehicleByIdAsync(repairRequest.VehicleID);
            if (vehicle == null)
                throw new Exception("Vehicle not found");

            // Calculate estimated time and amount based on selected services
            decimal totalEstimatedAmount = 0;
            long totalEstimatedTime = 0;

            List<BusinessObject.Service> selectedServices = new List<BusinessObject.Service>();
            if (dto.SelectedServiceIds != null && dto.SelectedServiceIds.Any())
            {
                selectedServices = await _unitOfWork.Services.Query()
                    .Where(s => dto.SelectedServiceIds.Contains(s.ServiceId))
                    .ToListAsync();

                foreach (var service in selectedServices)
                {
                    totalEstimatedAmount += service.Price;
                    totalEstimatedTime += (long)(service.EstimatedDuration * 60); // Convert hours to minutes
                }
            }

            // Create a new repair order based on the repair request
            var repairOrder = new BusinessObject.RepairOrder
            {
                VehicleId = repairRequest.VehicleID,
                RoType = RoType.Scheduled, // Set type to scheduled for repair request conversion
                ReceiveDate = dto.ReceiveDate,
                EstimatedCompletionDate = dto.EstimatedCompletionDate,
                EstimatedAmount = totalEstimatedAmount,
                Note = dto.Note,
                EstimatedRepairTime = totalEstimatedTime,
                UserId = repairRequest.UserID,
                StatusId = 1, // Default to pending status
                BranchId = repairRequest.BranchId,
                RepairRequestId = repairRequest.RepairRequestID, // Link to the repair request
                PaidStatus = PaidStatus.Unpaid, // Default paid status
                CreatedAt = DateTime.UtcNow
            };

            // Create the repair order
            var createdRepairOrder = await _repairOrderService.CreateRepairOrderAsync(repairOrder, dto.SelectedServiceIds);
            
            // Update the repair request status to indicate it has been converted
            repairRequest.Status = RepairRequestStatus.Arrived;
            repairRequest.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            // Return the created repair order DTO
            var fullRepairOrder = await _repairOrderService.GetRepairOrderWithFullDetailsAsync(createdRepairOrder.RepairOrderId);
            var repairOrderDto = _repairOrderService.MapToRepairOrderDto(fullRepairOrder);
            
            return repairOrderDto;
        }
    }
}