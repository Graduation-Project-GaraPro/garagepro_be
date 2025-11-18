using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.RequestEmergency
{
    public class PriceEmergency
    {
        [Key]
            public Guid PriceId { get; set; }
            public decimal BasePrice { get; set; }        // Giá mở cửa (VD: 50,000)
            public decimal PricePerKm { get; set; }       // Giá theo mỗi km (VD: 10,000/km)
            public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        

    }
}
