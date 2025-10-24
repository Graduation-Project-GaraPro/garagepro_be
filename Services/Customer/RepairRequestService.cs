using AutoMapper;
using BusinessObject.Customers;
using Dtos.Customers;
using Repositories;
using Repositories.Customers;
using Repositories.RepairRequestRepositories;
using Repositories.VehicleRepositories;
using Services.Cloudinaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Customer
{
    public class RepairRequestService : IRepairRequestService
    {
        private readonly IRepairRequestRepository _repairRequestRepository;
        private readonly IRequestPartRepository _requestPartsRepository;
        private readonly IRequestServiceRepository _requestServicesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;

        public RepairRequestService(
            IRepairRequestRepository repairRequestRepository,
            IUserRepository userRepository,
            IVehicleRepository vehicleRepository,
            IRequestPartRepository requestPartRepository,
            IRequestServiceRepository requestServiceRepository,
            ICloudinaryService cloudinaryService,
            IMapper mapper)
        {
            _repairRequestRepository = repairRequestRepository;
            _userRepository = userRepository;
            _vehicleRepository = vehicleRepository;
            _requestPartsRepository = requestPartRepository;
            _requestServicesRepository = requestServiceRepository;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RepairRequestDto>> GetAllAsync()
        {
            var requests = await _repairRequestRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RepairRequestDto>>(requests);
        }

        public async Task<IEnumerable<RepairRequestDto>> GetByUserIdAsync(string userId)
        {
            var requests = await _repairRequestRepository.GetByUserIdAsync(userId);
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
                    IsCompleted = request.IsCompleted,
                    ImageUrls = request.RepairImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(),
                    Services = _mapper.Map<List<RequestServiceDto>>(request.RequestServices?.ToList() ?? new List<RequestService>()),
                    Parts = _mapper.Map<List<RequestPartDto>>(request.RequestParts?.ToList() ?? new List<RequestPart>()),
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
                IsCompleted = request.IsCompleted,
                ImageUrls = request.RepairImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(),
                Services = _mapper.Map<List<RequestServiceDto>>(request.RequestServices?.ToList() ?? new List<RequestService>()),
                Parts = _mapper.Map<List<RequestPartDto>>(request.RequestParts?.ToList() ?? new List<RequestPart>()),
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };

            return dto;
        }

        public async Task<RepairRequestDto> GetByIdAsync(Guid id)
        {
            var request = await _repairRequestRepository.GetByIdAsync(id);
            return _mapper.Map<RepairRequestDto>(request);
        }
      
        public async Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId)
        {
            var customer = await _userRepository.GetByIdAsync(userId);
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleID);

            if (vehicle.UserId != userId)
                throw new Exception("This vehicle does not belong to the current user");

            var repairRequest = _mapper.Map<RepairRequest>(dto);
            repairRequest.RepairRequestID = Guid.NewGuid();
            repairRequest.UserID = userId;
            repairRequest.RequestDate = DateTime.UtcNow;
            repairRequest.IsCompleted = false;

            // Map parts & services if any
            if (dto.Parts != null)
                repairRequest.RequestParts = _mapper.Map<List<RequestPart>>(dto.Parts);

            if (dto.Services != null)
                repairRequest.RequestServices = _mapper.Map<List<RequestService>>(dto.Services);

            // Map images if any
            if (dto.Images != null) // giả sử frontend gửi IFormFile[]
            {
                foreach (var file in dto.Images)
                {
                    var imageUrl = await _cloudinaryService.UploadImageAsync(file);
                    repairRequest.RepairImages.Add(new RepairImage
                    {
                        ImageId = Guid.NewGuid(),
                        ImageUrl = imageUrl,
                        RepairRequest = repairRequest
                    });
                }
            }

            await _repairRequestRepository.AddAsync(repairRequest);
            return _mapper.Map<RepairRequestDto>(repairRequest);
        }

        public async Task<RepairRequestDto> UpdateRepairRequestAsync(Guid id, UpdateRepairRequestDto dto)
        {
            var request = await _repairRequestRepository.GetByIdAsync(id);
            if (request == null) return null;

            _mapper.Map(dto, request);
            await _repairRequestRepository.UpdateAsync(request);

            return _mapper.Map<RepairRequestDto>(request);
        }

        public async Task<bool> DeleteRepairRequestAsync(Guid id)
        {
            return await _repairRequestRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<RequestImagesDto>> GetImagesAsync(Guid repairRequestId)
        {
            var images = await _repairRequestRepository.GetImagesAsync(repairRequestId);
            return _mapper.Map<IEnumerable<RequestImagesDto>>(images);
        }

        public async Task<RequestImagesDto> AddImageAsync(RequestImagesDto dto)
        {
            var image = _mapper.Map<RepairImage>(dto);
            image.ImageId = Guid.NewGuid();
            await _repairRequestRepository.AddImageAsync(image);
            return _mapper.Map<RequestImagesDto>(image);
        }

        public async Task<bool> DeleteImageAsync(Guid imageId)
        {
            return await _repairRequestRepository.DeleteImageAsync(imageId);
        }

        public async Task<IEnumerable<RequestPartDto>> GetPartsAsync(Guid repairRequestId)
        {
            var parts = await _requestPartsRepository.GetByRepairRequestIdAsync(repairRequestId);
            return _mapper.Map<IEnumerable<RequestPartDto>>(parts);
        }

        public async Task<RequestPartDto> AddPartAsync(RequestPartDto dto)
        {
            var part = _mapper.Map<RequestPart>(dto);
            part.RequestPartId = Guid.NewGuid();
            await _requestPartsRepository.AddAsync(part);
            return _mapper.Map<RequestPartDto>(part);
        }

        public async Task<bool> DeletePartAsync(Guid partId)
        {
            return await _requestPartsRepository.DeleteAsync(partId);
        }

        public async Task<IEnumerable<RequestServiceDto>> GetServicesAsync(Guid repairRequestId)
        {
            var services = await _requestServicesRepository.GetByRepairRequestIdAsync(repairRequestId);
            return _mapper.Map<IEnumerable<RequestServiceDto>>(services);
        }

        public async Task<RequestServiceDto> AddServiceAsync(RequestServiceDto dto)
        {
            var service = _mapper.Map<RequestService>(dto);
            service.RequestServiceId = Guid.NewGuid();
            await _requestServicesRepository.AddAsync(service);
            return _mapper.Map<RequestServiceDto>(service);
        }

        public async Task<bool> DeleteServiceAsync(Guid requestServiceId)
        {
            return await _requestServicesRepository.DeleteAsync(requestServiceId);
        }

       
    }
}