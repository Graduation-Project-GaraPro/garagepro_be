using BusinessObject.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Vehicles
{
    public class Vehicle
    {
        public Guid VehicleID { get; set; }

        [Required]
        public Guid? BrandID { get; set; }//chưa fixx thì có thể null

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

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BrandID")]
        public virtual VehicleBrand Brand { get; set; }

        [ForeignKey("ModelID")]
        public virtual VehicleModel Model { get; set; }

        [ForeignKey("ColorID")]
        public virtual VehicleColor Color { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser Customer { get; set; }  // Nullable navigation
    }
}
