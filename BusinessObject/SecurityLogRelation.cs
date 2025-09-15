using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class SecurityLogRelation
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey(nameof(SecurityLog))]
        public long SecurityLogId { get; set; }

        [ForeignKey(nameof(RelatedLog))]
        public long RelatedLogId { get; set; }

        public SecurityLog SecurityLog { get; set; }
        public SystemLog RelatedLog { get; set; }
    }
}
