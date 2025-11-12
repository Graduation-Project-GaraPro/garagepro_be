using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Customers
{
    public class RequestLimitsConfig
    {
        public int MaxActiveRequestsPerUser { get; set; }
        public int MaxRequestsPerVehiclePerDay { get; set; }
        public int CreateCooldownMinutes { get; set; }

        public TimeSpan CreateCooldown => TimeSpan.FromMinutes(CreateCooldownMinutes);
    }
}
