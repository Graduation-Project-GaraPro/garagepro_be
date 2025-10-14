using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class ServiceCategory
    {
        [Key]
        public Guid ServiceCategoryId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }
       
        // Parent-child relationship for hierarchical categories
        public Guid? ParentServiceCategoryId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Service> Services { get; set; }
        
        // Parent-child navigation properties for hierarchical categories
        public virtual ServiceCategory ParentServiceCategory { get; set; }
        public virtual ICollection<ServiceCategory> ChildServiceCategories { get; set; }
    }
}