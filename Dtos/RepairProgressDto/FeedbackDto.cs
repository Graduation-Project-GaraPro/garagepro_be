using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class FeedbackDto
    {
        public string Description { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public DateTime CreatedAt { get; set; }
     
    }
}
