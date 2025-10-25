using System.ComponentModel.DataAnnotations;

namespace Dtos.Parts
{
    public class UpdatePartDto
    {
        [Required(ErrorMessage = "PartCategoryId is required")]
        public Guid PartCategoryId { get; set; }

        public Guid? BranchId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; } = 0;
    }
}