using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairOrderArchivedDtos
{
    public class RepairOrderArchivedListItemDto
    {
        public Guid RepairOrderId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? ArchivedAt { get; set; }

        // Chỉ lấy mấy field bạn yêu cầu
        public string LicensePlate { get; set; }
        public string BranchName { get; set; }
        public string BrandName { get; set; }
        public string ModelName { get; set; }

        // Thêm vài field hay dùng (tùy bạn dùng hay không)
        public decimal Cost { get; set; }
        public decimal PaidAmount { get; set; }
        public int StatusId { get; set; }
    }
}
