using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Technician
{
    public class Specifications
    {
        [Key]
        public Guid SpecificationsID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        // FK -> VehicleLookup
        [ForeignKey(nameof(VehicleLookup))]
        public Guid LookupID { get; set; }
        public virtual VehicleLookup VehicleLookup { get; set; }

        // Navigation property
        public virtual ICollection<SpecificationsData> SpecificationsData { get; set; }
    }
}
