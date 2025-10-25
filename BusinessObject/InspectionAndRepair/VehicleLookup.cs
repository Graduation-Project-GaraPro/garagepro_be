using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.InspectionAndRepair
{
    public class VehicleLookup
    {
        [Key]
        public Guid LookupID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Automaker { get; set; }

        [Required]
        [MaxLength(100)]
        public string NameCar { get; set; }

        // Navigation property
        public virtual ICollection<SpecificationsData> SpecificationsDatas { get; set; }
    }
}
