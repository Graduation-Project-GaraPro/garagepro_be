using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Vehicles
{
    public class VehicleColor
    {
        [Key]
        public Guid ColorID { get; set; }

        [Required]
        [MaxLength(50)]
        public string ColorName { get; set; }

        [MaxLength(7)]
        public string HexCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<VehicleModelColor> VehicleModelColors { get; set; } = new List<VehicleModelColor>();
    }
}
