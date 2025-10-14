using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Helper;
using Project02.Models;
using Project02.ViewModels.Genre;
using Project02.ViewModels.Movie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class GenresController : Controller
    {
        private readonly AppDbContext _ctx;

        public GenresController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET: Genres
        [HttpGet("admin/genre")]
        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Genre> query = _ctx.Genres.AsNoTracking();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                var keyWord = search.Trim();
                query = query.Where(g => g.Genre_Name.Contains(keyWord));
            }

            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(g => g.Genre_Name),
                "name_desc" => query.OrderByDescending(g => g.Genre_Name),
                _ => query.OrderBy(g => g.Genre_ID)
            };
            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GenreRowVm
                {
                    Genre_ID = g.Genre_ID,
                    Genre_Name = g.Genre_Name,
                    Genre_Slug = g.Genre_Slug
                })
                .ToListAsync();
            var model = new GenreIndexVm
                {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                search = search,
                sortOrder = sortOrder,
                SortOptions = new List<SelectListItem>
                {
                    new("Sort by", "", string.IsNullOrEmpty(sortOrder)),
                    new("Name Ascending", "name_asc", sortOrder == "name_asc"),
                    new("Name Descending", "name_desc", sortOrder == "name_desc"),
                }
            };
            return View(model);
        }

        // GET: Genres/Create
        [HttpGet("/admin/genre/create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("/admin/genre/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GenreCreateVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var exists = await _ctx.Genres.AnyAsync(g => g.Genre_Name == vm.Genre_Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Genre_Name), "Tên thể loại đã tồn tại");
                return View(vm);
            }
            var genre = new Genre
            {
                Genre_Name = vm.Genre_Name,
                Genre_Slug = SlugHelper.GenerateSlug(vm.Genre_Name)
            };
            _ctx.Add(genre);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Genres/Edit/5
        [HttpGet("/admin/genre/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute]string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _ctx.Genres.Where(m => m.Genre_Slug == id)
                .AsNoTracking()
                .Select(m => new GenreEditVm
                {
                    Genre_ID = m.Genre_ID,
                    Genre_Name = m.Genre_Name,
                    Genre_Slug = m.Genre_Slug
                }).FirstOrDefaultAsync();
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        [HttpPost("/admin/genre/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute]string id, GenreEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var genre = await _ctx.Genres.FirstOrDefaultAsync(m => m.Genre_Slug == id);
            if (genre == null)
            {
                return NotFound();
            }
            var exists = await _ctx.Genres.AnyAsync(g => g.Genre_Name == vm.Genre_Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Genre_Name), "Tên thể loại đã tồn tại");
                return View(vm);
            }
            genre.Genre_Name = vm.Genre_Name;
            genre.Genre_Slug = SlugHelper.GenerateSlug(vm.Genre_Name);

            try
            {
                _ctx.Update(genre);
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(genre.Genre_Slug))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/admin/genre/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var genre = await _ctx.Genres.FirstOrDefaultAsync(m => m.Genre_Slug == id);
            if (genre == null) return Json(new { ok = false, message = "Not Found" });
            _ctx.Genres.Remove(genre);
            await _ctx.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private bool GenreExists(string id)
        {
            return _ctx.Genres.Any(e => e.Genre_Slug == id);
        }
    }
}