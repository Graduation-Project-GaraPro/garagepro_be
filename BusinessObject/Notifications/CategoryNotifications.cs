using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Notifications
{
    public class CategoryNotification
    {
        [Key]
        public Guid CategoryID { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        // Navigation property
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
