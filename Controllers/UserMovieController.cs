using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Customer;
using Project02.ViewModels.Genre;
using Project02.ViewModels.Movie;
using Project02.ViewModels.MovieDetailVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
    public class UserMovieController : Controller
    {
        private readonly AppDbContext _context;

        public UserMovieController(AppDbContext context)
        {
            _context = context;
        }
        public List<Movie> GetAllMovies()
        {
            return _context.Movies.Include(m => m.Genres).ToList();
        }
        [HttpGet("/movie")]
        public async Task<IActionResult> Index(string? search, string? genreSlug, int page = 1, int pageSize = 12)
        {
            var moviesQuery = _context.Movies.Include(m => m.Genres).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                moviesQuery = moviesQuery.Where(m => m.Movie_Name.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(genreSlug))
            {
                moviesQuery = moviesQuery.Where(m => m.Genres.Any(g => g.Genre_Slug.ToLower().Contains(genreSlug.ToLower())));
            }

            int totalItems = await moviesQuery.CountAsync();

            var moviePage = await moviesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var genres = await _context.Genres.OrderBy(g => g.Genre_Name).ToListAsync();

            // Lấy danh sách phim cùng trạng thái có lịch chiếu
            var movieSlugs = moviePage.Select(m => m.Movie_Slug).ToList();

            // Truy vấn showtimes các phim trong trang để biết phim nào có showtimes
            var showtimeMovies = await _context.Showtimes
                .Where(s => movieSlugs.Contains(s.Movie.Movie_Slug))
                .Select(s => s.Movie.Movie_Slug)
                .Distinct()
                .ToListAsync();

            // Truyền data vào ViewModel hoặc ViewBag
            var movieListVm = new MovieListVm
            {
                Movies = moviePage,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                SearchKeyword = search ?? "",
                Genres = await _context.Genres.OrderBy(g => g.Genre_Name).ToListAsync(),
                SelectedGenreSlug = genreSlug,
                ShowtimeMovies = showtimeMovies
            };

            ViewBag.Genres = new SelectList(_context.Genres.OrderBy(g => g.Genre_Name), "Genre_Slug", "Genre_Name", movieListVm.SelectedGenreSlug);
            return View(movieListVm);
        }


        [HttpGet("/movie/{id}")]
        public async Task<IActionResult> Details(string? id)
        {
            
            var movie = await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.Movie_Slug == id);

            if (movie == null) return NotFound();

            var allGenres = await _context.Genres.ToListAsync();
            ViewData["Genres"] = allGenres;

            var selectedGenres = movie.Genres.Select(g => new GenreRowVm
            {
                Genre_Name = g.Genre_Name,
                Genre_Slug = g.Genre_Slug,
            }).ToList();

            var vm = new MovieDetailVm
            {
                Movie = new MovieRowVm
                {
                    Movie_Slug = movie.Movie_Slug,
                    Movie_Name = movie.Movie_Name,
                    Movie_Year = movie.Movie_Year,
                    Movie_Producer = movie.Movie_Producer,
                    Movie_Description = movie.Movie_Description,
                    Movie_Duration = movie.Movie_Duration,
                    Movie_Poster = movie.Movie_Poster,
                    Movie_Status = movie.Movie_Status,
                    Genres = selectedGenres
                },
            };

            var showtimesExist = await _context.Showtimes.AnyAsync(s => s.Movie.Movie_Slug == id);
            ViewBag.HasShowtimes = showtimesExist;

            return View(vm);
        }
        private bool MovieExists(long id)
        {
            return _context.Movies.Any(e => e.Movie_ID == id);
        }
    }
}
