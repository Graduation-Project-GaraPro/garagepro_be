using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Vehicles
{
    public class VehicleDto
    {
        public Guid VehicleID { get; set; }
        public Guid BrandID { get; set; }
        public Guid UserID { get; set; }
        public Guid ModelID { get; set; }
        public Guid ColorID { get; set; }
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property names for reference
        public string BrandName { get; set; }
        public string ModelName { get; set; }
        public string ColorName { get; set; }
    }

    public class CreateVehicleDto
    {
        [Required]
        public Guid BrandID { get; set; }

        [Required]
        public Guid UserID { get; set; }

        [Required]
        public Guid ModelID { get; set; }

        [Required]
        public Guid ColorID { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$",
            ErrorMessage = "Invalid license plate format")]
        public string LicensePlate { get; set; }

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
            ErrorMessage = "VIN must be 17 characters, excluding I, O, Q")]
        public string VIN { get; set; }
    }

    public class UpdateVehicleDto
    {
        [Required]
        public Guid BrandID { get; set; }

        [Required]
        public Guid ModelID { get; set; }

        [Required]
        public Guid ColorID { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$",
            ErrorMessage = "Invalid license plate format")]
        public string LicensePlate { get; set; }

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
            ErrorMessage = "VIN must be 17 characters, excluding I, O, Q")]
        public string VIN { get; set; }
    }
}