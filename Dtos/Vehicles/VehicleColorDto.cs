using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Vehicles
{
    public class VehicleColorDto
    {
        public Guid ColorID { get; set; }
        public string ColorName { get; set; }

    }
}
