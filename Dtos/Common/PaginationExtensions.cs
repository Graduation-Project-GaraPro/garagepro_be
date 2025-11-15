using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Common
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, count, pageNumber, pageSize);
        }

        public static PagedResult<T> ToPagedResult<T>(
            this List<T> source,
            int pageNumber,
            int pageSize)
        {
            var count = source.Count;
            var items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>(items, count, pageNumber, pageSize);
        }
    }
}
