using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class UpdateServiceDto
    {
        [Required(ErrorMessage = "ServiceId is required for update")]
        public Guid ServiceId { get; set; }

        [Required(ErrorMessage = "ServiceCategoryId is required")]
        public Guid ServiceCategoryId { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [MaxLength(100, ErrorMessage = "Service name cannot exceed 100 characters"), MinLength(3)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters"), MinLength(10)]
        public string Description { get; set; }

        [Range(1000, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 1000")]
        public decimal Price { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Estimated duration must be greater than or equal to 1")]
        public decimal EstimatedDuration { get; set; }

        public bool IsActive { get; set; }
        public bool IsAdvanced { get; set; }

        [MinLength(1, ErrorMessage = "At least one branch must be assigned")]
        public List<Guid> BranchIds { get; set; } = new();

        // ✅ đổi từ Ids sang Names
        [MaxLength(50, ErrorMessage = "A service cannot have more than 50 parts")]
        public List<string> PartCategoryNames { get; set; } = new();
    }

}
