using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.FeedBacks
{
    public class FeedBackReadDto
    {
        public Guid FeedBackId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // optional
        public string Description { get; set; }
        public int? Rating { get; set; }
        public Guid RepairOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
