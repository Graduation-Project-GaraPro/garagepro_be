using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Branches
{
    public class BranchPart
    {
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }

        public Guid PartId { get; set; }
        public Part Part { get; set; }
    }
}
