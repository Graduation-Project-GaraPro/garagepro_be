using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObject
{
   public class PaymentInformationModel
    {
        public string OrderType { get; set; } = "other";
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; } = "";
        public string Name { get; set; } = "";

    }
}
