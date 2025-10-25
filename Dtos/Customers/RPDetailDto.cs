using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Customers;
using Dtos.Vehicles;

namespace Dtos.Customers
{
    public class RPDetailDto
    {
        public Guid RepairRequestID { get; set; }
        public Guid VehicleID { get; set; }
        public string UserID { get; set; }
        public Guid BranchId { get; set; }

        public string? Description { get; set; }
        public DateTime RequestDate { get; set; }
        public RepairRequestStatus Status { get; set; }

        public decimal? EstimatedCost { get; set; }   // Tổng tiền ước tính
        public List<string>? ImageUrls { get; set; }  // URL ảnh
        public virtual VehicleDto Vehicle { get; set; }

        // Danh sách service khách chọn
        public List<RPServiceDetail>? RequestServices { get; set; }
    }
}
