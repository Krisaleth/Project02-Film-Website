using Project02.Models;
using Microsoft.EntityFrameworkCore;

namespace Project02.Extension
{
    public static class PagingExtension
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T> (
            this IQueryable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var total = await query.CountAsync();

            var lastPage = Math.Max(1, (int)Math.Ceiling((double)total/pageSize));
            if (page > lastPage) page = lastPage;

            var items = await query
                .Skip((page - 1)*pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }
    }
}
