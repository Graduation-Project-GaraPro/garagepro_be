using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Branches
{
    public class BranchCreateDto
    {
        [Required(ErrorMessage = "Branch name is required")]
        [MinLength(2)]
        [MaxLength(200, ErrorMessage = "Branch name cannot exceed 200 characters")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street is required")]
        [MaxLength(200, ErrorMessage = "Street cannot exceed 200 characters")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ward is required")]
        [MaxLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
        public string Ward { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required")]
        [MaxLength(100, ErrorMessage = "District cannot exceed 100 characters")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [MinLength(1, ErrorMessage = "At least one service must be assigned to the branch")]
        public List<Guid> ServiceIds { get; set; } = new();

        [MinLength(1, ErrorMessage = "At least one staff must be assigned to the branch")]
        public List<string> StaffIds { get; set; } = new();

        [MinLength(1, ErrorMessage = "Operating hours must be provided")]
        public List<OperatingHourDto> OperatingHours { get; set; } = new();
    }
}
