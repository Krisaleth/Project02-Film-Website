using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Helper;
using Project02.Models;
using Project02.Services;
using Project02.ViewModels;
using Project02.ViewModels.Movie;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;


namespace Project02.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IFileStorage _files;

        public MoviesController(AppDbContext context, IFileStorage files)
        {
            _ctx = context;
            _files = files;
        }

        // GET: Movies
        [HttpGet("/admin/movie")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? q, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Movie> query = _ctx.Movies.AsNoTracking();

            // Search
            if (!string.IsNullOrEmpty(q))
            {
                var keyWord = q.Trim();
                query = query.Where(m => m.Movie_Name.Contains(keyWord));
            }

            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(m => m.Movie_Name),
                "name_desc" => query.OrderByDescending(m => m.Movie_Name),
                "status_asc" => query.OrderBy(m => m.Movie_Status),
                "status_desc" => query.OrderByDescending(m => m.Movie_Status),
                "year_asc" => query.OrderBy(m => m.Movie_Year),          // nếu có cột năm
                "year_desc" => query.OrderByDescending(m => m.Movie_Year),
                _ => query.OrderBy(m => m.Movie_ID)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieRowVm
                {
                    Movie_ID = m.Movie_ID,
                    Movie_Slug = m.Movie_Slug,
                    Movie_Name = m.Movie_Name,
                    Movie_Year = m.Movie_Year,
                    Movie_Producer = m.Movie_Producer,
                    Movie_Description = m.Movie_Description,
                    Movie_Duration = m.Movie_Duration,
                    Movie_Poster = m.Movie_Poster,
                    Movie_Status = m.Movie_Status,
                })
                .ToListAsync();
            var vm = new MovieIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Q = q,
                SortOptions = new List<SelectListItem>
                {
                    new("Sort by", "", string.IsNullOrEmpty(sortOrder)),
                    new("Name Ascending", "name_asc", sortOrder == "name_asc"),
                    new("Name Descending", "name_desc", sortOrder == "name_desc"),
                    new("Status Ascending", "status_asc", sortOrder == "status_asc"),
                    new("Status Descending", "status_desc", sortOrder == "status_desc"),
                    new("Year Ascending", "year_asc", sortOrder == "year_asc"),
                    new("Year Descending", "year_desc", sortOrder == "year_desc"),
                },
                sortOrder = sortOrder
            };

            return View(vm);
            
        }

        
        [HttpGet("/movie/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var vm = await _ctx.Movies.Where(m => m.Movie_Slug == id)
                .AsNoTracking()
                .Select(m => new MovieDetailVm
                {
                    Movie_Slug = m.Movie_Slug,
                    Movie_Name = m.Movie_Name,
                    Movie_Year = m.Movie_Year,
                    Movie_Producer = m.Movie_Producer,
                    Movie_Description = m.Movie_Description,
                    Movie_Poster = m.Movie_Poster,
                    DurationFormatted = (m.Movie_Duration / 60) + "h" + (m.Movie_Duration % 60) + "m",
                    Genres = m.Genres.Select(g => g.Genre_Name).ToList(),
                }).FirstOrDefaultAsync();

            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        
        [Authorize(Roles = "Admin")]
        [HttpGet("/admin/movie/create")]
        public IActionResult Create()
        {
            return View();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/movie/create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create(MovieCreateVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var posterPath = await _files.SaveAsync(vm.Movie_Poster, "uploads/posters");

            var movie = new Movie
            {
                Movie_Name = vm.Movie_Name,
                Movie_Description = vm.Movie_Description,
                Movie_Duration = vm.Movie_Duration,
                Movie_Status = vm.Movie_Status,
                Movie_Slug = SlugHelper.GenerateSlug($"{vm.Movie_Name}-${vm.Movie_Year}" ),
                Movie_Poster = posterPath
            };

            _ctx.Movies.Add(movie);
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/admin/movie/edit/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit([FromRoute]string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = await _ctx.Movies.Where(m => m.Movie_Slug == id)
                .AsNoTracking()
                .Select(m => new MovieEditVm
                {
                    Movie_ID = m.Movie_ID,        
                    Movie_Name = m.Movie_Name,
                    Movie_Slug = m.Movie_Slug,
                    Movie_Year = m.Movie_Year,
                    Movie_Producer = m.Movie_Producer,
                    Movie_Description = m.Movie_Description,
                    Movie_Duration = m.Movie_Duration,
                    Movie_Status = m.Movie_Status,
                    ExistingPoster = m.Movie_Poster,
                    RowsVersion = m.RowsVersion
                }).FirstOrDefaultAsync();
            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/movie/edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit([FromRoute]string id, MovieEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var movie = await _ctx.Movies.FirstOrDefaultAsync(m => m.Movie_Slug == id);
            if (movie == null) return NotFound();

            movie.Movie_Name = vm.Movie_Name;
            movie.Movie_Description = vm.Movie_Description;
            movie.Movie_Duration = vm.Movie_Duration;
            movie.Movie_Status = vm.Movie_Status;

            string? newPosterPath = null;
            if (vm.Movie_Poster != null && vm.Movie_Poster.Length > 0)
            {
                newPosterPath = await _files.SaveAsync(vm.Movie_Poster, "uploads/posters");
                movie.Movie_Poster = newPosterPath;
            }

            _ctx.Entry(movie).Property("RowsVersion").OriginalValue = vm.RowsVersion;

            try
            {
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Bản ghi đã được thay đổi trước đó!");
                return View(vm);
            }
            
        }

        
        [HttpPost("/admin/movie/delete/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id, string? rowVersionBase64)
        {
            var movie = await _ctx.Movies.FirstOrDefaultAsync(m => m.Movie_Slug == id);
            if (movie == null) return Json(new { ok = false, message = "Not Found" });

            if (!string.IsNullOrEmpty(rowVersionBase64))
            {
                _ctx.Entry(movie).Property(x => x.RowsVersion).OriginalValue = Convert.FromBase64String(rowVersionBase64);
            }

            _ctx.Movies.Remove(movie);

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { ok = false, message = "Bản ghi đã bị thay đổi bới ai đó" });
            }
            return Json(new { ok = true });
        }

        private bool MovieExists(string id)
        {
            return _ctx.Movies.Any(e => e.Movie_Slug == id);
        }
    }
}
