using Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.QuotationService;
using System.Security.Claims;

namespace Garage_pro_api.Controllers.Customer
{
        [Route("api/[controller]")]
        [ApiController]
    [Authorize]
    public class QuotationController : ControllerBase
        {
            private readonly IQuotationService _quotationService;

            public QuotationController(IQuotationService quotationService)
            {
                _quotationService = quotationService;
            }

        //lấy all báo giá của customer
        [HttpGet("all")]
            public async Task<ActionResult<List<QuotationDto>>> GetAllQuotations()
            {
            // Lấy userId từ token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var quotations = await _quotationService.GetQuotationsByUserIdAsync(userId);
                return Ok(quotations);
            }

         
            // Lấy báo giá theo RepairRequestId
            [HttpGet("repair-request/{repairRequestId}")]
            public async Task<ActionResult<List<QuotationDto>>> GetQuotationsByRepairRequestId(Guid repairRequestId)
            {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var quotations = await _quotationService.GetQuotationsByRepairRequestIdAsync(userId, repairRequestId);
                return Ok(quotations);
            }
        //apporve báo giá
        [HttpPost("approve/{quotationId}")]
            public async Task<ActionResult<QuotationDto>> ApproveQuotation(Guid quotationId)
            {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                var approvedQuotation = await _quotationService.ApproveQuotationAsync( quotationId);
                return Ok(approvedQuotation);
        }
        //customer từ chối báo giá
        [HttpPost("reject/{quotationId}")]
            public async Task<ActionResult<QuotationDto>> RejectQuotation(Guid quotationId)
            {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                var rejectedQuotation = await _quotationService.RejectQuotationAsync( quotationId);
                return Ok(rejectedQuotation);
        }


        /// Customer cập nhật Part (chọn Spec)           
        // [HttpPut("update-parts")]
        // public async Task<ActionResult<QuotationDto>> UpdateQuotationParts(UpdateQuotationPartsDto dto)
        // {
        //     var userId = User.FindFirst("sub")?.Value;
        //     if (string.IsNullOrEmpty(userId))
        //         return Unauthorized();

        //     var updatedQuotation = await _quotationService.UpdateQuotationPartsAsync(userId, dto);
        //     return Ok(updatedQuotation);
        // }
    }
}


