using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dtos.Customers
{
    public class CreateRepairRequestWithImageDto
    {
        public Guid VehicleID { get; set; }
        public Guid BranchId { get; set; }
        public string? Description { get; set; }
        public DateTime RequestDate { get; set; }


        public List<RequestServiceDto>? Services { get; set; } = new List<RequestServiceDto>();

        public List<IFormFile>? Images { get; set; }  // ảnh gửi từ Android
    }
}
