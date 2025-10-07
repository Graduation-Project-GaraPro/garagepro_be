using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class CreateServiceDto
    {
        [Required]
        public Guid ServiceCategoryId { get; set; }

        [Required, MaxLength(100)]
        public string ServiceName { get; set; }

        [Required, MaxLength(50)]
        public string ServiceStatus { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public decimal Price { get; set; }
        public decimal EstimatedDuration { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAdvanced { get; set; } = false;

        public List<Guid> BranchIds { get; set; } = new();
        public List<Guid> PartIds { get; set; } = new();

    }
}
