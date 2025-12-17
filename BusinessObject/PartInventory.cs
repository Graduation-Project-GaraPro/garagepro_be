using BusinessObject.Branches;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    public class PartInventory
    {
        [Key]
        public Guid PartInventoryId { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(Part))]
        public Guid PartId { get; set; }

        [Required]
        [ForeignKey(nameof(Branch))]
        public Guid BranchId { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Part Part { get; set; }
        public virtual Branch Branch { get; set; }
    }
}