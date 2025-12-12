using System;

namespace Dtos.Quotations
{
    public class AvailableServiceDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAdvanced { get; set; }
        public Guid? ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
    }
}
