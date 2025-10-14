using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Vehicles
{
    public class VehicleBrand
    {
        [Key]
        public Guid BrandID { get; set; }

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
