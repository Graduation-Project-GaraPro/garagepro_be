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