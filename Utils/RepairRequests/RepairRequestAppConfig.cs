using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Customers;
using Microsoft.Extensions.Configuration;

namespace Utils.RepairRequests
{
    public class RepairRequestAppConfig
    {
        private static IConfiguration _configuration;
        private static RequestLimitsConfig _limits;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            _limits = _configuration.GetSection("RequestLimits").Get<RequestLimitsConfig>();
        }

        public static int MaxActiveRequestsPerUser => _limits?.MaxActiveRequestsPerUser ?? 3;
        public static int MaxRequestsPerVehiclePerDay => _limits?.MaxRequestsPerVehiclePerDay ?? 2;
        public static TimeSpan CreateCooldown => TimeSpan.FromMinutes(_limits?.CreateCooldownMinutes ?? 2);
    }
}
