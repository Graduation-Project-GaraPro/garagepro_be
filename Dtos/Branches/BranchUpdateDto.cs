using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Branches
{
    public class BranchUpdateDto : BranchCreateDto
    {
        public Guid BranchId { get; set; }
        public bool IsActive { get; set; }
    }
}
