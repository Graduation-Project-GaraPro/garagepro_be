using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public class UserFilterDto
    {
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
    }

}
