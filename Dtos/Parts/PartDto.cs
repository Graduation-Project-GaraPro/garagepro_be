using System;

namespace Dtos.Parts
{
    public class PartDto
    {
        public Guid PartId { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; }
        public Guid? BranchId { get; set; }
        public string BranchName { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; } //  from PartInventory
        
        // Vehicle Model Information
        public Guid ModelId { get; set; }
        public string ModelName { get; set; }
        public string BrandName { get; set; }
        public Guid BrandId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}