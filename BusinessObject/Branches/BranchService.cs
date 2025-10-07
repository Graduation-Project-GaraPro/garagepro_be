using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Branches
{
    public class BranchService
    {
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }

        public Guid ServiceId { get; set; }
        public Service Service { get; set; }
    }

}
