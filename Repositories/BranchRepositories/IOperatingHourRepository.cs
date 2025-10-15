using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Branches;

namespace Repositories.BranchRepositories
{
    public interface IOperatingHourRepository
    {
        Task<IEnumerable<OperatingHour>> GetByBranchAsync(Guid branchId);
    }

}
