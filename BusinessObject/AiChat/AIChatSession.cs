using BusinessObject.AiChat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.AiChat
{
    public class AIChatSession
    {
        [Key]
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public Guid VehicleId { get; set; }
        public string SessionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AIChatSessionStatus Status { get; set; }
        public int TotalMessages { get; set; }
        public string DiagnosisResult { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<AIChatMessage> Messages { get; set; }
    }
}
