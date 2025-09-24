using BusinessObject.Authentication;
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
        public Guid RepairRequestID { get; set; }

        [Required]
        public Guid VehicleID { get; set; }

        [Required]
        public String UserID { get; set; } // Người tạo yêu cầu sửa chữa

        
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả lỗi / yêu cầu

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser Customer { get; set; }
        public virtual ICollection<RepairImage> RepairImages { get; set; } = new List<RepairImage>();// lưu nhiều ảnh 
        public ICollection<RequestPart> RequestParts { get; set; }
        public ICollection<RequestService> RequestServices { get;set; }
       


        // Optional: các tiến trình sửa chữa
        //public virtual ICollection<RepairTask> RepairTasks { get; set; }
    }
}
