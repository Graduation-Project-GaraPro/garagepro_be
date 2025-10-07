using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Repositories.PartRepositories
{
    public interface IPartRepository
    {
        Task<bool> ExistsAsync(Expression<Func<Part, bool>> predicate);
    }
}
