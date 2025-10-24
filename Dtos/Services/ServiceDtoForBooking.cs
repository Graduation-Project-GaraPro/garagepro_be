using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class ServiceDtoForBooking
    {
        public Guid ServiceId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; } // Giá sau ưu đãi
        public decimal EstimatedDuration { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdvanced { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual GetCategoryForServiceDto ServiceCategory { get; set; }
        public virtual ICollection<PartCategoryForBooking>? PartCategories { get; set; }
            = new List<PartCategoryForBooking>();
    }
}
