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

        public EmergencyRequestService(IEmergencyRequestRepository repository, IMapper mapper, IRepairRequestRepository repairRequestRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _requestRepository = repairRequestRepository;
        }

        public async Task<EmergencyResponeDto> CreateEmergencyAsync(string userId, CreateEmergencyRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Mapping từ DTO sang entity
            var emergencyRequest = _mapper.Map<RequestEmergency>(dto);

            // Kiểm tra xe có thuộc user không
            if (emergencyRequest.Vehicle.UserId != userId)
                throw new UnauthorizedAccessException("Vehicle does not belong to the user.");

            // Gán CustomerId
            emergencyRequest.CustomerId = userId;

            // Thêm thời gian hiện tại và trạng thái mặc định
            emergencyRequest.RequestTime = DateTime.UtcNow;
            emergencyRequest.Status = RequestEmergency.EmergencyStatus.Pending;

            // Lưu vào repository
            var createdRequest = await _repository.CreateAsync(emergencyRequest);

            // Lấy full entity (include Customer và Vehicle nếu cần)
            var fullRequest = await _repository.GetByIdAsync(createdRequest.EmergencyRequestId);

            if (fullRequest == null)
                throw new Exception("Failed to retrieve the created Emergency");

            // Mapping sang DTO trả về, chỉ những trường cần thiết
            var responseDto = new EmergencyResponeDto
            {
                EmergencyRequestId = fullRequest.EmergencyRequestId,
                VehicleName = fullRequest.Vehicle?.Model.ModelName + fullRequest.Vehicle?.Brand.BrandName + fullRequest.Vehicle?.LicensePlate ?? "",
                IssueDescription = fullRequest.IssueDescription,
                //EmergencyType = fullRequest.EmergencyType.ToString(),
                RequestTime = fullRequest.RequestTime,
                Status = fullRequest.Status.ToString(),
                Latitude = fullRequest.Latitude,
                Longitude = fullRequest.Longitude,
                CustomerName = fullRequest.Customer?.UserName ?? "",
                CustomerPhone = fullRequest.Customer?.PhoneNumber ?? "",
               // AssignedTechnicianName = fullRequest.AssignedTechnician?.Name, // Nếu có
                //EmergencyFee = fullRequest.EmergencyFee // Nếu muốn hiển thị ngay
            };

            return responseDto;
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

            // 1️⃣ Cập nhật trạng thái
            emergency.Status = RequestEmergency.EmergencyStatus.Accepted;
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
                    CreatedAt =emergency.RequestTime,
                    UserID = emergency.CustomerId
                };
                await _requestRepository.AddAsync(repairRequest);
            }

            return true;
        }


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
