using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dtos.InspectionAndRepair
{
    public class RepairCreateDto
    {
        public Guid JobId { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string EstimatedTime { get; set; } // nhập dạng "02:30:00"
    }
}
