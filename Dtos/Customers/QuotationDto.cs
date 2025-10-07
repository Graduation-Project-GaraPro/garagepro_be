using System;
using System.Collections.Generic;

namespace Customers
{
    // DTO hiển thị báo giá
    public class QuotationDto
    {
        public Guid QuotationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerConcern { get; set; }
        public string? Finding { get; set; }

        // Danh sách dịch vụ
        public List<ServiceItemDto> Services { get; set; } = new List<ServiceItemDto>();

        // Danh sách linh kiện / Part
        public List<PartItemDto> Parts { get; set; } = new List<PartItemDto>();
    }

    // DTO dịch vụ
    public class ServiceItemDto
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }

    // DTO  Part
    public class PartItemDto
    {
        public Guid PartId { get; set; }              // ID Part gốc
        public string PartName { get; set; }          // Tên Part
        public decimal Price { get; set; }            // Giá Part
        public int Quantity { get; set; }             // Quantity cố định not change
        public decimal Total => Price * Quantity;

        // Danh sách Specification 
        public List<PartSpecificationDto> Specifications { get; set; } = new List<PartSpecificationDto>();

        // Nếu Customer có thể chọn Spec, lưu Spec đã chọn
        public Guid? SelectedSpecId { get; set; }//lấy từ bảng PartSpecification
    }

    // DTO Specification
    public class PartSpecificationDto
    {
        public Guid SpecId { get; set; }
        public string SpecValue { get; set; } // ví dụ: "Dầu loại tốt"
    }

    // DTO Customer gửi lựa chọn Spec cho Part
    public class UpdateQuotationPartsDto
    {
        public Guid QuotationId { get; set; } // ID báo giá
        public List<UpdatePartItemDto> Parts { get; set; } = new List<UpdatePartItemDto>();
    }

    public class UpdatePartItemDto
    {
        public Guid PartId { get; set; }          // ID Part gốc
        public Guid? SelectedSpecId { get; set; } // Customer chọn Spec nếu có
    }
}
