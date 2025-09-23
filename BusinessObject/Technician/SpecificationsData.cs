using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Technician
{
    public class SpecificationsData
    {
        [Key]
        public Guid DataID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Label { get; set; }

        [Required]
        [MaxLength(200)]
        public string Value { get; set; }

        // FK -> Specifications
        [ForeignKey(nameof(Specifications))]
        public Guid SpecificationsID { get; set; }
        public virtual Specifications Specifications { get; set; }
    }
}
