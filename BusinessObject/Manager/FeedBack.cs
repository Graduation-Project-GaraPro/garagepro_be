using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Manager
{
    public class FeedBack
    {
        public Guid FeedBackId { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
