using AutoMapper;
using BusinessObject.RequestEmergency;
using Dtos.Emergency;
using Microsoft.AspNetCore.Mvc;
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

        public EmergencyRequestService(IEmergencyRequestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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

            return fullRequest!;
        }


        public async Task<IEnumerable<RequestEmergency>> GetByCustomerAsync(string customerId)
        {
            return await _repository.GetByCustomerAsync(customerId);
        }

        public async Task<RequestEmergency?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }
        

        public async Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double latitude, double longitude, int count = 5)
        {
            return await _repository.GetNearestBranchesAsync(latitude, longitude, count);
        }
    }
}
