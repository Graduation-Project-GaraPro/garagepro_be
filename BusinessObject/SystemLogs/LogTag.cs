using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.SystemLogs
{
    public class LogTag
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(SystemLog))]
        public long LogId { get; set; }

        [MaxLength(100)]
        public LogTagType Tag { get; set; }

        // Navigation
        public SystemLog SystemLog { get; set; }
    }
}
