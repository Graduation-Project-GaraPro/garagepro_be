using BusinessObject.Authentication;
using BusinessObject.Enums;
using Dtos.InspectionAndRepair;
using Dtos.RepairOrder;
using Dtos.RoBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore; // Add this for ToListAsync
using Repositories.ServiceRepositories; // Add this for service repository
using Services;
using Services.Hubs;
using Services.VehicleServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RepairOrderController : ControllerBase
    {
        private readonly IRepairOrderService _repairOrderService;
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;
        private readonly IOrderStatusService _orderStatusService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IHubContext<RepairOrderHub> _hubContext;

        public RepairOrderController(
            IRepairOrderService repairOrderService, 
            IUserService userService,
            ICustomerService customerService,
            IVehicleService vehicleService,
            IOrderStatusService orderStatusService,
            IServiceRepository serviceRepository,
            IHubContext<RepairOrderHub> hubContext)
        {
            _repairOrderService = repairOrderService;
            _userService = userService;
            _customerService = customerService;
            _vehicleService = vehicleService;
            _orderStatusService = orderStatusService;
            _serviceRepository = serviceRepository;
            _hubContext = hubContext;
        }

        // GET: api/RepairOrder
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetRepairOrders()
        {
            var repairOrders = await _repairOrderService.GetAllRepairOrdersAsync();
            // Map to enhanced DTOs
            var repairOrderDtos = new List<Dtos.RepairOrder.RepairOrderDto>();
            foreach (var ro in repairOrders)
            {
                var fullRepairOrder = await _repairOrderService.GetRepairOrderWithFullDetailsAsync(ro.RepairOrderId);
                var repairOrderDto = _repairOrderService.MapToRepairOrderDto(fullRepairOrder);
                repairOrderDtos.Add(repairOrderDto);
            }
            return Ok(repairOrderDtos);
        }

        // GET: api/RepairOrder/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepairOrder(Guid id)
        {
            var repairOrder = await _repairOrderService.GetRepairOrderCardAsync(id);
            if (repairOrder == null)
            {
                return NotFound();
            }

            // Use the enhanced DTO with all the information
            var fullRepairOrder = await _repairOrderService.GetRepairOrderWithFullDetailsAsync(id);
            var repairOrderDto = _repairOrderService.MapToRepairOrderDto(fullRepairOrder);

            return Ok(repairOrderDto);
        }

        // POST: api/RepairOrder
        [HttpPost]
        public async Task<IActionResult> CreateRepairOrder([FromBody] CreateRoDto createRoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    message = "Invalid model state", 
                    errors = ModelState.Select(x => new { 
                        field = x.Key, 
                        errors = x.Value.Errors.Select(e => e.ErrorMessage) 
                    }) 
                });
            }

            try
            {
                // Get the authenticated user to extract branch ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Get the user from the user service to extract their branch ID
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found", userId = userId });
                }

                // Ensure the user has a branch assigned
                if (!user.BranchId.HasValue)
                {
                    return BadRequest(new { 
                        message = "User is not assigned to a branch", 
                        userId = userId,
                        userBranchId = user.BranchId 
                    });
                }

                // Validate that the customer exists
                var customer = await _userService.GetByIdAsync(createRoDto.CustomerId);
                if (customer == null)
                {
                    return BadRequest(new { 
                        message = "Customer not found", 
                        customerId = createRoDto.CustomerId 
                    });
                }

                // Validate that the vehicle exists
                var vehicle = await _vehicleService.GetVehicleByIdAsync(createRoDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(new { 
                        message = "Vehicle not found", 
                        vehicleId = createRoDto.VehicleId 
                    });
                }

                // Get the default "Pending" status
                var statusColumns = await _orderStatusService.GetOrderStatusesByColumnsAsync();
                var pendingStatus = statusColumns.Pending.FirstOrDefault();
                var statusId = pendingStatus != null ? pendingStatus.OrderStatusId : 1;

                // Calculate estimated time and amount based on selected services
                decimal totalEstimatedAmount = 0;
                long totalEstimatedTime = 0;

                List<BusinessObject.Service> selectedServices = new List<BusinessObject.Service>();
                if (createRoDto.SelectedServiceIds != null && createRoDto.SelectedServiceIds.Any())
                {
                    selectedServices = await _serviceRepository.Query()
                        .Where(s => createRoDto.SelectedServiceIds.Contains(s.ServiceId))
                        .ToListAsync();

                    foreach (var service in selectedServices)
                    {
                        totalEstimatedAmount += service.Price;
                        totalEstimatedTime += (long)(service.EstimatedDuration * 60); // Convert hours to minutes
                    }

                }
                // T�nh to�n chi ph� kh?n c?p n?u c�
              

                    // Create a new repair order based on the simplified DTO
                    var repairOrder = new BusinessObject.RepairOrder
                {
                    VehicleId = createRoDto.VehicleId,
                    RoType = createRoDto.RoType,
                    ReceiveDate = createRoDto.ReceiveDate,
                    EstimatedCompletionDate = createRoDto.EstimatedCompletionDate,
                    EstimatedAmount = totalEstimatedAmount, 
                    Note = createRoDto.Note,
                    EstimatedRepairTime = totalEstimatedTime,
                    UserId = createRoDto.CustomerId,
                    StatusId = statusId,
                    BranchId = user.BranchId.Value, 
                    RepairRequestId = createRoDto.RepairRequestId,
                    PaidStatus = PaidStatus.Unpaid, 
                    Cost = 0, 
                    CreatedAt = DateTime.UtcNow 
                };

                var createdRepairOrder = await _repairOrderService.CreateRepairOrderAsync(repairOrder, createRoDto.SelectedServiceIds);
                
                // Return the created repair order
                var fullRepairOrder = await _repairOrderService.GetRepairOrderWithFullDetailsAsync(createdRepairOrder.RepairOrderId);
                var repairOrderDto = _repairOrderService.MapToRepairOrderDto(fullRepairOrder);


               // await _hubContext.Clients.All.SendAsync("RepairOrderCreated", repairOrderDto);
                return CreatedAtAction(nameof(GetRepairOrder), new { id = repairOrderDto.RepairOrderId }, repairOrderDto);
                
            }
            catch (Exception ex)
            {
                // Log the full exception details for debugging
                return BadRequest(new { 
                    message = "An error occurred while creating the repair order", 
                    details = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        
        }

        // PUT: api/RepairOrder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRepairOrder(Guid id, UpdateRepairOrderDto updateRepairOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedRepairOrder = await _repairOrderService.UpdateRepairOrderStatusNoteServicesAsync(id, updateRepairOrderDto);
                var repairOrderDto = _repairOrderService.MapToRepairOrderDto(updatedRepairOrder);
                return Ok(repairOrderDto);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/RepairOrder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepairOrder(Guid id)
        {
            var result = await _repairOrderService.DeleteRepairOrderAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/RepairOrder/kanban
        [HttpGet("kanban")]
        [EnableQuery]
        public async Task<IActionResult> GetKanbanBoard()
        {
            var kanbanBoard = await _repairOrderService.GetKanbanBoardAsync();
            return Ok(kanbanBoard);
        }

        // GET: api/RepairOrder/listview
        [HttpGet("listview")]
        [EnableQuery]
        public async Task<IActionResult> GetListView(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string sortBy = "ReceiveDate",
            [FromQuery] string sortOrder = "Desc",
            [FromQuery] RoBoardFiltersDto filters = null)
        {
            var listView = await _repairOrderService.GetListViewAsync(filters, sortBy, sortOrder, page, pageSize);
            return Ok(listView);
        }
        
        // GET: api/RepairOrder/status/{statusId}
        [HttpGet("status/{statusId}")]
        [EnableQuery]
        public async Task<IActionResult> GetRepairOrdersByStatus(int statusId) // Changed from Guid to int
        {
            var repairOrders = await _repairOrderService.GetRepairOrdersByStatusAsync(statusId);
            return Ok(repairOrders);
        }

        // GET: api/RepairOrder/branch/{branchId}
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetRepairOrdersByBranch(Guid branchId)
        {
            // Validate the branch ID
            if (branchId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid branch ID provided" });
            }

            // Get repair orders for the specified branch
            var repairOrders = await _repairOrderService.GetRepairOrdersByBranchAsync(branchId);
            return Ok(repairOrders);
        }

        // POST: api/RepairOrder/status/update
        // - Creating inspections (Pending → In Progress, Completed → In Progress if not fully paid)
        // - Creating quotations (Pending → In Progress, Completed → In Progress if not fully paid)
        [AllowAnonymous]
        [HttpPost("status/update")]
        public async Task<IActionResult> UpdateRepairOrderStatus([FromBody] UpdateRoBoardStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.UpdateRepairOrderStatusAsync(updateDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        
        // POST: api/RepairOrder/archive
        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveRepairOrder([FromBody] ArchiveRepairOrderDto archiveDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.ArchiveRepairOrderAsync(archiveDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        // POST: api/RepairOrder/restore
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreRepairOrder([FromBody] RestoreRepairOrderDto restoreDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.RestoreRepairOrderAsync(restoreDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        
        // GET: api/RepairOrder/archived
        [HttpGet("archived")]
        [EnableQuery]
        public async Task<IActionResult> GetArchivedRepairOrders(
            [FromQuery] Guid? branchId = null,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string sortBy = "ArchivedAt",
            [FromQuery] string sortOrder = "Desc")
        {
            var filters = new RoBoardFiltersDto
            {
                OnlyArchived = true
            };

            if (branchId.HasValue)
            {
                filters.BranchIds = new List<Guid> { branchId.Value };
            }

            var archivedOrders = await _repairOrderService.GetArchivedRepairOrdersAsync(filters, sortBy, sortOrder, page, pageSize);
            return Ok(archivedOrders);
        }

        // GET: api/RepairOrder/archived/{id}
        [HttpGet("archived/{id}")]
        public async Task<IActionResult> GetArchivedRepairOrderDetail(Guid id)
        {
            var archivedDetail = await _repairOrderService.GetArchivedRepairOrderDetailAsync(id);
            
            if (archivedDetail == null)
            {
                return NotFound(new { message = "Archived repair order not found or repair order is not archived" });
            }

            return Ok(archivedDetail);
        }
        
        // POST: api/RepairOrder/cancel
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelRepairOrder([FromBody] CancelRepairOrderDto cancelDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _repairOrderService.CancelRepairOrderAsync(cancelDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // PUT: api/RepairOrder/{id}/labels
        [HttpPut("{id}/labels")]
        public async Task<IActionResult> UpdateRepairOrderLabels(Guid id, [FromBody] UpdateRepairOrderLabelsDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _repairOrderService.UpdateRepairOrderLabelsAsync(id, dto.LabelIds);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // GET: api/RepairOrder/{id}/customer-vehicle-info
        [HttpGet("{id}/customer-vehicle-info")]
        public async Task<IActionResult> GetCustomerVehicleInfo(Guid id)
        {
            try
            {
                var info = await _repairOrderService.GetCustomerVehicleInfoAsync(id);
                
                if (info == null)
                {
                    return NotFound(new { message = "Repair order not found" });
                }

                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
        
    }

    // DTO for updating labels
    public class UpdateRepairOrderLabelsDto
    {
        public List<Guid> LabelIds { get; set; } = new List<Guid>();
    }
}