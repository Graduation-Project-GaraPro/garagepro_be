using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class ServicePartCategory
    {
        [Key]
        public Guid ServicePartCategoryId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(Service))]
        public Guid ServiceId { get; set; }

        [Required]
        [ForeignKey(nameof(PartCategory))]
        public Guid PartCategoryId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Service Service { get; set; }
        public virtual PartCategory PartCategory { get; set; }
    }
}
