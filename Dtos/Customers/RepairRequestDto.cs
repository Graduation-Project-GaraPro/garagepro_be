using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Customers
{
    public class RepairRequestDto
    {
        public Guid RequestID { get; set; }
        public Guid VehicleID { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<RequestServiceDto> Services { get; set; }
        public List<RequestPartDto> Parts { get; set; }
    }

   

    public class UpdateRepairRequestDto
    {
        [Required]
        public string Description { get; set; }
        
        [Required]
        public string Status { get; set; }
        
        public List<Guid> ServiceIDs { get; set; }
        public List<RequestPartInputDto> Parts { get; set; }
    }

 

  

    public class RequestPartInputDto
    {
        [Required]
        public Guid PartID { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}