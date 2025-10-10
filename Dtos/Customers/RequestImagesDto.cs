using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Customers
{
    public class RequestImagesDto
    {
        
        public Guid RequestImageId { get; set; }
        public Guid ServiceRequestId { get; set; }
        public string ImageUrl { get; set; }
        //public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
