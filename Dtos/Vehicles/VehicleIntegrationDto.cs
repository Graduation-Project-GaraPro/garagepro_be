using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;
using Dtos.RoBoard;

namespace Dtos.Vehicles
{
    public class VehicleWithHistoryDto
    {
        public VehicleDto Vehicle { get; set; }
        public RoBoardCustomerDto Customer { get; set; }
        public List<RepairOrderSummaryDto> ServiceHistory { get; set; } = new List<RepairOrderSummaryDto>();
    }

    public class VehicleSchedulingDto
    {
        public Guid VehicleId { get; set; }

        [StringLength(50)]
        public string LicensePlate { get; set; }

        [StringLength(17)]
        public string VIN { get; set; }

        public int Year { get; set; }

        [StringLength(100)]
        public string MakeModel { get; set; }

        public DateTime? NextServiceDate { get; set; }

        public DateTime LastServiceDate { get; set; }

        public long? Odometer { get; set; }

        public List<RepairOrderSummaryDto> UpcomingAppointments { get; set; } = new List<RepairOrderSummaryDto>();
    }

    public class VehicleInsuranceDto
    {
        public Guid VehicleId { get; set; }

        [StringLength(50)]
        public string LicensePlate { get; set; }

        [StringLength(17)]
        public string VIN { get; set; }

        [StringLength(50)]
        public string InsuranceStatus { get; set; }

        public DateTime? InsuranceExpiryDate { get; set; }

        [StringLength(100)]
        public string InsuranceProvider { get; set; }

        [StringLength(50)]
        public string PolicyNumber { get; set; }
    }

    public class RepairOrderSummaryDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }

        [StringLength(50)]
        public string RepairOrderType { get; set; }

        [StringLength(100)]
        public string StatusName { get; set; }

        [StringLength(100)]
        public string BranchName { get; set; }

        [StringLength(100)]
        public string CustomerName { get; set; }

        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }

        [StringLength(50)]
        public string PaidStatus { get; set; }
    }

    public class VehicleRoHistoryDto
    {
        public Guid RepairOrderId { get; set; }
        
        public DateTime ReceiveDate { get; set; }
        
        public string RepairOrderType { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        [StringLength(100)]
        public string BranchName { get; set; }

        [StringLength(100)]
        public string CustomerName { get; set; }

        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public PaidStatus PaidStatus { get; set; }
    }

    // Add the missing VehicleWithCustomerDto class
    public class VehicleWithCustomerDto
    {
        public VehicleDto Vehicle { get; set; }
        public RoBoardCustomerDto Customer { get; set; }
    }
}