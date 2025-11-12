using BusinessObject.Authentication;
using BusinessObject.Branches;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.RequestEmergency
{
    public class RequestEmergency
    {
        [Key]
        public Guid EmergencyRequestId { get; set; }

        [Required]
        public string CustomerId { get; set; }  // Id người gửi yêu cầu
        [Required]
        public Guid BranchId { get; set; }     // Gara tiếp nhận (có thể null nếu sửa tại chỗ)

        [Required]
        public Guid VehicleId { get; set; }      // Liên kết tới xe đã đăng ký trong hệ thống

        [Required]
        public string IssueDescription { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }    // Tọa độ khách hàng

        [Required]
        public double Longitude { get; set; }

        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        public enum EmergencyStatus
        {
            Pending,    // Chờ gara xác nhận
            Accepted,   // Gara đã tiếp nhận
            Completed,  // Hoàn thành
            Canceled    // Khách hủy
        }

        public enum EmergencyType
        {
            OnSiteRepair, // Sửa tại chỗ
            TowToGarage   // Kéo xe về gara
        }

        [Required]
        public EmergencyType Type { get; set; } = EmergencyType.OnSiteRepair;

        [Required]
        public EmergencyStatus Status { get; set; } = EmergencyStatus.Pending;

        //  Ảnh 
        public virtual ICollection<EmergencyMedia>? MediaFiles { get; set; } = new List<EmergencyMedia>();

        // 🔹 Navigation Properties
        public ApplicationUser Customer { get; set; }
        public Branch Branch { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
