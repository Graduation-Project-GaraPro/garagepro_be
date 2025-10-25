using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.InspectionAndRepair
{
    public class SpecificationCategory
    {
        [Key]
        public Guid CategoryID { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } 

        public int DisplayOrder { get; set; } 

        // Navigation
        public virtual ICollection<Specification> Specifications { get; set; }
    }
}
