using System;
using System.Collections.Generic;

namespace Customers
{
    // DTO hiển thị báo giá
    public class QuotationDto
    {
        public Guid QuotationID { get; set; }
        public Guid RepairRequestID { get; set; }
        public string? BranchName { get; set; }
        public string? Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<QuotationItemDto> Items { get; set; } = new();
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
    public class QuotationItemDto
    {
        public Guid QuotationItemID { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
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
