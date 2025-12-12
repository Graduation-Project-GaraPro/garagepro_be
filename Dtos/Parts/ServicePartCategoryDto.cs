using System;
using System.Collections.Generic;

namespace Dtos.Parts
{
    public class ServicePartCategoryDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public List<PartCategoryDto> PartCategories { get; set; } = new();
    }

    public class PartCategoryWithServicesDto
    {
        public Guid LaborCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public List<ServiceBasicDto> Services { get; set; } = new();
        public int PartsCount { get; set; }
    }

    public class ServiceBasicDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategoryName { get; set; }
    }
}