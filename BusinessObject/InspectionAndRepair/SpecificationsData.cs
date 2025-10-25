using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.InspectionAndRepair
{
    public class SpecificationsData
    {
        [Key]
        public Guid DataID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Value { get; set; } // Giá trị cụ thể của xe

        // FK -> VehicleLookup
        [ForeignKey(nameof(VehicleLookup))]
        public Guid LookupID { get; set; }
        public virtual VehicleLookup VehicleLookup { get; set; }

        // FK -> Specification
        [ForeignKey(nameof(Specification))]
        public Guid FieldTemplateID { get; set; }
        public virtual Specification Specification { get; set; }
    }
}
