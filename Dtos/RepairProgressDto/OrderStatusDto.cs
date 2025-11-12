using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class OrderStatusDto
    {
        public int OrderStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<LabelDto> Labels { get; set; } = new List<LabelDto>();
    }
}
