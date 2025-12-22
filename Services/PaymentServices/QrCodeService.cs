using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BussinessObject;
using Dtos.PayOsDtos;

namespace Services.PaymentServices
{
    public interface IQrCodeService
    {
        Task<string> GeneratePaymentQrCodeAsync(Guid repairOrderId, decimal amount, string description);
    }

    public class QrCodeService : IQrCodeService
    {
        public async Task<string> GeneratePaymentQrCodeAsync(Guid repairOrderId, decimal amount, string description)
        {
            
            var qrData = $"PAYMENT:RO:{repairOrderId}:AMOUNT:{amount}:DESC:{description}";
            
            // Convert to base64 to simulate QR code data
            var qrDataBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(qrData));
            
            return await Task.FromResult(qrDataBase64);
        }
    }
}