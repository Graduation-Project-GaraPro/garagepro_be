using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Revenue
{
    public class TopServiceDto
    {
        public string ServiceName { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public double PercentageOfTotal { get; set; }
    }

}
