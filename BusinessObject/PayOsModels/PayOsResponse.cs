using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
    public class PayOsResponse<T>
    {
        public string code { get; set; } = default!;
        public string desc { get; set; } = default!;
        public T data { get; set; } = default!;
    }
}
