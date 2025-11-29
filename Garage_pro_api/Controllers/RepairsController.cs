using AutoMapper;
using BusinessObject.Authentication;
using Dtos.InspectionAndRepair;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Services.InspectionAndRepair;
using System;
using System.Threading.Tasks;
using DataAccessLayer;
using System.Text.Json;

namespace Garage_pro_api.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
  
    public class RepairsController : ODataController
    {
        private readonly IRepairService _repairService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyAppDbContext _context;

        public RepairsController(IRepairService repairService, UserManager<ApplicationUser> userManager, MyAppDbContext context)
        {
            _repairService = repairService;
            _userManager = userManager;
            _context = context;
        }

        private async Task<Guid> GetTechnicianIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Guid.Empty;

            var technician = await _context.Technicians
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            return technician?.TechnicianId ?? Guid.Empty;
        }

        [EnableQuery]
        [HttpGet("{repairOrderId}")]
        [Authorize("REPAIR_VIEW")]
        public async Task<IActionResult> GetRepairOrderDetails(Guid repairOrderId)
        {
            var technicianId = await GetTechnicianIdAsync();
            if (technicianId == Guid.Empty)
                return Unauthorized(new { Message = "Vui lòng đăng nhập bằng tài khoản Technician." });

            try
            {
                var result = await _repairService.GetRepairOrderDetailsAsync(repairOrderId, technicianId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Create")]
        [Authorize("REPAIR_CREATE")]
        public async Task<IActionResult> CreateRepair([FromBody] RepairCreateDto dto)
        {
            var technicianId = await GetTechnicianIdAsync();
            if (technicianId == Guid.Empty)
                return Unauthorized(new { message = "Vui lòng đăng nhập bằng tài khoản Technician." });

            try
            {
                var repairDto = await _repairService.CreateRepairAsync(technicianId, dto);
                return CreatedAtAction(nameof(GetRepairOrderDetails),
                    new { repairOrderId = repairDto.RepairOrderId },
                    repairDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)  
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)  
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
               
                return BadRequest(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace, 
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{repairId}/Update")]
        [Authorize("REPAIR_UPDATE")]
        public async Task<IActionResult> UpdateRepair(Guid repairId, [FromBody] RepairUpdateDto dto)
        {
            var technicianId = await GetTechnicianIdAsync();
            if (technicianId == Guid.Empty)
                return Unauthorized(new { message = "Vui lòng đăng nhập bằng tài khoản Technician." });

            try
            {
                await _repairService.UpdateRepairAsync(technicianId, repairId, dto);
                return Ok(new { message = "Cập nhật Repair thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
