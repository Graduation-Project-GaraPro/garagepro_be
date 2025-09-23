using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Label
    {
        [Key]
        public Guid LabelId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(OrderStatus))]
        public Guid OrderStatusId { get; set; }

        [Required]
        [MaxLength(100)]
        public string LabelName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
        // Navigation properties
        public virtual OrderStatus OrderStatus { get; set; } = null!;
    }
}