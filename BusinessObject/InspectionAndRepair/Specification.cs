using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.InspectionAndRepair { 
    public class Specification
    {
        [Key]
        public Guid SpecificationID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Label { get; set; } 

        public int DisplayOrder { get; set; }

   
        [ForeignKey(nameof(SpecificationCategory))]
        public Guid CategoryID { get; set; }
        public virtual SpecificationCategory  SpecificationCategory{ get; set; }

        // Navigation
        public virtual ICollection<SpecificationsData> SpecificationsDatas { get; set; }
    }
}
