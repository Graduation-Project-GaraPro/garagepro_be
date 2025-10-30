﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class RepairOrderProgressDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string RoType { get; set; } = string.Empty;
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal Cost { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaidStatus { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        // Vehicle information
        public VehicleDto Vehicle { get; set; } = new VehicleDto();

        // Order status with labels
        public OrderStatusDto OrderStatus { get; set; } = new OrderStatusDto();

        // All jobs in this repair order
        public List<JobDto> Jobs { get; set; } = new List<JobDto>();

        // Progress calculation
        public decimal ProgressPercentage
        {
            get
            {
                if (Jobs.Count == 0) return 0;

                var completedJobs = Jobs.Count(j => j.Status == "Completed");
                return (decimal)completedJobs / Jobs.Count * 100;
            }
        }

        public string ProgressStatus
        {
            get
            {
                var percentage = ProgressPercentage;
                return percentage switch
                {
                    0 => "Chưa bắt đầu",
                    < 100 => "Đang thực hiện",
                    100 => "Hoàn thành",
                    _ => "Không xác định"
                };
            }
        }
    }
}
