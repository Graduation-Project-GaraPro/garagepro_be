using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos.Parts;

namespace Dtos.Services
{
    public class PartCategoryForBooking
    {
        public Guid PartCategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<PartDto> Parts { get; set; } = new List<PartDto>();
    }
}
