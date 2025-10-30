using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairProgressDto
{
    public class LabelDto
    {
        public Guid LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string HexCode { get; set; } = string.Empty;
    }
}
