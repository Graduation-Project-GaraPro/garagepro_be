using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Vehicle
{
    public class VehicleDto
    {
        public Guid VehicleId { get; set; }

        [Required]
        public Guid BrandId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid ModelId { get; set; }

        [Required]
        public Guid ColorId { get; set; }

        [StringLength(50)]
        public string LicensePlate { get; set; }

        [StringLength(17)]
        public string VIN { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public long Mileage { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string OwnerEmail { get; set; }

        [Required]
        public DateTime LastServiceDate { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [StringLength(100)]
        public string WarrantyStatus { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateVehicleDto
    {
        [Required]
        public Guid BrandId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid ModelId { get; set; }

        [Required]
        public Guid ColorId { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[A-Z0-9]{1,10}$", ErrorMessage = "License plate must be 1-10 uppercase alphanumeric characters")]
        public string LicensePlate { get; set; }

        [StringLength(17)]
        [RegularExpression(@"^[A-Z0-9]{17}$", ErrorMessage = "VIN must be exactly 17 uppercase alphanumeric characters")]
        public string VIN { get; set; }

        [Required]
        [Range(1886, 2030, ErrorMessage = "Year must be between 1886 and current year")]
        public int Year { get; set; }

        [Required]
        [Range(0, long.MaxValue, ErrorMessage = "Mileage must be a positive number")]
        public long Mileage { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string OwnerEmail { get; set; }

        [Required]
        public DateTime LastServiceDate { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [StringLength(100, ErrorMessage = "Warranty status cannot exceed 100 characters")]
        public string WarrantyStatus { get; set; }
    }

    public class UpdateVehicleDto
    {
        [Required]
        public Guid BrandId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid ModelId { get; set; }

        [Required]
        public Guid ColorId { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[A-Z0-9]{1,10}$", ErrorMessage = "License plate must be 1-10 uppercase alphanumeric characters")]
        public string LicensePlate { get; set; }

        [StringLength(17)]
        [RegularExpression(@"^[A-Z0-9]{17}$", ErrorMessage = "VIN must be exactly 17 uppercase alphanumeric characters")]
        public string VIN { get; set; }

        [Required]
        [Range(1886, 2030, ErrorMessage = "Year must be between 1886 and current year")]
        public int Year { get; set; }

        [Required]
        [Range(0, long.MaxValue, ErrorMessage = "Mileage must be a positive number")]
        public long Mileage { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string OwnerEmail { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [StringLength(100, ErrorMessage = "Warranty status cannot exceed 100 characters")]
        public string WarrantyStatus { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class VehicleWithCustomerDto
    {
        public VehicleDto Vehicle { get; set; }
        public CustomerDto Customer { get; set; }
    }

    public class CustomerDto
    {
        public string UserId { get; set; }
        
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }
        
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }
        
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }
        
        [Phone]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }
        
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }
    }
}