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
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash; // Default to Cash
    }
}