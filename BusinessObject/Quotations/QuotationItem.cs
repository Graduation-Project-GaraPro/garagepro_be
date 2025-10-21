using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Quotations
{
    public class QuotationItem
    {
        [Key]
        public Guid QuotationItemID { get; set; }

        [Required]
        public Guid QuotationID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ItemType { get; set; } // Part, Service

        [Required]
        [MaxLength(255)]
        public string ItemName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // References
        public Guid? PartID { get; set; }

        public Guid? ServiceID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("QuotationID")]
        public virtual Quotation Quotation { get; set; }

        [ForeignKey("PartID")]
        public virtual Part Part { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }
    }
}
