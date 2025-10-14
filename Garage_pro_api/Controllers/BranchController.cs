using Dtos.Branches;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.BranchServices;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

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

        // GET: api/branch/{id}
        [HttpGet("{id}")]
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
                return BadRequest(new { message = ex });
            }
        }

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
                return BadRequest(new { message = ex });
            }
        }
    }
}
