using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels;
using Project02.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

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
            var moviesWithShowtime = await _ctx.Showtimes
                .Select(s => s.Movie.Movie_Slug)
                .Distinct()
                .ToListAsync();

            var movies = await _ctx.Movies.Select(m => new MovieShowVm
            {
                MovieSlug = m.Movie_Slug,
                MovieName = m.Movie_Name,
                MovieYear = m.Movie_Year,
                MoviePoster = m.Movie_Poster,
            })
            .ToListAsync();

            var recommend = await _ctx.Movies.OrderBy(f => Guid.NewGuid()).Take(16).Select(m => new MovieShowVm
            {
                MovieSlug = m.Movie_Slug,
                MovieName = m.Movie_Name,
                MovieYear = m.Movie_Year,
                MoviePoster = m.Movie_Poster,
            }).ToListAsync();

            var vm = new UserDashBoardVm
            {
                Movies = movies,
                RandomMovies = recommend,
                MoviesWithShowtime = moviesWithShowtime
            };

            return View(vm);
        }


        [HttpGet("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("/about")]
        public IActionResult About()
        {
            return View();
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
