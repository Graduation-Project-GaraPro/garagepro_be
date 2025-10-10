using BusinessObject.Customers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Customers
{
    public class RepairRequestDto
    {
       
            public Guid RepairRequestID { get; set; }
            public Guid VehicleID { get; set; }
            public string UserID { get; set; }
            public Guid BranchId { get; set; }

            public string? Description { get; set; }
            public DateTime RequestDate { get; set; }
            public Status Status { get; set; }

            public decimal? EstimatedCost { get; set; }   // Tổng tiền ước tính
            public List<string>? ImageUrls { get; set; }  // URL ảnh

            // Danh sách service khách chọn
            public List<RequestServiceDto>? RequestServices { get; set; }
        


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