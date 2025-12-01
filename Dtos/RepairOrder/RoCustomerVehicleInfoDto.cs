using System;

namespace Dtos.RepairOrder
{
    public class RoCustomerVehicleInfoDto
    {
        // Customer Information
        public string CustomerId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerFullName => $"{CustomerFirstName} {CustomerLastName}".Trim();
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        // Vehicle Information
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public int? Year { get; set; }
        public long? Odometer { get; set; }
        public string BrandName { get; set; }
        public string ModelName { get; set; }
        public string ColorName { get; set; }

        // Repair Order Basic Info
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string StatusName { get; set; }
    }
}
