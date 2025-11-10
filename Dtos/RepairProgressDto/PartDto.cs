using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class PartDto
    {
        public Guid PartId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }      
        
    }
}
