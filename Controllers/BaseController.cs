using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Project02.Data;

namespace Project02.Controllers
{
    public class BaseController : Controller
    {
       private readonly AppDbContext _context;

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
        {
            var genres = await _context.Genres.OrderBy(g => g.Genre_Name).ToListAsync();
            ViewData["Genres"] = genres;

            await next();
        }
    }
}
