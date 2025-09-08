using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Extension;
using Project02.Models;
using Project02.Services;
using Project02.ViewModels;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Project02.Helper;


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
        [HttpGet("admin/movie")]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
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

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(m => m.Movie_Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieRowVm
                {
                    Movie_ID = m.Movie_ID,
                    Movie_Slug = m.Movie_Slug,
                    Movie_Name = m.Movie_Name,
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
            };

            return View(vm);
            
        }

        
        [HttpGet("movie/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var vm = await _ctx.Movies.Where(m => m.Movie_Slug == id)
                .AsNoTracking()
                .Select(m => new MovieDetailVm
                {
                    Movie_Slug = m.Movie_Slug,
                    Movie_Name = m.Movie_Name,
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

        // GET: Movies/Create
        [HttpGet("/admin/movie/create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                Movie_Slug = SlugHelper.GenerateSlug(vm.Movie_Name),
                Movie_Poster = posterPath
            };

            _ctx.Movies.Add(movie);
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        [HttpGet("admin/movie/edit/{id}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _ctx.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var movie = await _ctx.Movies.FirstOrDefaultAsync(m => m.Movie_ID == vm.Movie_ID);
            if (movie == null) return NotFound();

            movie.Movie_Name = vm.Movie_Name;
            movie.Movie_Description = vm.Movie_Description;
            movie.Movie_Duration = vm.Movie_Duration;
            movie.Movie_Status = vm.Movie_Status;

            if (vm.Movie_Poster != null)
            {
                await _files.DeleteAsync(movie.Movie_Poster);

                movie.Movie_Poster = await _files.SaveAsync(vm.Movie_Poster, "uploads/posters");
            }

            _ctx.Entry(movie).Property("RowVersion").OriginalValue = vm.RowVersion;

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

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _ctx.Movies
                .FirstOrDefaultAsync(m => m.Movie_ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var movie = await _ctx.Movies.FindAsync(id);
            if (movie != null)
            {
                _ctx.Movies.Remove(movie);
            }

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(long id)
        {
            return _ctx.Movies.Any(e => e.Movie_ID == id);
        }
    }
}
