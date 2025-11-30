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

        /// <summary>
        /// GET: api/QuotationTreeSelection/root
        /// Get root-level service categories (starting point of the tree)
        /// </summary>
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

        /// <summary>
        /// GET: api/QuotationTreeSelection/category/{categoryId}
        /// Get child categories and services for a specific category
        /// Drill down one level in the tree
        /// </summary>
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


    }
}
