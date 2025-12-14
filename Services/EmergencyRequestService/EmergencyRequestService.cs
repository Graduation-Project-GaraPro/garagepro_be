using AutoMapper;
using BusinessObject;
using BusinessObject.Customers;
using BusinessObject.RequestEmergency;
using CloudinaryDotNet.Core;
using Dtos.Customers;
using Dtos.Emergency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Repositories;
using Repositories.Customers;
using Repositories.EmergencyRequestRepositories;
using Repositories.VehicleRepositories;
using Services.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Services.EmergencyRequestService
{
    public class EmergencyRequestService: IEmergencyRequestService
    {
        private readonly IEmergencyRequestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IRepairRequestRepository _requestRepository;
        private readonly IPriceEmergencyRepositories _priceRepo;
        private readonly IHubContext<EmergencyRequestHub> _hubContext;
        private readonly Services.GeocodingServices.IGeocodingService _geocodingService;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;



        public EmergencyRequestService(
            IEmergencyRequestRepository repository, 
            IMapper mapper, 
            IRepairRequestRepository repairRequestRepository, 
            IPriceEmergencyRepositories priceRepo,
            IHubContext<EmergencyRequestHub> hubContext,
            Services.GeocodingServices.IGeocodingService geocodingService,
            IVehicleRepository vehicleRepository,
            IMemoryCache cache,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _repository = repository;
            _mapper = mapper;
            _requestRepository = repairRequestRepository;
            _priceRepo = priceRepo;
            _hubContext = hubContext;
            _geocodingService = geocodingService;
            _vehicleRepository = vehicleRepository;
            _cache = cache;
            _userRepository = userRepository;
            _config = configuration;
        }

        public async Task<EmergencyResponeDto> CreateEmergencyAsync(string userId, CreateEmergencyRequestDto dto, string? idempotencyKey = null)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Latitude < -90 || dto.Latitude > 90 || dto.Longitude < -180 || dto.Longitude > 180)
                throw new ArgumentOutOfRangeException("Coordinates are out of range.");

            if (dto.VehicleId == Guid.Empty)
                throw new ArgumentException("VehicleId is required.");
            if (string.IsNullOrWhiteSpace(dto.IssueDescription))
                throw new ArgumentException("Issue description is required.");
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var cacheKey = $"emg:idemp:{userId}:{idempotencyKey}";
                if (_cache.TryGetValue<Guid>(cacheKey, out var existingId))
                {
                    var existing = await _repository.GetByIdAsync(existingId);
                    if (existing != null)
                        return MapToDto(existing);
                }
            }

            var lastKey = $"emg:last:{userId}";
            if (_cache.TryGetValue<DateTime>(lastKey, out var last) && (DateTime.UtcNow - last) < TimeSpan.FromSeconds(60))
            {
                throw new InvalidOperationException("Too many requests. Please wait before creating another emergency.");
            }

            var hasActive = await _repository.AnyActiveAsync(userId);
            if (hasActive)
                throw new InvalidOperationException("Active emergency already exists for this user.");


            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle == null)
                throw new ArgumentException("Vehicle not found.");
            if (!string.Equals(vehicle.UserId, userId, StringComparison.Ordinal))
                throw new InvalidOperationException("Vehicle does not belong to user.");

            // Map DTO to entity
            var emergencyRequest = _mapper.Map<RequestEmergency>(dto);

            // Assign UserId from parameter
            emergencyRequest.CustomerId = userId;

            // Add current time and default status
            emergencyRequest.RequestTime = DateTime.UtcNow;
            emergencyRequest.Status = RequestEmergency.EmergencyStatus.Pending;
            emergencyRequest.ResponseDeadline = emergencyRequest.RequestTime.AddMinutes(5);
            emergencyRequest.Address = await _geocodingService.ReverseGeocodeAsync(emergencyRequest.Latitude, emergencyRequest.Longitude);

            // Save to repository
            var createdRequest = await _repository.CreateAsync(emergencyRequest);

            var fullRequest = await _repository.GetByIdAsync(createdRequest.EmergencyRequestId);
            if (fullRequest?.Branch != null && HasValidCoords(fullRequest.Branch.Latitude, fullRequest.Branch.Longitude))
            {
                fullRequest.DistanceToGarageKm = GetDistance(
                    fullRequest.Latitude,
                    fullRequest.Longitude,
                    fullRequest.Branch.Latitude,
                    fullRequest.Branch.Longitude
                );
                await _repository.UpdateAsync(fullRequest);
            }

            _cache.Set(lastKey, DateTime.UtcNow, TimeSpan.FromMinutes(5));
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var cacheKey = $"emg:idemp:{userId}:{idempotencyKey}";
                _cache.Set(cacheKey, fullRequest.EmergencyRequestId, TimeSpan.FromHours(1));
            }

            // Send real-time notification via SignalR when creating a new emergency
            try
            {
                var notificationData = new
                {
                    EmergencyRequestId = fullRequest.EmergencyRequestId,
                    Status = "Pending",
                    CustomerId = fullRequest.CustomerId,
                    BranchId = fullRequest.BranchId,
                    VehicleId = fullRequest.VehicleId,
                    IssueDescription = fullRequest.IssueDescription,
                    Latitude = fullRequest.Latitude,
                    Longitude = fullRequest.Longitude,
                    DistanceToGarageKm = fullRequest.DistanceToGarageKm,
                    EstimatedArrivalMinutes = fullRequest.DistanceToGarageKm.HasValue ? (int?)Math.Ceiling(fullRequest.DistanceToGarageKm.Value / 30.0 * 60.0) : null,
                    RequestTime = fullRequest.RequestTime,
                    ResponseDeadline = fullRequest.ResponseDeadline,
                    Address = fullRequest.Address,
                    CustomerName = fullRequest.Customer?.UserName ?? "",
                    CustomerPhone = fullRequest.Customer?.PhoneNumber ?? "",
                    BranchName = fullRequest.Branch?.BranchName ?? "",
                    Message = "New emergency request",
                    Timestamp = DateTime.UtcNow
                };

                // Send to all clients (so admin/branch can see new requests)
                await _hubContext.Clients.All.SendAsync("EmergencyRequestCreated", notificationData);


                // Send to specific customer (so the customer knows the request was created)
                await _hubContext.Clients.Group($"customer-{fullRequest.CustomerId}")
                    .SendAsync("EmergencyRequestCreated", notificationData);

                // Send to specific branch (so the branch receives the new request)
                await _hubContext.Clients.Group($"branch-{fullRequest.BranchId}")
                    .SendAsync("EmergencyRequestCreated", notificationData);
            }
            catch (Exception ex)
            {
                // Log errors but do not interrupt the emergency creation process
                Console.WriteLine($"Error sending real-time notification: {ex.Message}");
            }

            return MapToDto(fullRequest);
        }

        private EmergencyResponeDto MapToDto(RequestEmergency fr)
        {
            var addr = string.IsNullOrWhiteSpace(fr.Address) ? $"{fr.Latitude},{fr.Longitude}" : fr.Address;
            var mapUrl = (fr.Latitude != 0 || fr.Longitude != 0) ? $"https://www.google.com/maps?q={fr.Latitude},{fr.Longitude}" : null;
            int? etaMinutes = null;
            if (fr.DistanceToGarageKm.HasValue)
            {
                const double avgSpeedKmh = 30.0; // assumed average urban speed
                etaMinutes = (int)Math.Ceiling(fr.DistanceToGarageKm.Value / avgSpeedKmh * 60.0);
            }

            return new EmergencyResponeDto
            {
                EmergencyRequestId = fr.EmergencyRequestId,
                VehicleName = fr.Vehicle != null
                    ? $"{fr.Vehicle.Model?.ModelName ?? ""} {fr.Vehicle.Brand?.BrandName ?? ""} {fr.Vehicle.LicensePlate ?? ""}".Trim()
                    : "",
                IssueDescription = fr.IssueDescription,
                EmergencyType = fr.Type.ToString(),
                RequestTime = fr.RequestTime,
                Status = fr.Status.ToString(),
                Latitude = fr.Latitude,
                Longitude = fr.Longitude,
                Address = addr,
                MapUrl = mapUrl,
                ResponseDeadline = fr.ResponseDeadline,
                RespondedAt = fr.RespondedAt,
                AutoCanceledAt = fr.AutoCanceledAt,
                CustomerName = fr.Customer?.UserName ?? "",
                CustomerPhone = fr.Customer?.PhoneNumber ?? "",
                DistanceToGarageKm = fr.DistanceToGarageKm,
                EstimatedArrivalMinutes = etaMinutes,
                EmergencyFee = fr.EstimatedCost
            };
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
                Address = fr.Address,
                DistanceToGarageKm = fr.DistanceToGarageKm,
                EstimatedArrivalMinutes = fr.DistanceToGarageKm.HasValue ? (int?)Math.Ceiling(fr.DistanceToGarageKm.Value / 30.0 * 60.0) : null,
                ResponseDeadline = fr.ResponseDeadline,
                RespondedAt = fr.RespondedAt,
                AutoCanceledAt = fr.AutoCanceledAt,
                CustomerName = fr.Customer?.UserName ?? "",
                CustomerPhone = fr.Customer?.PhoneNumber ?? "",
                AssignedTechnicianName = fr.Technician?.LastName ?? "",
                AssginedTecinicianPhone =fr.Technician?.PhoneNumber??""
            }).ToList();


            return responseDtos;
        }

        public async Task<RequestEmergency?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<EmergencyResponeDto?> GetDtoByIdAsync(Guid id)
        {
            var fr = await _repository.GetByIdAsync(id);
            if (fr == null) return null;
            return MapToDto(fr);
        }

        public async Task<string> ReverseGeocodeAddressAsync(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException("Coordinates are out of range.");

            var address = await _geocodingService.ReverseGeocodeAsync(latitude, longitude);
            return string.IsNullOrWhiteSpace(address) ? $"{latitude},{longitude}" : address;
        }
        

        public async Task<List<BranchNearbyResponseDto>> GetNearestBranchesAsync(double latitude, double longitude, int count = 5)
        {
            return await _repository.GetNearestBranchesAsync(latitude, longitude, count);
        }

        public async Task<IEnumerable<RequestEmergency>> GetAllRequestEmergencyAsync()
        {
            return await _repository.GetAllEmergencyAsync();
        }

        public async Task<bool> ApproveEmergency(Guid emergenciesId, string managerUserId)
        {
            var emergency = await _repository.GetByIdAsync(emergenciesId);
            if (emergency == null)
                throw new ArgumentException($"Emergency with ID {emergenciesId} not found.");

            // Validate required fields
            if (emergency.VehicleId == Guid.Empty)
                throw new InvalidOperationException("Emergency request must have a valid VehicleId.");
            if (emergency.BranchId == Guid.Empty)
                throw new InvalidOperationException("Emergency request must have a valid BranchId.");
            if (string.IsNullOrEmpty(emergency.CustomerId))
                throw new InvalidOperationException("Emergency request must have a valid CustomerId.");

            if (emergency.Status != RequestEmergency.EmergencyStatus.Pending)
                throw new InvalidOperationException("Only pending emergencies can be approved.");

            if (emergency.Branch == null || !emergency.Branch.IsActive)
                throw new InvalidOperationException("Branch is inactive or not available.");
            var manager = await _userRepository.GetByIdAsync(managerUserId);
            if (manager == null)
                throw new InvalidOperationException("Manager user not found.");
            if (!manager.BranchId.HasValue || manager.BranchId.Value != emergency.BranchId)
                throw new InvalidOperationException("Manager not authorized to approve this branch.");


            // Update status
            emergency.Status = RequestEmergency.EmergencyStatus.Accepted;
            emergency.RespondedAt = DateTime.UtcNow;

            // Auto-calculate fee when approved
            var priceConfig = await _priceRepo.GetLatestPriceAsync(); // Get latest price
            if (priceConfig != null && emergency.Branch != null && HasValidCoords(emergency.Branch.Latitude, emergency.Branch.Longitude))
            {
                double distance = GetDistance(
                    emergency.Latitude,
                    emergency.Longitude,
                    emergency.Branch.Latitude,
                    emergency.Branch.Longitude
                );

                decimal totalPrice = priceConfig.BasePrice;
                if (emergency.Type == RequestEmergency.EmergencyType.TowToGarage)
                {
                    totalPrice += (decimal)distance * priceConfig.PricePerKm;
                }

                emergency.DistanceToGarageKm = distance;
                emergency.EstimatedCost = totalPrice;
            }
            await _repository.UpdateAsync(emergency);

            // Auto-create RepairRequest if missing
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
                    ArrivalWindowStart = DateTimeOffset.UtcNow, // Add this required field
                    EstimatedCost = emergency.EstimatedCost ?? 0
                };
                
                try
                {
                    await _requestRepository.AddAsync(repairRequest);
                }
                catch (Exception ex)
                {
                    // Log detailed error
                    Console.WriteLine($"Error creating RepairRequest: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                    }
                    throw new Exception($"Failed to create RepairRequest: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
            }

            // Send real-time notification via SignalR
            try
            {
                var notificationData = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "Accepted",
                    CustomerId = emergency.CustomerId,
                    BranchId = emergency.BranchId,
                    EstimatedCost = emergency.EstimatedCost,
                    DistanceToGarageKm = emergency.DistanceToGarageKm,
                    RespondedAt = emergency.RespondedAt,
                    Message = "Emergency request has been approved",
                    Timestamp = DateTime.UtcNow
                };

                // Send to all clients
                await _hubContext.Clients.All.SendAsync("EmergencyRequestApproved", notificationData);
                Console.WriteLine($"RT sent: EmergencyRequestApproved → All, id={emergency.EmergencyRequestId}");

                // Send to specific customer
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}")
                    .SendAsync("EmergencyRequestApproved", notificationData);
                Console.WriteLine($"RT sent: EmergencyRequestApproved → customer-{emergency.CustomerId}, id={emergency.EmergencyRequestId}");

                // Send to specific branch
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}")
                    .SendAsync("EmergencyRequestApproved", notificationData);
                Console.WriteLine($"RT sent: EmergencyRequestApproved → branch-{emergency.BranchId}, id={emergency.EmergencyRequestId}");
            }
            catch (Exception ex)
            {
                // Log errors but do not interrupt the approval process
                Console.WriteLine($"Error sending real-time notification: {ex.Message}");
            }

            return true;
        }
        // Calculate distance between two coordinates
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

        private static bool HasValidCoords(double lat, double lon)
        {
            if (double.IsNaN(lat) || double.IsNaN(lon)) return false;
            if (lat < -90 || lat > 90 || lon < -180 || lon > 180) return false;
            if (lat == 0 && lon == 0) return false;
            return true;
        }

        public async Task<bool> RejectEmergency(Guid emergenciesId, string? reason)
        {

            var emergency = await _repository.GetByIdAsync(emergenciesId);
            if (emergency == null)
                throw new ArgumentException($"Emergency with ID {emergenciesId} not found.");

            // Cannot reject when already processed
            if (emergency.Status != RequestEmergency.EmergencyStatus.Pending)
                throw new InvalidOperationException("Cannot reject an emergency.");

            emergency.Status = RequestEmergency.EmergencyStatus.Canceled;
            emergency.RejectReason = reason;
            emergency.RespondedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(emergency);

            //  Send real-time notification via SignalR
            try
            {
                var notificationData = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "Canceled",
                    CustomerId = emergency.CustomerId,
                    BranchId = emergency.BranchId,
                    RejectReason = reason,
                    RespondedAt = emergency.RespondedAt,
                    Message = "Emergency request has been rejected",
                    Timestamp = DateTime.UtcNow
                };

                // Send to all clients
                await _hubContext.Clients.All.SendAsync("EmergencyRequestRejected", notificationData);

                // Send to specific customer
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}")
                    .SendAsync("EmergencyRequestRejected", notificationData);

                // Send to specific branch
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}")
                    .SendAsync("EmergencyRequestRejected", notificationData);
            }
            catch (Exception ex)
            {
                // Log errors but do not interrupt the rejection process
                Console.WriteLine($"Error sending real-time notification: {ex.Message}");
            }

            return true;

        }

        public async Task<bool> SetInProgressAsync(Guid emergenciesId)
        {
            var emergency = await _repository.GetByIdAsync(emergenciesId);
            if (emergency == null)
                throw new ArgumentException($"Emergency with ID {emergenciesId} not found.");

            if (emergency.Status != RequestEmergency.EmergencyStatus.Accepted)
                throw new InvalidOperationException("Only accepted emergencies can be set to InProgress.");

            emergency.Status = RequestEmergency.EmergencyStatus.InProgress;
            await _repository.UpdateAsync(emergency);

            try
            {
                var payload = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "InProgress",
                    CustomerId = emergency.CustomerId,
                    TechnicianId = emergency.TechnicianId,
                    BranchId = emergency.BranchId,
                    Message = "Emergency is in progress",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestInProgress", payload);
                Console.WriteLine($"RT sent: EmergencyRequestInProgress → All, id={emergency.EmergencyRequestId}");
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("EmergencyRequestInProgress", payload);
                Console.WriteLine($"RT sent: EmergencyRequestInProgress → customer-{emergency.CustomerId}, id={emergency.EmergencyRequestId}");
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}").SendAsync("EmergencyRequestInProgress", payload);
                Console.WriteLine($"RT sent: EmergencyRequestInProgress → branch-{emergency.BranchId}, id={emergency.EmergencyRequestId}");
            }
            catch { }

            return true;
        }

        public async Task<bool> CancelEmergencyAsync(string userId, Guid emergenciesId)
        {
            var emergency = await _repository.GetByIdAsync(emergenciesId);
            if (emergency == null)
                throw new ArgumentException($"Emergency with ID {emergenciesId} not found.");

            if (!string.Equals(emergency.CustomerId, userId, StringComparison.Ordinal))
                throw new InvalidOperationException("Emergency does not belong to user.");

            if (emergency.Status != RequestEmergency.EmergencyStatus.Pending)
                throw new InvalidOperationException("Only pending emergencies can be canceled.");

            emergency.Status = RequestEmergency.EmergencyStatus.Canceled;
            

            await _repository.UpdateAsync(emergency);

            try
            {
                var payload = new
                {
                    EmergencyRequestId = emergency.EmergencyRequestId,
                    Status = "Canceled",
                    CustomerId = emergency.CustomerId,
                    BranchId = emergency.BranchId,                 
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestCanceled", payload);
                Console.WriteLine($"RT sent: EmergencyRequestCanceled → All, id={emergency.EmergencyRequestId}");
                await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("EmergencyRequestCanceled", payload);
                Console.WriteLine($"RT sent: EmergencyRequestCanceled → customer-{emergency.CustomerId}, id={emergency.EmergencyRequestId}");
                await _hubContext.Clients.Group($"branch-{emergency.BranchId}").SendAsync("EmergencyRequestCanceled", payload);
                Console.WriteLine($"RT sent: EmergencyRequestCanceled → branch-{emergency.BranchId}, id={emergency.EmergencyRequestId}");
            }
            catch { }

            return true;
        }

        // Ensure you already have this DTO
        /*
        namespace Dtos.Emergency
        {
            public class RouteDto
            {
                public double DistanceKm { get; set; }
                public int DurationMinutes { get; set; }
                public System.Text.Json.JsonElement Geometry { get; set; }
            }
        }
        */

        public async Task<RouteDto> GetRouteAsync(double fromLat, double fromLon, double toLat, double toLon)
        {
            // --- Read environment variables ---
            var token = _config["Mapbox:Token"];
            
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Missing MAPBOX_TOKEN environment variable.");

            //var profile = Environment.GetEnvironmentVariable("MAPBOX_PROFILE");
            var profile = _config["Mapbox:Profile"] ?? "driving-traffic";
            if (string.IsNullOrWhiteSpace(profile))
                profile = "driving-traffic";

            // --- Build coordinates string ---
            var coords = string.Join(";", new[]
            {
        $"{fromLon.ToString(CultureInfo.InvariantCulture)},{fromLat.ToString(CultureInfo.InvariantCulture)}",
        $"{toLon.ToString(CultureInfo.InvariantCulture)},{toLat.ToString(CultureInfo.InvariantCulture)}"
    });

            // --- Build URL ---
            var url =
                $"https://api.mapbox.com/directions/v5/mapbox/{profile}/{coords}" +
                "?alternatives=false&geometries=geojson&overview=full&language=en&steps=true&access_token="+token;

            // --- HttpClient (consider using IHttpClientFactory in production) ---
            using var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("garagepro-mapbox-client");
            http.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            // --- Execute request ---
            var res = await http.GetAsync(url);

            // Read content ONCE
            var raw = await res.Content.ReadAsStringAsync();

            // --- Handle HTTP error ---
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Mapbox Directions error. HTTP {(int)res.StatusCode}: {raw}"
                );
            }

            // --- Parse successful JSON ---
            using var doc = JsonDocument.Parse(raw);

            var root = doc.RootElement;

            if (!root.TryGetProperty("routes", out var routes) ||
                routes.ValueKind != JsonValueKind.Array ||
                routes.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Mapbox returned empty routes.");
            }

            var route = routes[0];
            var distance = route.GetProperty("distance").GetDouble();
            var duration = route.GetProperty("duration").GetDouble();
            var geometryEl = route.GetProperty("geometry");

            var steps = new List<Dtos.Emergency.RouteStepDto>();
            if (route.TryGetProperty("legs", out var legs) &&
                legs.ValueKind == JsonValueKind.Array &&
                legs.GetArrayLength() > 0)
            {
                var leg = legs[0];
                if (leg.TryGetProperty("steps", out var stepsEl) && stepsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var st in stepsEl.EnumerateArray())
                    {
                        double sd = st.TryGetProperty("distance", out var sde) && sde.ValueKind == JsonValueKind.Number ? sde.GetDouble() : 0.0;
                        double su = st.TryGetProperty("duration", out var sue) && sue.ValueKind == JsonValueKind.Number ? sue.GetDouble() : 0.0;
                        string name = st.TryGetProperty("name", out var sne) && sne.ValueKind == JsonValueKind.String ? sne.GetString() : null;
                        string instruction = null;
                        if (st.TryGetProperty("maneuver", out var man) && man.ValueKind == JsonValueKind.Object && man.TryGetProperty("instruction", out var ins) && ins.ValueKind == JsonValueKind.String)
                        {
                            instruction = ins.GetString();
                        }
                        steps.Add(new Dtos.Emergency.RouteStepDto
                        {
                            Instruction = instruction ?? name ?? string.Empty,
                            DistanceMeters = sd,
                            DurationSeconds = su,
                            Name = name ?? string.Empty
                        });
                    }
                }
            }

            // Clone geometry to avoid disposed JsonDocument issues
            var geometry = JsonDocument.Parse(geometryEl.GetRawText()).RootElement;

            // --- Build DTO ---
            return new RouteDto
            {
                DistanceKm = Math.Round(distance / 1000.0, 3),
                DurationMinutes = (int)Math.Ceiling(duration / 60.0),
                Geometry = geometry,
                Steps = steps
            };
        }






        public async Task<RouteDto> GetRouteByEmergencyIdAsync(Guid emergencyId)
        {
            var e = await _repository.GetByIdAsync(emergencyId);
            if (e == null || e.Branch == null) throw new ArgumentException("Emergency or branch not found");
            return await GetRouteAsync(e.Branch.Latitude, e.Branch.Longitude, e.Latitude, e.Longitude);
        }

        public async Task<bool> UpdateTechnicianLocationAsync(string technicianUserId, TechnicianLocationDto location)
        {
            var emergency = await _repository.GetByIdAsync(location.EmergencyRequestId);
            if (emergency == null) throw new ArgumentException("Emergency not found");
            if (emergency.Status != RequestEmergency.EmergencyStatus.Accepted && emergency.Status != RequestEmergency.EmergencyStatus.InProgress)
                throw new InvalidOperationException("Emergency must be accepted or in-progress to update location.");
            if (string.IsNullOrEmpty(emergency.TechnicianId) || !string.Equals(emergency.TechnicianId, technicianUserId, StringComparison.Ordinal))
                throw new InvalidOperationException("Only the assigned technician can update location for this emergency.");

            RouteDto? route = null;
            if (location.RecomputeRoute)
            {
                try
                {
                    route = await GetRouteAsync(location.Latitude, location.Longitude, emergency.Latitude, emergency.Longitude);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Route compute failed: {ex.Message}");
                }
            }

            var payload = new
            {
                EmergencyRequestId = emergency.EmergencyRequestId,
                TechnicianUserId = technicianUserId,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                SpeedKmh = location.SpeedKmh,
                Bearing = location.Bearing,
                EtaMinutes = route?.DurationMinutes,
                Route = route?.Geometry,
                Steps = route?.Steps,
                DistanceKm = route?.DistanceKm,
                Timestamp = DateTime.UtcNow,
                TechnicianName = emergency.Technician.LastName,
                PhoneNumberTecnician = emergency.Technician.PhoneNumber
            };

            await _hubContext.Clients.Group($"customer-{emergency.CustomerId}").SendAsync("TechnicianLocationUpdated", payload);
            Console.WriteLine($"RT sent: TechnicianLocationUpdated → customer-{emergency.CustomerId}, id={emergency.EmergencyRequestId}");
            await _hubContext.Clients.Group($"branch-{emergency.BranchId}").SendAsync("TechnicianLocationUpdated", payload);
            Console.WriteLine($"RT sent: TechnicianLocationUpdated → branch-{emergency.BranchId}, id={emergency.EmergencyRequestId}");

            return true;
        }

        public async Task<bool> AssignTechnicianToEmergencyAsync(Guid emergencyId, Guid technicianUserId)
        {
            return await _repository.AssignTechnicianAsync(emergencyId, technicianUserId);
        }
    }
}
