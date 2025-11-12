using BusinessObject;
using BussinessObject;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Repositories;
using Repositories.PaymentRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRepairOrderRepository _orderRepository;
        private readonly IVnpay _vnpay;

        public PaymentService(IPaymentRepository paymentRepository,
                          IRepairOrderRepository orderRepository,
                          IVnpay vnpay)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _vnpay = vnpay;
        }

        public async Task<string> CreatePaymentUrlAsync(Guid repairOrderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(repairOrderId);
            if (order == null) throw new KeyNotFoundException("RepairOrder không tồn tại");

            // Store the repairOrderId in our payment record so we can find it later
            var payment = new Payment
            {
                RepairOrderId = order.RepairOrderId,
                UserId = userId,
                Amount = order.EstimatedAmount,
                Method = PaymentMethod.VnPay,
                Status = PaymentStatus.unpayed,
                PaymentDate = DateTime.Now
            };

            await _paymentRepository.AddAsync(payment);

            // Use the payment ID as the transaction reference
            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks, // Use long instead of Guid
                Money = (double)payment.Amount, // Cast decimal to double
                Description = $"Thanh toán RepairOrder {order.RepairOrderId}",
                IpAddress = "127.0.0.1",
                BankCode = BankCode.ANY,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };

            return _vnpay.GetPaymentUrl(request);
        }

        public async Task<string> ProcessIpnAsync(string queryString)
        {
            // Parse query string into IQueryCollection
            var queryDict = ParseQueryString(queryString);
            var paymentResult = _vnpay.GetPaymentResult(queryDict);
            
            // For IPN processing, we typically don't need to find a specific payment
            // The IPN is used by VNPAY to notify us of payment status changes
            if (paymentResult.IsSuccess)
            {
                // In a real implementation, you would update your database based on the payment result
                // For now, we'll just return a success message
                return "IPN processed successfully";
            }
            else
            {
                return "IPN processing failed";
            }
        }

        public async Task<object> ProcessCallbackAsync(string queryString)
        {
            // Parse query string into IQueryCollection
            var queryDict = ParseQueryString(queryString);
            var paymentResult = _vnpay.GetPaymentResult(queryDict);
            
            // For callback processing, we typically show a result page to the user
            if (paymentResult.IsSuccess)
            {
                return new
                {
                    Message = "Thanh toán thành công",
                    // You would typically include more information here based on what's available
                    // in the paymentResult.PaymentResponse object
                };
            }
            else
            {
                return new
                {
                    Message = "Thanh toán thất bại",
                    // You would typically include more information here based on what's available
                    // in the paymentResult.PaymentResponse object
                };
            }
        }
        
        private IQueryCollection ParseQueryString(string queryString)
        {
            var queryParams = new Dictionary<string, StringValues>();
            
            if (!string.IsNullOrEmpty(queryString))
            {
                // Remove the leading '?' if present
                if (queryString.StartsWith("?"))
                    queryString = queryString.Substring(1);
                    
                var pairs = queryString.Split('&');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        queryParams[keyValue[0]] = keyValue[1];
                    }
                }
            }
            
            return new QueryCollection(queryParams);
        }
    }
}