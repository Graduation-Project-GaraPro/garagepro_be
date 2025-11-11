using System;

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
        //public string CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid BranchId { get; set; } // gara mà khách chọn
        public string IssueDescription { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public EmergencyType Type { get; set; }

        // Danh sách hình ảnh (optional)
       // public List<EmergencyMediaDto>? MediaFiles { get; set; }
    }

    // DTO cho ảnh/video minh chứng
    public class EmergencyMediaDto
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
}
