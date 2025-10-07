using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;

using Project02.ViewModels.Customer;

namespace Project02.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _ctx;

        public HomeController(ILogger<HomeController> logger, AppDbContext ctx)
        {
            _logger = logger;
            _ctx = ctx;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _ctx.Movies.Select(m => new MovieShowVm
            {
                MovieId = m.Movie_ID,
                MovieName = m.Movie_Name,
                MovieYear = m.Movie_Year,
                MoviePoster = m.Movie_Poster,
            })
            .ToListAsync();

            var recommend = await _ctx.Movies.OrderBy(f => Guid.NewGuid()).Take(16).Select(m => new MovieShowVm
            {
                MovieId = m.Movie_ID,
                MovieName = m.Movie_Name,
                MovieYear = m.Movie_Year,
                MoviePoster = m.Movie_Poster,
            }).ToListAsync();

            var vm = new UserDashBoardVm
            {
                Movies = movies,
                RandomMovies = recommend,
            };

            return View(vm);
        }

        [HttpGet("/privacy")]
        public IActionResult Privacy()
        {
            // T?o d? li?u test Movie
            var movie = new Movie
            {
                Movie_ID = 1,
                Movie_Name = "Inception",
                Movie_Poster = "/uploads/posters/inception-2010.jpg",
                Movie_Producer = "Syncopy / Warner Bros.",
                Movie_Description = "A skilled thief leads a team into the subconscious world of dreams.",
                Movie_Duration = 148,
                Movie_Year = 2010,

                // Genres d??i d?ng ICollection<Genre>
                Genres = new List<Genre>
                {
                    new Genre { Genre_Name = "Action" },
                    new Genre { Genre_Name = "Sci-Fi" },
                    new Genre { Genre_Name = "Thriller" }
                },

                // Showtimes test v?i Cinema và Hall ??y ??
                Showtimes = new List<Showtime>
                {
                    new Showtime
                    {
                        Showtime_ID = 1,
                        Hall_ID = 1,
                        Hall = new Hall { Hall_ID = 1 },
                        Start_Time = DateTime.Now.AddHours(1),
                        End_Time = DateTime.Now.AddHours(3),
                        Language = "English",
                        Format = "2D",
                    },
                    new Showtime
                    {
                        Showtime_ID = 2,
                        Hall_ID = 2,
                        Hall = new Hall { Hall_ID = 2 },
                        Start_Time = DateTime.Now.AddHours(4),
                        End_Time = DateTime.Now.AddHours(6),
                        Language = "Vietnamese",
                        Format = "3D",
                    }
                }
            };

            return View(movie);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
