using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.AiChat
{
    public class AIChatMessage
    {
        [Key]
        public Guid MessageId { get; set; }
        public Guid SessionId { get; set; }
        public string SenderType { get; set; }
        public string MessageText { get; set; }
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime CreateAt { get; set; }

        public AIChatSession Session { get; set; }
        public ICollection<AIDiagnostic_Keyword> Keywords { get; set; }
        public ICollection<AIResponseTemplate> ResponseTemplates { get; set; }
    }
}
