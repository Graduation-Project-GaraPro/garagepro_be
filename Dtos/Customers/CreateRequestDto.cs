using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public class CreateRequestDto
    {
        public Guid RequestID { get; set; }
        public Guid VehicleID { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public List<IFormFile> Images { get; set; } // file upload
        public List<RequestServiceDto> Services { get; set; }
        public List<RequestPartDto> Parts { get; set; }
    }
}
