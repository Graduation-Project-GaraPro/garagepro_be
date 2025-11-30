using BusinessObject.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.InspectionAndRepair
{
    public class NotificationDto
    {
        public Guid NotificationID { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public DateTimeOffset TimeSent { get; set; }
        public NotificationStatus Status { get; set; }
        public string Target { get; set; }
        public string UserID { get; set; }
    }
}
