using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Vehicle
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public string VIN { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public DateTime LastServiceDate { get; set; }
        public long? Odometer { get; set; }
        public Guid VehicleId { get; set; }
        public string UserId { get; set; }
        public string WarrantyStatus { get; set; }
    }

    public class CreateVehicleDto
    {
        [Required]
        public string CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Z0-9\-]{1,15}$")] // Allow hyphens in license plates
        public string LicensePlate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; }

        [Required]
        [MaxLength(50)]
        public string Model { get; set; }

        [Required]
        [Range(1886, 2030)]
        public int Year { get; set; }

        [Required]
        [MaxLength(30)]
        public string Color { get; set; }
        
        [MaxLength(17)]
        [RegularExpression(@"^[A-Za-z0-9]{17}$")]
        public string VIN { get; set; } = string.Empty;
        
        [Range(0, long.MaxValue)]
        public long? Odometer { get; set; }
    }

    public class UpdateVehicleDto
    {
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[A-Z0-9\-]{1,15}$")] // Allow hyphens in license plates
        public string LicensePlate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; }

        [Required]
        [MaxLength(50)]
        public string Model { get; set; }

        [Required]
        [Range(1886, 2030)]
        public int Year { get; set; }

        [Required]
        [MaxLength(30)]
        public string Color { get; set; }
        
        [MaxLength(17)]
        [RegularExpression(@"^[A-Za-z0-9]{17}$")]
        public string VIN { get; set; } = string.Empty;
        
        [Range(0, long.MaxValue)]
        public long? Odometer { get; set; }
        
        public DateTime? NextServiceDate { get; set; }
        
        [MaxLength(100)]
        public string WarrantyStatus { get; set; } = string.Empty;
    }
}