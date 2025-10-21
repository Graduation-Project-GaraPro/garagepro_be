//using BusinessObject;
//using BussinessObject;
//using Repositories;
//using Repositories.PaymentRepositories;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VNPAY.NET;
//using VNPAY.NET.Enums;
//using VNPAY.NET.Models;

//namespace Services.PaymentServices
//{
//    public class PaymentService: IPaymentService
//    {
//        private readonly IPaymentRepository _paymentRepository;
//        private readonly IRepairOrderRepository _orderRepository;
//        private readonly IVnpay _vnpay;

//        public PaymentService(IPaymentRepository paymentRepository,
//                          IRepairOrderRepository orderRepository,
//                          IVnpay vnpay)
//        {
//            _paymentRepository = paymentRepository;
//            _orderRepository = orderRepository;
//            _vnpay = vnpay;
//        }

//        public async Task<string> CreatePaymentUrlAsync(Guid repairOrderId, string userId)
//        {
//            var order = await _orderRepository.GetByIdAsync(repairOrderId);
//            if (order == null) throw new KeyNotFoundException("RepairOrder không tồn tại");

//            var payment = new Payment
//            {
//                RepairOrderId = order.RepairOrderId,
//                UserId = userId,
//                Amount = order.EstimatedAmount,
//                Method = PaymentMethod.VnPay,
//                Status = PaymentStatus.unpayed,
//                PaymentDate = DateTime.Now
//            };

//            await _paymentRepository.AddAsync(payment);

//            var request = new PaymentRequest
//            {
//                PaymentId = payment.PaymentId,
//                Money = payment.Amount,
//                Description = $"Thanh toán RepairOrder {order.RepairOrderId}",
//                IpAddress = "127.0.0.1",
//                BankCode = BankCode.ANY,
//                CreatedDate = DateTime.Now,
//                Currency = Currency.VND,
//                Language = DisplayLanguage.Vietnamese
//            };

//            return _vnpay.GetPaymentUrl(request);
//        }

//        public async Task<string> ProcessIpnAsync(string queryString)
//        {
//            var paymentResult = _vnpay.GetPaymentResult(queryString);
//            var payment = await _paymentRepository.GetByIdAsync(paymentResult.PaymentResponse.PaymentId);
//            if (payment == null) throw new KeyNotFoundException("Payment không tồn tại");

//            if (paymentResult.IsSuccess)
//            {
//                payment.Status = PaymentStatus.Paid;
//                payment.UpdatedAt = DateTime.Now;
//                await _paymentRepository.UpdateAsync(payment);

//                var order = await _orderRepository.GetByIdAsync(payment.RepairOrderId);
//                order.Status = OrderStatus.Paid;
//                await _orderRepository.UpdateAsync(order);

//                return "Thanh toán thành công";
//            }
//            else
//            {
//                payment.Status = PaymentStatus.Unpaid;
//                await _paymentRepository.UpdateAsync(payment);
//                return "Thanh toán thất bại";
//            }
//        }

//        public async Task<object> ProcessCallbackAsync(string queryString)
//        {
//            var paymentResult = _vnpay.GetPaymentResult(queryString);
//            var payment = await _paymentRepository.GetByIdAsync(paymentResult.PaymentResponse.PaymentId);

//            if (paymentResult.IsSuccess)
//            {
//                return new
//                {
//                    Message = "Thanh toán thành công",
//                    PaymentId = payment.PaymentId,
//                    OrderId = payment.RepairOrderId,
//                    Amount = payment.Amount
//                };
//            }
//            else
//            {
//                return new
//                {
//                    Message = "Thanh toán thất bại",
//                    PaymentId = payment.PaymentId,
//                    OrderId = payment.RepairOrderId,
//                    Amount = payment.Amount
//                };
//            }
//        }
//    }
//}
