using Microsoft.AspNetCore.Mvc;
using System;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

[ApiController]
[Route("api/[controller]")]
public class TestVnPayController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IVnpay _vnpay;

    public TestVnPayController(IConfiguration config, IVnpay vnpay)
    {
        _config = config;
        _vnpay = vnpay;
    }

    [HttpGet("CreatePaymentUrl")]
    public ActionResult<string> CreatePaymentUrl(double moneyToPay, string description)
        {
        try
        {
            var ipAddress = NetworkHelper.GetIpAddress(HttpContext); // Lấy địa chỉ IP của thiết bị thực hiện giao dịch

            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = moneyToPay,
                Description = description,
                IpAddress = ipAddress,
                BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
            };

            var paymentUrl = _vnpay.GetPaymentUrl(request);

            return Created(paymentUrl, paymentUrl);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("IpnAction")]
    public IActionResult IpnAction()
    {
        if (Request.QueryString.HasValue)
        {
            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (paymentResult.IsSuccess)
                {
                    // Thực hiện hành động nếu thanh toán thành công tại đây.
                    // Ví dụ: Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu.
                    //Manaager caapj nhat 
                    return Ok();
                }

                // Thực hiện hành động nếu thanh toán thất bại tại đây. Ví dụ: Hủy đơn hàng.
                //manager update status 
                return BadRequest("Thanh toán thất bại");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        return NotFound("Không tìm thấy thông tin thanh toán.");
    }
    [HttpGet("Callback")]
    // Xử lý khi khách hàng được chuyển hướng về từ VnPay
    // get ra thoong tin thanh toan 
    public ActionResult<string> Callback()
    {
        if (Request.QueryString.HasValue)
        {
            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                var resultDescription = $"{paymentResult.PaymentResponse.Description}.";

                if (paymentResult.IsSuccess)
                {
                    return Ok(resultDescription);
                }

                return BadRequest(resultDescription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        return NotFound("Không tìm thấy thông tin thanh toán.");
    }
}
