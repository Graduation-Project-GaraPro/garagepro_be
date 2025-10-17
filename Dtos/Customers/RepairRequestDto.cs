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
            public RepairRequestStatus Status { get; set; }

            public decimal? EstimatedCost { get; set; }   // Tổng tiền ước tính
            public List<string>? ImageUrls { get; set; }  // URL ảnh

            // Danh sách service khách chọn
            public List<RequestServiceDto>? RequestServices { get; set; }
        


    }



    public class UpdateRepairRequestDto
    {     
            [StringLength(500, ErrorMessage = "Description max 500 chars")]
            public string? Description { get; set; }

            public DateTime? RequestDate { get; set; }

            // Cho phép cập nhật lại hình ảnh nếu cần
            public List<string>? ImageUrls { get; set; }

        // Cho phép đổi dịch vụ và phụ tùng
        public List<RequestServiceDto>? Services { get; set; }
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