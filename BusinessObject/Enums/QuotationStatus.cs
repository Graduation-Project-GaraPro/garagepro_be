using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Enums
{
    public enum QuotationStatus
    {
        [Display(Name = "Pending")]
        Pending,
        
        [Display(Name = "Sent")]
        Sent,
        
        [Display(Name = "Approved")]
        Approved,
        
        [Display(Name = "Rejected")]
        Rejected,
        
        [Display(Name = "Expired")]
        Expired
    }
}