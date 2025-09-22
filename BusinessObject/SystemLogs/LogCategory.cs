using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.SystemLogs
{
    public class LogCategory
    {
        [Key]
        public int Id { get; set; }  // PK

        [Required, MaxLength(50)]
        public string Name { get; set; } // system, security, application...

        public string Description { get; set; } // optional

        // Navigation
        public ICollection<SystemLog> SystemLogs { get; set; }
    }
}
