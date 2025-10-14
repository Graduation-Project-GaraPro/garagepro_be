using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairHistory
{
    //public class RepairHistoryDto
    //{
    //    public Guid VehicleId { get; set; }
    //    public Guid ModelId { get; set; }
    //    public string LicensePlate { get; set; }
    //    public string VIN { get; set; }

    //    public OwnerDto Owner { get; set; }

    //    // số lần sửa (count of RepairOrders that technician has access to on this vehicle)
    //    public int RepairCount { get; set; }

    //    public List<RepairOrderDto> RepairOrders { get; set; } = new();
    //}

    //public class OwnerDto
    //{
    //    public string FullName { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public string Email { get; set; }
    //}

    //public class RepairOrderDto
    //{
    //    public Guid RepairOrderId { get; set; }
    //    public DateTime ReceiveDate { get; set; }
    //    public string Note { get; set; } // vấn đề khách hàng (RepairOrder.Note)
    //    public List<ServiceDto> Services { get; set; } = new();
    //    public List<JobDto> Jobs { get; set; } = new();
    //}

    //public class ServiceDto
    //{
    //    public Guid ServiceId { get; set; }
    //    public string ServiceName { get; set; }
    //}

    //public class JobDto
    //{
    //    public Guid JobId { get; set; }
    //    public string JobName { get; set; }
    //    public string Status { get; set; }
    //    public decimal TotalAmount { get; set; }
    //    public string Note { get; set; }
    //    public DateTime? Deadline { get; set; }
    //    public int Level { get; set; }

    //    public List<JobPartDto> Parts { get; set; } = new();
    //}

    //public class JobPartDto
    //{
    //    public Guid PartId { get; set; }
    //    public string PartName { get; set; }
    //    public int Quantity { get; set; }
    //    public decimal UnitPrice { get; set; }
    //}
    public class RepairHistoryDto
    {
        public string VehicleModelId { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public string OwnerFullName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public int RepairCount { get; set; } // Số lần xe được sửa chữa
        public List<JobHistoryDto> CompletedJobs { get; set; } = new();
    }

    public class JobHistoryDto
    {
        public string JobName { get; set; }
        public string Note { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? Deadline { get; set; }
        public int Level { get; set; }
        public string CustomerIssue { get; set; } // Notes từ RepairOrder
        public List<JobPartDto> JobParts { get; set; } = new();
        public List<ServiceDto> Services { get; set; } = new();
    }

    public class JobPartDto
    {
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ServiceDto
    {
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ActualDuration { get; set; }
        public string Notes { get; set; }
    }

}
