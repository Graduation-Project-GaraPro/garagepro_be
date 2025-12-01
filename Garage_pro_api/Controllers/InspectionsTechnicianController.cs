using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Enums;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services.InspectionAndRepair;

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

        [HttpGet("my-inspections")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetMyInspections()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "Bạn cần đăng nhập để xem danh sách kiểm tra xe." });

            var inspections = await _inspectionService.GetInspectionsByTechnicianAsync(user.Id);
            if (inspections == null || !inspections.Any())
                return Ok(new { Message = "Hiện tại bạn chưa được phân công kiểm tra xe nào." });

            return Ok(inspections); 
        }

        [HttpGet("{id}")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetInspectionById(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            var dto = await _inspectionService.GetInspectionByIdAsync(id, user.Id);
            if (dto == null) return NotFound(new { Message = "Không tìm thấy kiểm tra xe hoặc bạn không có quyền truy cập." });

            return Ok(dto);
        }

        [HttpPut("{id}")]
        [Authorize("INSPECTION_TECHNICIAN_UPDATE")]
        public async Task<IActionResult> UpdateInspection(Guid id, [FromBody] UpdateInspectionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            try
            {
                var updatedDto = await _inspectionService.UpdateInspectionAsync(id, request, user.Id);
                return Ok(new
                {
                    Message = request.IsCompleted ? "Kiểm tra xe đã hoàn thành và gửi cho Manager xem xét." : "Cập nhật kiểm tra xe thành công.",
                    Inspection = updatedDto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi cập nhật kiểm tra xe.", Error = ex.Message });
            }
        }
        [HttpDelete("{inspectionId}/services/{serviceId}/part-inspections/{partInspectionId}")]
        [Authorize("INSPECTION_TECHNICIAN_DELETE")]
        public async Task<IActionResult> RemovePartFromInspection(Guid inspectionId, Guid serviceId, Guid partInspectionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            try
            {
                await _inspectionService.RemovePartFromInspectionAsync(inspectionId, serviceId, partInspectionId, user.Id);
                return Ok(new { Message = "Đã xóa phụ tùng khỏi Inspection thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi xóa phụ tùng khỏi Inspection.", Error = ex.Message });
            }
        }

        [HttpDelete("{inspectionId}/services/{serviceInspectionId}/part-categories/{partCategoryId}")]
        [Authorize("INSPECTION_TECHNICIAN_DELETE")]
        public async Task<IActionResult> RemovePartCategoryFromService(
            Guid inspectionId,
            Guid serviceInspectionId,
            Guid partCategoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            try
            {
                var dto = await _inspectionService.RemovePartCategoryFromServiceAsync(
                    inspectionId,
                    serviceInspectionId,
                    partCategoryId,
                    user.Id
                );

                return Ok(new
                {
                    Message = "Đã xóa PartCategory và các parts liên quan thành công.",
                    Inspection = dto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Đã xảy ra lỗi khi xóa PartCategory.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id}/start")]
        [Authorize("INSPECTION_TECHNICIAN_UPDATE")]
        public async Task<IActionResult> StartInspection(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            try
            {
                var dto = await _inspectionService.StartInspectionAsync(id, user.Id);
                return Ok(new { Message = "Đã bắt đầu kiểm tra xe.", Inspection = dto });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi.", Error = ex.Message });
            }
        }

        [HttpGet("services")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                var services = await _inspectionService.GetAllServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi lấy danh sách service.", Error = ex.Message });
            }
        }

        [HttpPost("{inspectionId}/services")]
        [Authorize("INSPECTION_ADD_SERVICE")]
        public async Task<IActionResult> AddServiceToInspection(Guid inspectionId, [FromBody] AddServiceToInspectionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            try
            {
                var dto = await _inspectionService.AddServiceToInspectionAsync(inspectionId, request, user.Id);
                return Ok(new
                {
                    Message = "Thêm service vào Inspection thành công.",
                    Inspection = dto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi thêm service vào Inspection.", Error = ex.Message });
            }
        }

        [HttpDelete("{inspectionId}/services/{serviceInspectionId}")]
        [Authorize("INSPECTION_TECHNICIAN_DELETE")]
        public async Task<IActionResult> RemoveServiceFromInspection(Guid inspectionId, Guid serviceInspectionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            try
            {
                var dto = await _inspectionService.RemoveServiceFromInspectionAsync(inspectionId, serviceInspectionId, user.Id);
                return Ok(new
                {
                    Message = "Xóa service khỏi Inspection thành công.",
                    Inspection = dto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi xóa service khỏi Inspection.", Error = ex.Message });
            }
        }
        [HttpGet("my-technician-id")]
        [Authorize("INSPECTION_TECHNICIAN_VIEW")]
        public async Task<IActionResult> GetMyTechnicianId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { Message = "Bạn cần đăng nhập." });

            var technician = await _inspectionService.GetTechnicianByUserIdAsync(user.Id);
            if (technician == null)
                return NotFound(new { Message = "Không tìm thấy thông tin technician." });

            return Ok(new { TechnicianId = technician.TechnicianId });
        }

    }
}