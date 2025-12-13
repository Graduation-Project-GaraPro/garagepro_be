using System;

namespace Dtos.Parts
{
    /// <summary>
    /// DTO for editing parts - excludes branch information to prevent branch changes
    /// </summary>
    public class EditPartDto
    {
        public Guid PartId { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}