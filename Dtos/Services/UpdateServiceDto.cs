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
        [MaxLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Estimated duration must be at least 1 minute")]
        public decimal EstimatedDuration { get; set; }

        public bool IsActive { get; set; }
        public bool IsAdvanced { get; set; }

        [MinLength(1, ErrorMessage = "At least one branch must be assigned")]
        public List<Guid> BranchIds { get; set; } = new();

        [MaxLength(50, ErrorMessage = "A service cannot have more than 50 parts")]
        public List<Guid> PartIds { get; set; } = new();
    }
}
