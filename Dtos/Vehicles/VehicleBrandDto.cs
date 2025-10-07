using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Vehicles
{
    public class VehicleBrandDto
    {
        public Guid BrandID { get; set; }
        public string BrandName { get; set; }
        public string Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VehicleModelDto> Models { get; set; }
    }

    public class CreateVehicleBrandDto
    {
        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }
    }

    public class UpdateVehicleBrandDto
    {
        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; }

        [MaxLength(50)]
        public string Country { get; set; }
    }
}