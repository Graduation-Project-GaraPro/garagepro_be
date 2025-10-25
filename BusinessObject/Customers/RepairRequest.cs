using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Customers
{
    public class RepairRequest
    {
        [Key]
        public Guid RepairRequestID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid VehicleID { get; set; }

        [Required]
        public String UserID { get; set; } // Người tạo yêu cầu sửa chữa


        [StringLength(500)]
        public string? Description { get; set; } // Mô tả lỗi / yêu cầu
        [Required]
        public Guid BranchId { get; set; }// Chi nhánh xử lý yêu cầu

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;// ngày yêu cầu đem xe đến để sửa chữa 

        public DateTime? CompletedDate { get; set; }

        public RepairRequestStatus Status { get; set; } = RepairRequestStatus.Pending;  // Pending, Accept, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public decimal? EstimatedCost { get; set; } // Chi phí ước tính

        // Navigation properties
        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser Customer { get; set; }
        public virtual ICollection<RepairImage>? RepairImages { get; set; } = new List<RepairImage>();// lưu nhiều ảnh 
        //public ICollection<RequestPart>? RequestParts { get; set; }
        public ICollection<RequestService>? RequestServices { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

        // Một yêu cầu sửa chữa có thể dẫn đến một đơn sửa chữa
        //public virtual RepairOrder RepairOrders { get; set; }
        public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();

        // Optional: các tiến trình sửa chữa
        //public virtual ICollection<RepairTask> RepairTasks { get; set; }
    }
    public enum RepairRequestStatus
    {
        Pending, Accept, Arrived, Cancelled
    }
}
