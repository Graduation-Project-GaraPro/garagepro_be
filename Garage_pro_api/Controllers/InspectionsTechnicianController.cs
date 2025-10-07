using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Enums;
using Dtos.Technician;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.Technician;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    public class InspectionsTechnicianController : ODataController
    {
        private readonly IInspectionTechnicianService _inspectionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InspectionsTechnicianController(IInspectionTechnicianService inspectionService, UserManager<ApplicationUser> userManager)
        {
            _inspectionService = inspectionService;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách Inspection của Technician hiện tại
        /// Chỉ hiển thị các inspection có trạng thái: New, Pending, InProgress, Completed
        /// Hỗ trợ OData: $select, $filter, $orderby, $top, $skip, $expand, $count
        /// </summary>
        /// <example>
        /// GET /odata/Inspections/my-inspections
        /// GET /odata/Inspections/my-inspections?$select=InspectionId,Status,CustomerConcern
        /// GET /odata/Inspections/my-inspections?$filter=Status eq 'New'
        /// GET /odata/Inspections/my-inspections?$orderby=CreatedAt desc
        /// GET /odata/Inspections/my-inspections?$top=10&$skip=0
        /// GET /odata/Inspections/my-inspections?$count=true
        /// </example>
        [HttpGet("my-inspections")]
        [Authorize(Roles = "Technician")]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        public async Task<IActionResult> GetMyInspections()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập để xem danh sách kiểm tra xe." });
            }

            var inspections = await _inspectionService.GetInspectionsByTechnicianAsync(user.Id);

            var result = inspections.Select(inspection => new
            {
                inspection.InspectionId,
                inspection.RepairOrderId,
                inspection.TechnicianId,
                inspection.Status,
                StatusText = inspection.Status.ToString(),
                inspection.CustomerConcern,
                inspection.Finding,
                inspection.CreatedAt,
                inspection.UpdatedAt,
                RepairOrder = inspection.RepairOrder != null ? new
                {
                    inspection.RepairOrder.RepairOrderId,
                    Vehicle = inspection.RepairOrder.Vehicle != null ? new
                    {
                        inspection.RepairOrder.Vehicle.VehicleId,
                        inspection.RepairOrder.Vehicle.LicensePlate,
                        inspection.RepairOrder.Vehicle.VIN,
                        BrandId = inspection.RepairOrder.Vehicle.BrandId,
                        ModelId = inspection.RepairOrder.Vehicle.ModelId,
                        ColorId = inspection.RepairOrder.Vehicle.ColorId
                    } : null,
                    Customer = inspection.RepairOrder.Vehicle?.User != null ? new
                    {
                        CustomerId = inspection.RepairOrder.Vehicle.User.Id,
                        FullName = $"{inspection.RepairOrder.Vehicle.User.FirstName} {inspection.RepairOrder.Vehicle.User.LastName}".Trim(),
                        Email = inspection.RepairOrder.Vehicle.User.Email,
                        PhoneNumber = inspection.RepairOrder.Vehicle.User.PhoneNumber
                    } : null ,
                    Services = inspection.RepairOrder.RepairOrderServices?.Select(ros => new
                    {
                        ros.RepairOrderServiceId,
                        ros.ServiceId,
                        ServiceName = ros.Service?.ServiceName,
                        ros.ServicePrice,
                        ros.ActualDuration,
                        ros.Notes
                    }).ToList()
                } : null,
                ServiceInspections = inspection.ServiceInspections?.Select(si => new
                {
                    si.ServiceInspectionId,
                    si.ServiceId,
                    ServiceName = si.Service?.ServiceName,
                    si.ConditionStatus,
                    si.CreatedAt
                }).ToList(),
                PartInspections = inspection.PartInspections?.Select(pi => new
                {
                    pi.PartInspectionId,
                    pi.PartId,
                    PartName = pi.Part?.Name,
                    pi.Status,
                    pi.CreatedAt
                }).ToList(),
                CanUpdate = inspection.Status == InspectionStatus.New ||
                           inspection.Status == InspectionStatus.Pending ||
                           inspection.Status == InspectionStatus.InProgress
            }).AsQueryable();

            return Ok(result);
        }


        /// <summary>
        /// Lấy thông tin chi tiết một Inspection
        /// Chỉ Technician được giao mới có thể xem
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Technician")]
        [EnableQuery]
        public async Task<IActionResult> GetInspectionById(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });
            }

            var inspection = await _inspectionService.GetInspectionByIdAsync(id, user.Id);
            if (inspection == null)
            {
                return NotFound(new { Message = "Không tìm thấy kiểm tra xe hoặc bạn không có quyền truy cập." });
            }

            var result = new
            {
                inspection.InspectionId,
                inspection.RepairOrderId,
                inspection.TechnicianId,
                inspection.Status,
                StatusText = inspection.Status.ToString(),
                inspection.CustomerConcern,
                inspection.Finding,
                inspection.CreatedAt,
                inspection.UpdatedAt,

                RepairOrder = inspection.RepairOrder != null ? new
                {
                    inspection.RepairOrder.RepairOrderId,
                    Vehicle = inspection.RepairOrder.Vehicle != null ? new
                    {
                        inspection.RepairOrder.Vehicle.VehicleId,
                        inspection.RepairOrder.Vehicle.LicensePlate,
                        inspection.RepairOrder.Vehicle.VIN,
                        BrandId = inspection.RepairOrder.Vehicle.BrandId,
                        ModelId = inspection.RepairOrder.Vehicle.ModelId,
                        ColorId = inspection.RepairOrder.Vehicle.ColorId
                    } : null,
                    Customer = inspection.RepairOrder.Vehicle?.User != null ? new
                    {
                        CustomerId = inspection.RepairOrder.Vehicle.User.Id,
                        FullName = $"{inspection.RepairOrder.Vehicle.User.FirstName} {inspection.RepairOrder.Vehicle.User.LastName}".Trim(),
                        Email = inspection.RepairOrder.Vehicle.User.Email,
                        PhoneNumber = inspection.RepairOrder.Vehicle.User.PhoneNumber
                    } : null,
                    Services = inspection.RepairOrder.RepairOrderServices?.Select(ros => new
                    {
                        ros.ServiceId,
                        ServiceName = ros.Service?.ServiceName,
                        ros.ServicePrice,
                        ros.ActualDuration,
                        ros.Notes
                    }).ToList()
                } : null,

                ServiceInspections = inspection.ServiceInspections?.Select(si => new
                {
                    si.ServiceInspectionId,
                    si.ServiceId,
                    ServiceName = si.Service?.ServiceName,
                    si.ConditionStatus,
                    si.CreatedAt
                }).ToList(),

                PartInspections = inspection.PartInspections?.Select(pi => new
                {
                    pi.PartInspectionId,
                    pi.PartId,
                    PartName = pi.Part?.Name,
                    pi.Status,
                    pi.CreatedAt
                }).ToList(),

                CanUpdate = inspection.Status == InspectionStatus.New ||
                           inspection.Status == InspectionStatus.Pending ||
                           inspection.Status == InspectionStatus.InProgress
            };

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật kết quả kiểm tra xe
        /// - Chỉ Technician được giao mới có thể cập nhật
        /// - Chỉ cập nhật được khi trạng thái là New, Pending, InProgress
        /// - Technician viết Finding (kết quả kiểm tra) và gợi ý Parts cần thay thế
        /// - Khi hoàn thành (IsCompleted = true), trạng thái chuyển từ New sang Pending để Manager review
        /// </summary>
        /// <param name="id">ID Inspection</param>
        /// <param name="request">Thông tin cập nhật</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> UpdateInspection(Guid id, [FromBody] UpdateInspectionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });
            }

            if (string.IsNullOrWhiteSpace(request.Finding))
            {
                return BadRequest(new { Message = "Vui lòng nhập kết quả kiểm tra (Finding)." });
            }

            if (request.ServiceUpdates == null || !request.ServiceUpdates.Any())
            {
                return BadRequest(new { Message = "Vui lòng nhập danh sách dịch vụ cần cập nhật." });
            }

            try
            {
                var updatedInspection = await _inspectionService.UpdateInspectionAsync(id, request, user.Id);

                return Ok(new
                {
                    Message = request.IsCompleted
                        ? "Kiểm tra xe đã hoàn thành và gửi cho Manager xem xét."
                        : "Cập nhật kiểm tra xe thành công.",
                    Inspection = new
                    {
                        updatedInspection.InspectionId,
                        updatedInspection.Status,
                        StatusText = updatedInspection.Status.ToString(),
                        updatedInspection.Finding,
                        updatedInspection.UpdatedAt,
                        ServiceInspections = updatedInspection.ServiceInspections?.Select(si => new
                        {
                            si.ServiceInspectionId,
                            si.ServiceId,
                            ServiceName = si.Service?.ServiceName,
                            si.ConditionStatus
                        }).ToList(),
                        PartInspections = updatedInspection.PartInspections?.Select(pi => new
                        {
                            pi.PartInspectionId,
                            pi.PartId,
                            PartName = pi.Part?.Name,
                            pi.Status
                        }).ToList()
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi logic từ repo (VD: part không thuộc service, gợi ý sai trạng thái)
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi khác
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi cập nhật kiểm tra xe.", Error = ex.Message });
            }
        }


        /// <summary>
        /// Bắt đầu kiểm tra xe (chuyển trạng thái từ New sang InProgress)
        /// </summary>
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> StartInspection(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });
            }

            var inspection = await _inspectionService.StartInspectionAsync(id, user.Id);
            if (inspection == null)
            {
                return BadRequest(new
                {
                    Message = "Không thể bắt đầu kiểm tra. Inspection không tồn tại hoặc không ở trạng thái New."
                });
            }

            return Ok(new
            {
                Message = "Đã bắt đầu kiểm tra xe.",
                Inspection = new
                {
                    inspection.InspectionId,
                    inspection.Status,
                    StatusText = inspection.Status.ToString(),
                    inspection.UpdatedAt
                }
            });
        }
    }
}