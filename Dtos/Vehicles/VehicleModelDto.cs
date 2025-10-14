using System;
using System.ComponentModel.DataAnnotations;

namespace Dtos.Vehicles
{
    public class VehicleModelDto
    {
        public Guid ModelID { get; set; }
        public string ModelName { get; set; }
        public int ManufacturingYear { get; set; }
        public Guid BrandID { get; set; }
        public string BrandName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVehicleModelDto
    {
        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int ManufacturingYear { get; set; }

        [Required]
        public Guid BrandID { get; set; }
    }

    public class UpdateVehicleModelDto
    {
        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int ManufacturingYear { get; set; }
    }
}