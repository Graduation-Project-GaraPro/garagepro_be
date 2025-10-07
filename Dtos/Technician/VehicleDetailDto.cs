using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Technician
{
    public class VehicleDetailDto
    {
        public Guid LookupId { get; set; }
        public string Automaker { get; set; }
        public string NameCar { get; set; }
        public List<VehicleCategoryDto> Categories { get; set; } = new();
    }

    public class VehicleCategoryDto
    {
        public string Category { get; set; }
        public int DisplayOrder { get; set; }
        public List<VehicleFieldDto> Fields { get; set; } = new();
    }

    public class VehicleFieldDto
    {
        public string Label { get; set; }
        public int DisplayOrder { get; set; }
        public string Value { get; set; }
    }
}
