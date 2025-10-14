using BusinessObject.Authentication;
using BusinessObject.Vehicles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Vehicle
    {
        [Key]
        public Guid VehicleId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BrandId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public Guid ModelId { get; set; }

        [Required]
        public Guid ColorId { get; set; }

        [MaxLength(50)]
        [RegularExpression(@"^[A-Z0-9\-]{1,15}$")] // Allow hyphens in license plates
        public string LicensePlate { get; set; }

        [MaxLength(17)]
        [RegularExpression(@"^[A-Z0-9]{17}$")]
        public string VIN { get; set; }

        [Required]
        [Range(1886, 2030)]
        public int Year { get; set; }

        [Range(0, long.MaxValue)]
        public long? Odometer { get; set; }

        [Required]
        public DateTime LastServiceDate { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [MaxLength(100)]
        public string? WarrantyStatus { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BrandId")]
        public virtual VehicleBrand Brand { get; set; }

        [ForeignKey("ModelId")]
        public virtual VehicleModel Model { get; set; }

        [ForeignKey("ColorId")]
        public virtual VehicleColor Color { get; set; }

        public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
        public virtual ApplicationUser User { get; set; } = null!;
    }
}