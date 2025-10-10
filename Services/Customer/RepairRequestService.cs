using AutoMapper;
using BusinessObject;
using BusinessObject.Customers;
using Dtos.Customers;
using Repositories;
using Repositories.Customers;
using Repositories.PartRepositories;
using Repositories.RepairRequestRepositories;
using Repositories.ServiceRepositories;
using Repositories.Vehicles;
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
        //private readonly IRequestPartRepository _requestPartsRepository;
        private readonly IRequestServiceRepository _requestServicesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPartRepository _partRepository;
        //private readonly ISpecPartRepository _partSpecificationRepository;

        public RepairRequestService(
            //ISpecPartRepository specPartRepository,
            IPartRepository partRepository,
            IServiceRepository serviceRepository,
            IRepairRequestRepository repairRequestRepository,
            IUserRepository userRepository,
            IVehicleRepository vehicleRepository,
            //IRequestPartRepository requestPartRepository,
            IRequestServiceRepository requestServiceRepository,
            ICloudinaryService cloudinaryService,
            IMapper mapper)
        {
            //_partSpecificationRepository = specPartRepository;
            _partRepository = partRepository;
            _serviceRepository = serviceRepository;
            _repairRequestRepository = repairRequestRepository;
            _userRepository = userRepository;
            _vehicleRepository = vehicleRepository;
            //_requestPartsRepository = requestPartRepository;
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

        public async Task<RepairRequestDto> GetByIdAsync(Guid id)
        {
            var request = await _repairRequestRepository.GetByIdAsync(id);
            return _mapper.Map<RepairRequestDto>(request);
        }

        public async Task<RepairRequestDto> CreateRepairRequestAsync(CreateRequestDto dto, string userId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleID);
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
                var service = await _serviceRepository.GetByIdAsync(serviceDto.ServiceId)
                              ?? throw new Exception($"Service {serviceDto.ServiceId} not found");

                // Chỉ giữ giá service (tiền công)
                var requestService = new RequestService
                {
                    ServiceId = service.ServiceId,
                    ServiceFee = service.Price,   // <-- chỉ tính tiền công
                    RequestParts = new List<RequestPart>()
                };

                totalServiceFee += service.Price;

                // Nếu có parts thì cộng tiền parts riêng
                if (serviceDto.Parts != null)
                {
                    foreach (var partDto in serviceDto.Parts)
                    {
                        var part = await _partRepository.GetByIdAsync(partDto.PartId)
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

            // Tổng tiền cuối cùng
           
            repairRequest.EstimatedCost = totalServiceFee + totalPartsFee;

            // Xử lý ảnh
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

        //public async Task<IEnumerable<RequestPartDto>> GetPartsAsync(Guid repairRequestId)
        //{
        //    var parts = await _requestPartsRepository.GetByRepairRequestIdAsync(repairRequestId);
        //    return _mapper.Map<IEnumerable<RequestPartDto>>(parts);
        //}

        //public async Task<RequestPartDto> AddPartAsync(RequestPartDto dto)
        //{
        //    var part = _mapper.Map<RequestPart>(dto);
        //    part.RequestPartId = Guid.NewGuid();
        //    await _requestPartsRepository.AddAsync(part);
        //    return _mapper.Map<RequestPartDto>(part);
        //}

        //public async Task<bool> DeletePartAsync(Guid partId)
        //{
        //    return await _requestPartsRepository.DeleteAsync(partId);
        //}

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
