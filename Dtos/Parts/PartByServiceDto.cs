using System;

namespace Dtos.Parts
{
    public class PartByServiceDto
    {
        public Guid PartId { get; set; }
        public Guid PartCategoryId { get; set; }
        public Guid? BranchId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}