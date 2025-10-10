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
        // public Guid RequestID { get; set; }
        [Required]
        public Guid BranchId { get; set; }
        [Required]
        public Guid VehicleID { get; set; }
        [Required, StringLength(500, ErrorMessage = "Description max 500 chars")]
        public string? Description { get; set; }
        [Required]
        public DateTime RequestDate { get; set; }
      //  public string Status { get; set; }
        public List<string>? ImageUrls { get; set; } = new List<string>();// URL ảnh hoặc Base64 string
        public List<RequestServiceDto>? Services { get; set; } = new List<RequestServiceDto>();
       
    }
}
