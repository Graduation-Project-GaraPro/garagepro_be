using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.RequestEmergency.RequestEmergency;

namespace Dtos.EmergencyTechnicians
{
    public class EmergencyForTechnicianDto
    {
        
        public Guid EmergencyRequestId { get; set; }

        
        public string CustomerId { get; set; }  // Id người gửi yêu cầu
        
        public Guid BranchId { get; set; }     // Gara tiếp nhận (có thể null nếu sửa tại chỗ)

        
        public Guid VehicleId { get; set; }      // Liên kết tới xe đã đăng ký trong hệ thống

        
        public string IssueDescription { get; set; } = string.Empty;

        public string BranchName { get; set; }

        public double BranchLatitude { get; set; }
        public double BranchLongitude { get; set; }


        [Required]
        public double Latitude { get; set; }    // Tọa độ khách hàng

        [Required]
        public double Longitude { get; set; }
      
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;                 
        public EmergencyStatus Status { get; set; } = EmergencyStatus.Pending;

        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
