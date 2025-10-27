using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessObject;
using Services;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class OrderStatusController : ControllerBase
    {
        private readonly IOrderStatusService _orderStatusService;

        public OrderStatusController(IOrderStatusService orderStatusService)
        {
            _orderStatusService = orderStatusService;
        }

        // GET: api/orderstatus/columns
        [HttpGet("columns")]
        [AllowAnonymous] // Allow anonymous access for testing
        public async Task<IActionResult> GetOrderStatusesByColumns()
        {
            try
            {
                var statusColumns = await _orderStatusService.GetOrderStatusesByColumnsAsync();
                return Ok(statusColumns);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving order statuses", error = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous] // Allow anonymous access for testing
        public async Task<IActionResult> GetOrderStatuses()
        {
            try
            {
                var statusColumns = await _orderStatusService.GetAllOrderStatusAsync();
                return Ok(statusColumns);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving order statuses", error = ex.Message });
            }
        }

        // GET: api/orderstatus/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderStatusById(int id) // Changed from Guid to int
        {
            try
            {
                var orderStatus = await _orderStatusService.GetOrderStatusByIdAsync(id);
                if (orderStatus == null)
                    return NotFound(new { message = $"Order status with ID {id} not found" });

                return Ok(orderStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the order status", error = ex.Message });
            }
        }

        // GET: api/orderstatus/{id}/labels
        [HttpGet("{id}/labels")]
        public async Task<IActionResult> GetLabelsByOrderStatusId(int id) // Changed from Guid to int
        {
            try
            {
                var labels = await _orderStatusService.GetLabelsByOrderStatusIdAsync(id);
                return Ok(labels);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving labels", error = ex.Message });
            }
        }

        // POST: api/orderstatus/initialize
        [HttpPost("initialize")]
        [Authorize(Roles = "Manager")] // Only managers can initialize default statuses
        public async Task<IActionResult> InitializeDefaultStatuses()
        {
            try
            {
                await _orderStatusService.InitializeDefaultStatusesAsync();
                return Ok(new { message = "Default statuses initialized successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while initializing default statuses", error = ex.Message });
            }
        }

        // GET: api/orderstatus/{id}/exists
        [HttpGet("{id}/exists")]
        public async Task<IActionResult> CheckOrderStatusExists(int id) // Changed from Guid to int
        {
            try
            {
                var exists = await _orderStatusService.OrderStatusExistsAsync(id);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking order status existence", error = ex.Message });
            }
        }
    }
}