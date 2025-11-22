using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.RepairOrderArchivedDtos
{
    public class RepairOrderArchivedTechnicianDto
    {
        public Guid TechnicianId { get; set; }
        public string FullName { get; set; }

        public float Quality { get; set; }
        public float Speed { get; set; }
        public float Efficiency { get; set; }
        public float Score { get; set; }
    }
}
