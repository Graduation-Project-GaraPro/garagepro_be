using BusinessObject.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.InspectionAndRepair
{
    public class SendNotificationRequest
    {
        public string UserId { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Message;
        public string Target { get; set; }
        public string Title { get; set; } = "Notification";
        public Dictionary<string, object> Metadata { get; set; } 
    }
}
