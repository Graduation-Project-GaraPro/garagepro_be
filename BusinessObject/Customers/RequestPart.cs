using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Customers
{
    public class RequestPart
    {
    [Key]
    public Guid RequestPartId { get; set; } = Guid.NewGuid();
    [Required]
    public Guid RequestServiceId { get; set; }
    [Required]
    public Guid PartId { get; set; }
    //public int number { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal totalAmount { get; set; }

    [ForeignKey("RequestServiceId")]
    public virtual RequestService RequestService { get; set; }

    [ForeignKey("PartId")]
    public virtual Part Part { get; set; }

}
}
