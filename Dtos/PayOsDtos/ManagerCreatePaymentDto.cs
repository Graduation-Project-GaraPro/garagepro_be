using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessObject;

namespace Dtos.PayOsDtos
{
    public class ManagerCreatePaymentDto
    {
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash; // Default to Cash
        public string? Description { get; set; }
    }
}