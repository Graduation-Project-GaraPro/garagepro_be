using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Vehicles
{
    public class VehicleModel
    {
        [Key]
        public Guid ModelID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; }
        public int ManufacturingYear { get; set; }

        [Required]
        public Guid BrandID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BrandID")]
        public virtual VehicleBrand Brand { get; set; }

        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<VehicleModelColor> VehicleModelColors { get; set; }
    }
}
