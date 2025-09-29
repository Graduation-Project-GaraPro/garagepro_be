using System.ComponentModel.DataAnnotations;

namespace BusinessObject
{
    public class Color
    {
        [Key]
        public Guid ColorId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)] // For hex color codes like #FF5733
        public string HexCode { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Label> Labels { get; set; } = null!;
    }
}