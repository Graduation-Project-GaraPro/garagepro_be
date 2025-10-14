using BusinessObject.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessObject.Manager
{
    public class FeedBack
    {
        public Guid FeedBackId { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public Guid RepairOrderId { get; set; }
        public virtual RepairOrder RepairOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
