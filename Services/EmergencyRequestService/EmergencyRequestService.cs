using AutoMapper;
using BusinessObject;
using BusinessObject.Customers;
using BusinessObject.RequestEmergency;
using CloudinaryDotNet.Core;
using Dtos.Customers;
using Dtos.Emergency;
using Microsoft.AspNetCore.Mvc;
using Repositories.Customers;
using Repositories.EmergencyRequestRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EmergencyRequestService
{
    public class EmergencyRequestService: IEmergencyRequestService
    {
        private readonly IEmergencyRequestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IRepairRequestRepository _requestRepository;
        private readonly IPriceEmergencyRepositories _priceRepo;

        public EmergencyRequestService(IEmergencyRequestRepository repository, IMapper mapper, IRepairRequestRepository repairRequestRepository, IPriceEmergencyRepositories priceRepo)
        {
            _repository = repository;
            _mapper = mapper;
            _requestRepository = repairRequestRepository;
            _priceRepo = priceRepo;
        }

        public async Task<RequestEmergency> CreateEmergencyAsync(string userId, CreateEmergencyRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Mapping từ DTO sang entity
            var emergencyRequest = _mapper.Map<RequestEmergency>(dto);

            // Gán UserId từ tham số
            emergencyRequest.CustomerId = userId;

            // Thêm thời gian hiện tại và trạng thái mặc định
            emergencyRequest.RequestTime = DateTime.UtcNow;
            emergencyRequest.Status = RequestEmergency.EmergencyStatus.Pending;

            // Lưu vào repository
            var createdRequest = await _repository.CreateAsync(emergencyRequest);

            var fullRequest = await _repository.GetByIdAsync(createdRequest.EmergencyRequestId);

            return fullRequest;
        }

        public async Task<IEnumerable<EmergencyResponeDto>> GetByCustomerAsync(string customerId)
        {
            var fullRequest = await _repository.GetByCustomerAsync(customerId);
            var responseDtos = fullRequest.Select(fr => new EmergencyResponeDto
            {
                EmergencyRequestId = fr.EmergencyRequestId,
                // VehicleName = fr.Vehicle?.Model.ModelName + fr.Vehicle?.Brand.BrandName + fr.Vehicle?.LicensePlate ?? "",
                VehicleName = fr.Vehicle != null
            ? $"{fr.Vehicle.Model?.ModelName ?? ""} {fr.Vehicle.Brand?.BrandName ?? ""} {fr.Vehicle.LicensePlate ?? ""}".Trim()
            : "",
                IssueDescription = fr.IssueDescription,
                //EmergencyType = fr.EmergencyType.ToString(),
                RequestTime = fr.RequestTime,
                Status = fr.Status.ToString(),
                Latitude = fr.Latitude,
                Longitude = fr.Longitude,
                CustomerName = fr.Customer?.UserName ?? "",
                CustomerPhone = fr.Customer?.PhoneNumber ?? ""
            }).ToList();


            return responseDtos;
        }

        public async Task<RequestEmergency?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }
        

        public async Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double latitude, double longitude, int count = 5)
        {
            return await _repository.GetNearestBranchesAsync(latitude, longitude, count);
        }

        public async Task<IEnumerable<RequestEmergency>> GetAllRequestEmergencyAsync()
        {
            return await _repository.GetAllEmergencyAsync();
        }

        public async Task<bool> ApproveEmergency(Guid emergenciesId)
        {
            var emergency = await _repository.GetByIdAsync(emergenciesId);
            if (emergency == null)
                throw new ArgumentException($"Emergency with ID {emergenciesId} not found.");

            // Kiểm tra các trường bắt buộc
            if (emergency.VehicleId == Guid.Empty)
                throw new InvalidOperationException("Emergency request must have a valid VehicleId.");
            if (emergency.BranchId == Guid.Empty)
                throw new InvalidOperationException("Emergency request must have a valid BranchId.");
            if (string.IsNullOrEmpty(emergency.CustomerId))
                throw new InvalidOperationException("Emergency request must have a valid CustomerId.");

            // Cập nhật trạng thái
            emergency.Status = RequestEmergency.EmergencyStatus.Accepted;

            // 2 Tính tiền tự động khi approve
            var priceConfig = await _priceRepo.GetLatestPriceAsync(); // Lấy giá mới nhất
            if (priceConfig != null && emergency.Branch != null)
            {
                double distance = GetDistance(
                    emergency.Latitude,
                    emergency.Longitude,
                    emergency.Branch.Latitude,
                    emergency.Branch.Longitude
                );

                decimal totalPrice = priceConfig.BasePrice + (decimal)distance * priceConfig.PricePerKm;

                emergency.DistanceToGarageKm = distance;
                emergency.EstimatedCost = totalPrice;
            }
            await _repository.UpdateAsync(emergency);

            // 2️⃣ Tạo RepairRequest tự động nếu chưa có
            var existingRepair = await _requestRepository.GetByEmergencyIdAsync(emergenciesId);
            if (existingRepair == null)
            {
                var repairRequest = new RepairRequest
                {
                    VehicleID = emergency.VehicleId,
                    Description = emergency.IssueDescription,
                    Status = RepairRequestStatus.Accept,
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    RequestDate = emergency.RequestTime,
                    BranchId = emergency.BranchId,
                    CreatedAt = emergency.RequestTime,
                    UserID = emergency.CustomerId,
                    ArrivalWindowStart = DateTimeOffset.UtcNow, // Thêm trường bắt buộc này
                    EstimatedCost = emergency.EstimatedCost ?? 0
                };
                
                try
                {
                    await _requestRepository.AddAsync(repairRequest);
                }
                catch (Exception ex)
                {
                    // Log chi tiết lỗi
                    Console.WriteLine($"Error creating RepairRequest: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                    }
                    throw new Exception($"Failed to create RepairRequest: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
            }

            return true;
        }
        // Hàm tính khoảng cách giữa hai tọa độ
        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg) => deg * (Math.PI / 180);

        public async Task<bool> RejectEmergency(Guid emergenciesId, string? reason)
        {

            var emergency = await _repository.GetByIdAsync(emergenciesId);
            if (emergency == null)
                throw new ArgumentException($"Emergency with ID {emergenciesId} not found.");

            // Không được reject khi đã được xử lý
            if (emergency.Status == RequestEmergency.EmergencyStatus.Accepted ||
                emergency.Status == RequestEmergency.EmergencyStatus.Completed)
                throw new InvalidOperationException("Cannot reject an accepted or completed emergency.");

            emergency.Status = RequestEmergency.EmergencyStatus.Canceled;
            emergency.RejectReason = reason;

            await _repository.UpdateAsync(emergency);
            return true;

        }
    }
}
