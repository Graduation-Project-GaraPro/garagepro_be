using Dtos.Quotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.QuotationServices;
using System;
using System.Threading.Tasks;

namespace Garage_pro_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")] // Only managers can access
    public class QuotationTreeSelectionController : ControllerBase
    {
        private readonly IQuotationTreeSelectionService _treeSelectionService;

        public QuotationTreeSelectionController(IQuotationTreeSelectionService treeSelectionService)
        {
            _treeSelectionService = treeSelectionService;
        }

        /// GET: api/QuotationTreeSelection/root
        /// Get root-level service categories
        [HttpGet("root")]
        public async Task<ActionResult<ServiceCategoryTreeResponseDto>> GetRootCategories()
        {
            try
            {
                var result = await _treeSelectionService.GetRootCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving root categories", Detail = ex.Message });
            }
        }

        /// GET: api/QuotationTreeSelection/category/{categoryId}

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ServiceCategoryTreeResponseDto>> GetCategoryChildren(Guid categoryId)
        {
            try
            {
                var result = await _treeSelectionService.GetCategoryChildrenAsync(categoryId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving category children", Detail = ex.Message });
            }
        }

        [HttpGet("service/{serviceId}")]
        public async Task<ActionResult<ServiceDetailsDto>> GetServiceDetails(Guid serviceId)
        {
            try
            {
                var result = await _treeSelectionService.GetServiceDetailsAsync(serviceId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving service details", Detail = ex.Message });
            }
        }

        /// GET: api/QuotationTreeSelection/parts/category/{partCategoryId}
        [HttpGet("parts/category/{partCategoryId}")]
        public async Task<ActionResult<List<PartForSelectionDto>>> GetPartsByCategory(Guid partCategoryId)
        {
            try
            {
                var result = await _treeSelectionService.GetPartsByCategoryAsync(partCategoryId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving parts", Detail = ex.Message });
            }
        }
    }
}
