using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    public class InspectionType
    {
        [Key]
        public int InspectionTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; } // basic/advanced

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InspectionFee { get; set; } // charged

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
