using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Enums
{
    public enum PaidStatus
    {
        [Display(Name = "Unpaid")]
        Unpaid,
        
        [Display(Name = "Partial")]
        Partial,
        
        [Display(Name = "Paid")]
        Paid
    }
}