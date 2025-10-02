using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Branches;

namespace BusinessObject
{
    public class Part
    {
        [Key]
        public Guid PartId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PartCategoryId { get; set; }

        [Required]
        public Guid BranchId { get; set; }  // v?n gi? 1-n n?u c?n

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual PartCategory PartCategory { get; set; }
        public virtual Branch Branch { get; set; } // 1-n
        public virtual ICollection<PartSpecification> PartSpecifications { get; set; } = new List<PartSpecification>();
        public virtual ICollection<JobPart> JobParts { get; set; } = new List<JobPart>();
        public virtual ICollection<RepairOrderServicePart> RepairOrderServiceParts { get; set; } = new List<RepairOrderServicePart>();
        public virtual ICollection<PartInspection> PartInspections { get; set; } = new List<PartInspection>();

        // Many-to-many with Branch
        public virtual ICollection<BranchPart> BranchParts { get; set; } = new List<BranchPart>();
    }

}