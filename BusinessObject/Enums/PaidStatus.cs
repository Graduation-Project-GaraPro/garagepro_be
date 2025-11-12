using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Enums
{
    public enum PaidStatus
    {
        [Display(Name = "Unpaid")]
        Unpaid,
            
        [Display(Name = "Paid")]
        Paid
    }
}