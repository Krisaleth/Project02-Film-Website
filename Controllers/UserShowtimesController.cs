using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Customer;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
    public class UserShowtimesController : Controller
    {
        private readonly AppDbContext _context;

        public UserShowtimesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(AuthenticationSchemes = "UserScheme")]
        [HttpGet("/showtime/{id}")]
        public async Task<IActionResult> SelectShowtimes(string id)
        {
            var showtimes = await _context.Showtimes.Include(m => m.Movie).Include(s => s.Hall).ThenInclude(h => h.Cinema).Where(s => s.Movie.Movie_Slug == id).ToListAsync();
            var hallsGrouped = showtimes
                .GroupBy(s => s.Hall.Cinema)
                .ToDictionary(
                    g => g.Key.Cinema_ID,
                    g => g.Select(s => s.Hall).Distinct().OrderBy(h => h.Hall_ID).ToList()
                );
            var movie = showtimes.Where(s => s.Movie.Movie_Slug == id).FirstOrDefault();
            var vm = new ShowtimeShowVm { 
                Showtimes = showtimes, 
                HallsGroupedByCinema = hallsGrouped,
                MovieName = movie.Movie.Movie_Name,
            };

            return View(vm);
        }
    }
}
