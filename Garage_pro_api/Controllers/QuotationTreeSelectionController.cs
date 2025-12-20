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

        /// GET: api/QuotationTreeSelection/service/{serviceId}?modelId={modelId}
        /// Updated to include vehicle model for model-specific part categories
        [HttpGet("service/{serviceId}")]
        public async Task<ActionResult<ServiceDetailsDto>> GetServiceDetails(Guid serviceId, [FromQuery] Guid? modelId = null)
        {
            try
            {
                var result = await _treeSelectionService.GetServiceDetailsAsync(serviceId, modelId);
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
        /// Updated to work with model-specific part categories
        /// Optional modelId parameter to filter parts by vehicle model
        [HttpGet("parts/category/{partCategoryId}")]
        public async Task<ActionResult<List<PartForSelectionDto>>> GetPartsByCategory(Guid partCategoryId, [FromQuery] Guid? modelId = null)
        {
            try
            {
                var result = await _treeSelectionService.GetPartsByCategoryAsync(partCategoryId, modelId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving parts", Detail = ex.Message });
            }
        }

        /// GET: api/QuotationTreeSelection/parts/model/{modelId}/category/{categoryName}
        /// NEW: Get parts by model and category name (for model-specific categories)
        [HttpGet("parts/model/{modelId}/category/{categoryName}")]
        public async Task<ActionResult<List<PartForSelectionDto>>> GetPartsByModelAndCategory(Guid modelId, string categoryName)
        {
            try
            {
                var result = await _treeSelectionService.GetPartsByModelAndCategoryAsync(modelId, categoryName);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving parts", Detail = ex.Message });
            }
        }
    }
}
