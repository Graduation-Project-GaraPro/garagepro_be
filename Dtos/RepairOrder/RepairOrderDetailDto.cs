using System;
using System.Collections.Generic;

namespace Dtos.RepairOrder
{
    public class RepairOrderDetailDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Guid BranchId { get; set; }
        public string BranchName { get; set; }

        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        public decimal EstimatedAmount { get; set; }
        public decimal Cost { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaidStatus { get; set; }

        public List<JobDetailDto> Jobs { get; set; } = new();
        public List<JobPartDto> Parts { get; set; } = new();
        public List<JobTechnicianDto> Technicians { get; set; } = new();
        public List<PaymentDto> Payments { get; set; } = new();
        public string Note { get; set; }
    }

    public class JobDetailDto
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string ServiceName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class JobPartDto
    {
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class JobTechnicianDto
    {
        public Guid TechnicianId { get; set; }
        public string TechnicianName { get; set; }
    }

    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
