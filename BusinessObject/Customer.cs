using BusinessObject.Authentication;
using BusinessObject.Manager;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessObject
{
    public class Customer
    {
        public Guid CustomerId { get; set; }

        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }

        public Guid BranchId { get; set; }         // FK tới Branch.Id
        public Branch Branch { get; set; }        // Navigation property

        public ICollection<FeedBack> Feedbacks { get; set; }
    }
}
