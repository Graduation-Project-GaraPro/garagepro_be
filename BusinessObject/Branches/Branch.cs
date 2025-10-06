using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace BusinessObject.Branches
{
    public class Branch
    {
        [Key]
        public Guid BranchId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string BranchName { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Street { get; set; }
        [MaxLength(100)]
        public string Ward { get; set; }
        [MaxLength(100)]
        public string District { get; set; }
        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<OperatingHour> OperatingHours { get; set; } = new List<OperatingHour>();

        // Navigation properties
        public virtual ICollection<RepairOrder> RepairOrders { get; set; } = null!;
        public virtual ICollection<Part> Parts { get; set; } = null!;
        public virtual ICollection<ApplicationUser> Staffs { get; set; } = new List<ApplicationUser>();

        // Many-to-many
        public virtual ICollection<BranchService> BranchServices { get; set; } = new List<BranchService>();

    }


}