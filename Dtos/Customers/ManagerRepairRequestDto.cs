using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Customers
{
    public class ManagerRepairRequestDto
    {
        public Guid RequestID { get; set; }
        public Guid VehicleID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string VehicleInfo { get; set; } // Brand + Model
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<RequestServiceDto> Services { get; set; }
        public List<RequestPartDto> Parts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}