using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Statistical
{


    public class TechnicianStatisticDto
    {
        public float Quality { get; set; }
        public float Speed { get; set; }
        public float Efficiency { get; set; }
        public float Score { get; set; }

        public int NewJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int OnHoldJobs { get; set; }

        public List<RecentJobDto> RecentJobs { get; set; } = new();
    }

    public class RecentJobDto
    {
        public string JobName { get; set; }
        public string LicensePlate { get; set; }
        public string Status { get; set; }
        public DateTime AssignedAt { get; set; }
    }
    //public class TechnicianStatisticsDTO
    //{
    //    public Guid TechnicianId { get; set; }
    //    public string TechnicianName { get; set; }
    //    public string Email { get; set; }
    //    public string PhoneNumber { get; set; }

    //    // Điểm số
    //    public float Quality { get; set; }
    //    public float Speed { get; set; }
    //    public float Efficiency { get; set; }
    //    public float Score { get; set; }

    //    // Số lượng Job theo trạng thái
    //    public JobStatusCountDTO JobStatusCount { get; set; }

    //    // 3 Job gần nhất
    //    public List<RecentJobDTO> RecentJobs { get; set; }
    //}

    //public class JobStatusCountDTO
    //{
    //    public int New { get; set; }
    //    public int InProgress { get; set; }
    //    public int Completed { get; set; }
    //    public int OnHold { get; set; }
    //    public int Total { get; set; }
    //}

    //public class RecentJobDTO
    //{
    //    public Guid JobId { get; set; }
    //    public string JobName { get; set; }
    //    //public string VehicleName { get; set; }  // Tên xe (Brand + Model)
    //    public string LicensePlate { get; set; }
    //    public string JobStatus { get; set; }  // New, InProgress, Completed, OnHold
    //    public DateTime AssignedTime { get; set; }  // Thời gian từ JobTechnician
    //    public DateTime? Deadline { get; set; }
    //}
}
