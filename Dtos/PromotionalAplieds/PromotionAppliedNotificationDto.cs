using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.PromotionalAplieds
{
    public class PromotionAppliedNotificationDto
    {
        public Guid QuotationId { get; set; }
        public string UserId { get; set; } = string.Empty;

        public List<PromotionAppliedServiceDto> Services { get; set; } = new();
    }

    public class PromotionAppliedServiceDto
    {
        public Guid QuotationServiceId { get; set; }
        public Guid? AppliedPromotionId { get; set; }
        public string? PromotionName { get; set; }
    }
}
