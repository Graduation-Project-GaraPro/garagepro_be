using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Dtos.Emergency
{
    // Dùng cho khách hàng gửi tọa độ để tìm gara gần nhất và trả về danh sách gara gần nhất
    public class NearbyBranchRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    // Dùng để trả về thông tin gara gần nhất (danh sách hoặc 1 gara)
    public class BranchNearbyResponseDto
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }

        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public double DistanceKm { get; set; }
    }

    // Enum cho loại cứu hộ
    public enum EmergencyType
    {
        OnSiteRepair,
        TowToGarage
    }

    // Dùng khi khách hàng gửi yêu cầu cứu hộ
    public class CreateEmergencyRequestDto
    {
        [Required]
        public Guid VehicleId { get; set; }
        public Guid BranchId { get; set; }
        [Required]
        [StringLength(500)]
        public string IssueDescription { get; set; }
        [Range(-90, 90)]
        public double Latitude { get; set; }
        [Range(-180, 180)]
        public double Longitude { get; set; }
        public EmergencyType Type { get; set; }// mac dinh garra

        // Danh sách hình ảnh (optional)
       // public List<EmergencyMediaDto>? MediaFiles { get; set; }
    }

    // DTO cho ảnh/video minh chứng
    public class EmergencyMediaDto
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
    public class RouteDto
    {
        public double DistanceKm { get; set; }
        public int DurationMinutes { get; set; }
        public JsonElement Geometry { get; set; }
    }


    public class TechnicianLocationDto
    {
        public Guid EmergencyRequestId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? SpeedKmh { get; set; }
        public double? Bearing { get; set; }
        public bool RecomputeRoute { get; set; } = true;
    }
}
