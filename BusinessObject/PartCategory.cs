using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class PartCategory
    {
        [Key]
        public Guid LaborCategoryId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Part> Parts { get; set; }
    }
}