using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.FeedBacks
{
    public class FeedBackCreateDto
    {
       // public string UserId { get; set; }
        public string Description { get; set; }
        public int? Rating { get; set; }
        public Guid RepairOrderId { get; set; }
    }

}
