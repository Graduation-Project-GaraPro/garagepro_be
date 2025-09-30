using BusinessObject.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Notifications
{
    public class Notification
    {
        [Key]
        public Guid NotificationID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;

        [Required]
        public NotificationStatus Status { get; set; }

        [MaxLength(200)]
        public string Target { get; set; }


         // FK -> CategoryNotification
        [Required]
        [ForeignKey(nameof(CategoryNotification))]
        public Guid CategoryID { get; set; }
        public virtual CategoryNotification CategoryNotification { get; set; }

        // FK -> User
        [Required]
        [ForeignKey(nameof(ApplicationUser))]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}

