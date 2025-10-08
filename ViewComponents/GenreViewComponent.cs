using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project02.Data;

namespace Project02.ViewComponents
{
    public class GenreViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public GenreViewComponent (AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var genres = await _context.Genres.OrderBy(g => g.Genre_Name).ToListAsync();
            return View(genres);
        }
    }
}
