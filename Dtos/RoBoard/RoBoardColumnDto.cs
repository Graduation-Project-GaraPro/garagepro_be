using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RoBoard
{
    public class RoBoardColumnDto
    {
        public Guid OrderStatusId { get; set; }
        public string StatusName { get; set; }
        public int RepairOrderCount { get; set; }
        public int OrderIndex { get; set; }
        public List<RoBoardCardDto> Cards { get; set; } = new List<RoBoardCardDto>();
        public List<RoBoardLabelDto> AvailableLabels { get; set; } = new List<RoBoardLabelDto>();
    }

    public class RoBoardColumnsDto
    {
        public List<RoBoardColumnDto> Pending { get; set; } = new List<RoBoardColumnDto>();
        public List<RoBoardColumnDto> InProgress { get; set; } = new List<RoBoardColumnDto>();
        public List<RoBoardColumnDto> Completed { get; set; } = new List<RoBoardColumnDto>();
    }

    public class RoBoardLabelDto
    {
        public Guid LabelId { get; set; }
        public string LabelName { get; set; }
        public string Description { get; set; }
        public RoBoardColorDto Color { get; set; }
    }

    public class RoBoardColorDto
    {
        public Guid ColorId { get; set; }
        public string ColorName { get; set; }
        public string HexCode { get; set; }
    }
}