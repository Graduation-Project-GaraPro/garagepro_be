using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairOrderArchivedDtos
{
    public class RepairOrderArchivedJobPartDto
    {
        public Guid JobPartId { get; set; }
        public Guid PartId { get; set; }
        public string PartName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public int? WarrantyMonths { get; set; }

        public DateTime? WarrantyStartAt { get; set; }

        public DateTime? WarrantyEndAt { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }

}
