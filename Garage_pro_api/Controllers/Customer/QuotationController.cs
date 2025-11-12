﻿using Dtos.Quotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.QuotationServices;
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
        //laasy baso gia cua usser ddos 
        [HttpGet("{UserId}")]
        public async Task<ActionResult<List<QuotationDto>>> GetQuotationsByUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
          ?? User.FindFirstValue("sub"); // hoặc tên claim chứa idUser
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var quotations = await _quotationService.GetQuotationsByUserIdAsync(userId);
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
            var approvedQuotation = await _quotationService.ApproveQuotationAsync(quotationId);

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
            var rejectedQuotation = await _quotationService.RejectQuotationAsync(quotationId);
            return Ok(rejectedQuotation);
        }

        // Customer response to quotation with service selections
        [HttpPost("response")]
        public async Task<ActionResult<QuotationDto>> CustomerResponse(CustomerQuotationResponseDto responseDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
                
            // Verify the quotation belongs to this user
            var quotation = await _quotationService.GetQuotationByIdAsync(responseDto.QuotationId);
            if (quotation == null || quotation.UserId != userId)
                return Unauthorized();
                
            try
            {
                var updatedQuotation = await _quotationService.ProcessCustomerResponseAsync(responseDto);
                return Ok(updatedQuotation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
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