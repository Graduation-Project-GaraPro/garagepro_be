using BusinessObject.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessObject.Manager
{
    public class FeedBack
    {
        [Key]
        public Guid FeedBackId { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(1000)] // optional, depends on your use case
        public string Description { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [Required]
        public Guid RepairOrderId { get; set; }

        [ForeignKey("RepairOrderId")]
        public virtual RepairOrder RepairOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
    
