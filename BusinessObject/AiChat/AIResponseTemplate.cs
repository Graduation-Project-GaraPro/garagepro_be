using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.AiChat
{
    public class AIResponseTemplate
    {
        [Key]
        public Guid TemplateId { get; set; }
        public Guid MessageId { get; set; }
        public string Template { get; set; }
        public string Variables { get; set; }
        public bool IsActive { get; set; }
        public string Language { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AIChatMessage Message { get; set; } = new AIChatMessage();
    }
}
