using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

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
        public string LicensePlate { get; set; }

        [MaxLength(100)]
        public string VIN { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<RepairOrder> RepairOrders { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}