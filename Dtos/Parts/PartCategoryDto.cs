using System;

namespace Dtos.Parts
{
    public class PartCategoryDto
    {
        public Guid LaborCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Vehicle Model Information
        public Guid ModelId { get; set; }
        public string ModelName { get; set; }
        public string BrandName { get; set; }
        public Guid BrandId { get; set; }
    }

    public class CreatePartCategoryDto
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }

    public class UpdatePartCategoryDto
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}