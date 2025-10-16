using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class PartServiceRelatedDto
    {
        
        public Guid PartId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PartCategoryId { get; set; }


        public Guid? BranchId { get; set; }  // v?n gi? 1-n n?u c?n

        [Required, MaxLength(100)]
        public string Name { get; set; }

       
        public decimal Price { get; set; }

        public int Stock { get; set; }

      
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
