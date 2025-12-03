using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Emergency
{
    public class EmergencyResponeDto
    {
        
            public Guid EmergencyRequestId { get; set; }       // ID của Emergency
            public string VehicleName { get; set; }           // Tên xe hoặc VehicleId
            public string IssueDescription { get; set; }     // Mô tả sự cố
            public string EmergencyType { get; set; }        // OnSite hoặc TowToGarage
            public DateTime RequestTime { get; set; }        // Thời gian tạo Emergency
            public string Status { get; set; }               // Pending, Accepted, InProgress, Completed

            // Vị trí cứu hộ
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string? Address { get; set; }
            public string? MapUrl { get; set; }
            public DateTime? ResponseDeadline { get; set; }
            public DateTime? RespondedAt { get; set; }
            public DateTime? AutoCanceledAt { get; set; }
            public double? DistanceToGarageKm { get; set; }
            public int? EstimatedArrivalMinutes { get; set; }

            // Thông tin khách hàng
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }

            // Tùy chọn thêm nếu muốn hiển thị tiến độ
            public string? AssignedTechnicianName { get; set; }
            public string? AssginedTecinicianPhone { get; set; }
            public decimal? EmergencyFee { get; set; }
        }

    }
