using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.SystemLogs
{
    public class SecurityLog
    {
        [Key, ForeignKey(nameof(SystemLog))]
        public long Id { get; set; }

        [MaxLength(20)]
        public string ThreatLevel { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        [MaxLength(255)]
        public string Resource { get; set; }

        [MaxLength(20)]
        public string Outcome { get; set; }

        // Navigation
        public SystemLog SystemLog { get; set; }
        public ICollection<SecurityLogRelation> Relations { get; set; }
    }
}
