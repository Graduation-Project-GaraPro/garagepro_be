using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public Guid CustomerId { get; set; }

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
        public virtual ICollection<RepairOrder> RepairOrders { get; set; }
        public virtual Customer Customer { get; set; }
    }
}