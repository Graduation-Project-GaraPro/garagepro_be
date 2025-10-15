using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Services
{
    public class BulkUpdateAdvanceStatusRequest
    {
        public List<Guid> ServiceIds { get; set; } = new();
        public bool IsAdvanced { get; set; }
    }
}
