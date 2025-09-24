using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Vehicles
{
    public class VehicleModelColor
    {
        public Guid ModelID { get; set; }
        public Guid ColorID { get; set; }

        // Navigation
        public virtual VehicleModel Model { get; set; }
        public virtual VehicleColor Color { get; set; }
    }
}
