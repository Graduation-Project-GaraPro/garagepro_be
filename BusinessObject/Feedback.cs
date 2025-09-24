using BusinessObject.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Feedback
    {
            [Key]
            public Guid FeedbackId { get; set; } = Guid.NewGuid();

            [Required]
            public String UserId { get; set; }

            [Required]
            public Guid RepairOrderId { get; set; }

            [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5")]
            public int Star { get; set; }

            [MaxLength(1000)]
            public string? Description { get; set; }

            [Required]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? UpdatedAt { get; set; }

            // Navigation
            [ForeignKey("UserId")]
            public virtual ApplicationUser User { get; set; }

            [ForeignKey("RepairOrderId")]
            public virtual RepairOrder RepairOrder { get; set; }
        }
    }
