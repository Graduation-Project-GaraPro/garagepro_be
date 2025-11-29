using BusinessObject.Branches;
using Dtos.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.BranchServices;
using Services; // Add this for IUserService
using System.Security.Claims; // Add this for ClaimTypes

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;
        private readonly IUserService _userService; // Add this

        public BranchController(IBranchService branchService, IUserService userService) // Update constructor
        {
            _branchService = branchService;
            _userService = userService;
        }
        [Authorize(Policy = "BRANCH_VIEW")]

        // GET: api/branch
        [HttpGet]
        public async Task<IActionResult> GetAllBranches(
         int page = 1,
         int pageSize = 10,
         string? search = null,
         string? city = null,
         bool? isActive = null)
        {
            try
            {
                var result = await _branchService.GetAllBranchesAsync(page, pageSize, search, city, isActive);

                return Ok(new
                {
                    branches = result.Branches,
                    totalCount = result.TotalCount,
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllBranchesBasis")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBranchesBasis()
                {
            try
            {
                var result = await _branchService.GetAllBranchesBasicAsync();

                // Project only the fields you want to expose
                var branches = result.Select(s => new
                {
                    s.BranchId,
                    s.BranchName,
                    s.Commune,
                    s.Province,
                    s.Street,
                    s.PhoneNumber ,
                    s.Email,
                    s.Description ,
                    s.IsActive 
            });

                return Ok(branches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        // GET: api/branch/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyBranch()
        {
            try
            {
                // Get the authenticated user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Get the user from the user service
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if user has a branch assigned
                if (!user.BranchId.HasValue)
                {
                    return NotFound(new { message = "User is not assigned to any branch" });
                }

                // Get the branch information
                var branch = await _branchService.GetBranchByIdAsync(user.BranchId.Value);
                if (branch == null)
                {
                    return NotFound(new { message = "Branch not found" });
                }

                return Ok(branch);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [Authorize(Policy = "BRANCH_VIEW")]

        // GET: api/branch/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBranchById(Guid id)
        {
            try
            {
                var branch = await _branchService.GetBranchByIdAsync(id);
                if (branch == null) return NotFound(new { message = "Branch not found" });
                return Ok(branch);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //[Authorize(Policy = "BRANCH_CREATE")]

        // POST: api/branch
        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody] BranchCreateDto dto)
        {
            try
            {
                var result = await _branchService.CreateBranchAsync(dto);
                return CreatedAtAction(nameof(GetBranchById), new { id = result.BranchId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //[Authorize(Policy = "BRANCH_UPDATE")]

        // PUT: api/branch/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] BranchUpdateDto dto)
        {
            try
            {
                if (id != dto.BranchId) return BadRequest(new { message = "BranchId mismatch" });

                var result = await _branchService.UpdateBranchAsync(dto);
                if (result == null) return NotFound(new { message = "Branch not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Policy = "BRANCH_STATUS_TOGGLE")]

        // Kích hoạt nhiều chi nhánh
        [HttpPatch("bulk-activate")]
        public async Task<IActionResult> BulkActivate([FromBody] IEnumerable<Guid> branchIds)
        {
            if (branchIds == null || !branchIds.Any())
                return BadRequest("No branch IDs provided.");

            try
            {
                await _branchService.UpdateIsActiveForManyAsync(branchIds, true);
                return Ok(new { message = "Branches activated successfully." });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to activate branches.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [Authorize(Policy = "BRANCH_STATUS_TOGGLE")]

        // Vô hiệu hóa nhiều chi nhánh
        [HttpPatch("bulk-deactivate")]
        public async Task<IActionResult> BulkDeactivate([FromBody] IEnumerable<Guid> branchIds)
        {
            if (branchIds == null || !branchIds.Any())
                return BadRequest("No branch IDs provided.");

            try
            {
                await _branchService.UpdateIsActiveForManyAsync(branchIds, false);
                return Ok(new { message = "Branches deactivated successfully." });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to deactivate branches.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [Authorize(Policy = "BRANCH_STATUS_TOGGLE")]

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] ToggleBranchStatusRequest request)
        {
            if (request == null)
                return BadRequest("Request body is missing.");

            try
            {
                var branch = await _branchService.GetBranchByIdAsync(id);
                if (branch == null)
                    return NotFound($"Branch with ID {id} not found.");

                await _branchService.UpdateIsActiveForManyAsync(new List<Guid> { id }, request.IsActive);

                string statusText = request.IsActive ? "activated" : "deactivated";
                return Ok(new { message = $"Branch {statusText} successfully." });
            }
            catch (ApplicationException ex)
            {
                //_logger.LogError(ex, "Failed to toggle branch status for ID {BranchId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Unexpected error when toggling branch status for ID {BranchId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        [Authorize(Policy = "BRANCH_DELETE")]

        // Xóa nhiều chi nhánh
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<Guid> branchIds)
        {
            if (branchIds == null || !branchIds.Any())
                return BadRequest("No branch IDs provided.");

            try
            {
                await _branchService.DeleteManyAsync(branchIds);
                return Ok(new { message = "Branches deleted successfully." });
            }
            catch (ApplicationException ex)
            {
                //_logger.LogError(ex, "Failed to delete branches.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [Authorize(Policy = "BRANCH_DELETE")]

        // DELETE: api/branch/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(Guid id)
        {
            try
            {
                var success = await _branchService.DeleteBranchAsync(id);
                if (!success) return NotFound(new { message = "Branch not found" });

                return Ok(new { message = "Branch deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        // GET: api/branch/{id}/technicians
        [HttpGet("{id}/technicians")]
        [Authorize(Policy = "BRANCH_VIEW")]
        public async Task<IActionResult> GetTechniciansByBranch(Guid id)
        {
            try
            {
                var users = await _userService.GetTechniciansByBranchAsync(id);
                return Ok(users.Select(u => new {
                    u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    u.Email,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLogin,
                    u.BranchId
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
