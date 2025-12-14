using System;

namespace Dtos.RepairOrder
{
    public class RepairOrderNotificationInfoDto
    {
        public Guid RepairOrderId { get; set; }
        public Guid BranchId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleInfo { get; set; } = string.Empty;
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
    }
}