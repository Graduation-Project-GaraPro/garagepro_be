using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;
using System;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestSignalRController : ControllerBase
    {
        private readonly IHubContext<EmergencyRequestHub> _hubContext;

        public TestSignalRController(IHubContext<EmergencyRequestHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Test gửi notification EmergencyRequestCreated
        /// </summary>
        [HttpPost("test-created")]
        public async Task<IActionResult> TestEmergencyCreated()
        {
            try
            {
                var testData = new
                {
                    EmergencyRequestId = Guid.NewGuid(),
                    Status = "Pending",
                    CustomerId = "test-customer-id",
                    BranchId = Guid.NewGuid(),
                    VehicleId = Guid.NewGuid(),
                    IssueDescription = "Test emergency request",
                    Latitude = 10.762622,
                    Longitude = 106.660172,
                    RequestTime = DateTime.UtcNow,
                    CustomerName = "Test Customer",
                    CustomerPhone = "0123456789",
                    BranchName = "Test Branch",
                    Message = "Có yêu cầu cứu hộ mới (TEST)",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestCreated", testData);
                
                Console.WriteLine($"[TEST] Sent EmergencyRequestCreated: {testData.EmergencyRequestId}");
                
                return Ok(new { 
                    Success = true, 
                    Message = "Test notification sent",
                    Data = testData 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test gửi notification EmergencyRequestApproved
        /// </summary>
        [HttpPost("test-approved")]
        public async Task<IActionResult> TestEmergencyApproved()
        {
            try
            {
                var testData = new
                {
                    EmergencyRequestId = Guid.NewGuid(),
                    Status = "Accepted",
                    CustomerId = "test-customer-id",
                    BranchId = Guid.NewGuid(),
                    EstimatedCost = 500000m,
                    DistanceToGarageKm = 5.5,
                    Message = "Yêu cầu cứu hộ đã được duyệt (TEST)",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestApproved", testData);
                
                Console.WriteLine($"[TEST] Sent EmergencyRequestApproved: {testData.EmergencyRequestId}");
                
                return Ok(new { 
                    Success = true, 
                    Message = "Test notification sent",
                    Data = testData 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test gửi notification EmergencyRequestRejected
        /// </summary>
        [HttpPost("test-rejected")]
        public async Task<IActionResult> TestEmergencyRejected()
        {
            try
            {
                var testData = new
                {
                    EmergencyRequestId = Guid.NewGuid(),
                    Status = "Canceled",
                    CustomerId = "test-customer-id",
                    BranchId = Guid.NewGuid(),
                    RejectReason = "Test reject reason - Không đủ nhân lực",
                    Message = "Yêu cầu cứu hộ đã bị từ chối (TEST)",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("EmergencyRequestRejected", testData);
                
                Console.WriteLine($"[TEST] Sent EmergencyRequestRejected: {testData.EmergencyRequestId}");
                
                return Ok(new { 
                    Success = true, 
                    Message = "Test notification sent",
                    Data = testData 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}

