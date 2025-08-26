using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Extension;
using Project02.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    public class FilmsController : Controller
    {
        private readonly AppDbContext _context;

        public FilmsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Films
        [HttpGet("/admin/movie")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index([FromQuery]int page = 1, [FromQuery] int pageSize = 6, [FromQuery] string? q = null)
        {
            var query = _context.Films.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(f => f.Film_Name.Contains(q));

            query = query.OrderByDescending(f => f.Film_Created_At);

            var result = await query.ToPagedResultAsync(page, pageSize);

            var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "admin";
            ViewBag.Username = username;
            ViewBag.q = q;
            return View(result);
        }

        // GET: Films/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .FirstOrDefaultAsync(m => m.Film_ID == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // GET: Films/Create
        [HttpGet("/admin/create")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var username = User.FindFirst(ClaimTypes.Name).Value;
            ViewBag.Username = username;
            return View();
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Film_ID,Film_Slug,Film_Name,Film_Type,Film_Description,Film_Duration,Film_Status,Film_Created_At,Film_Update_At")] Film film)
        {
            if (ModelState.IsValid)
            {
                _context.Add(film);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(film);
        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            return View(film);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Film_ID,Film_Slug,Film_Name,Film_Type,Film_Description,Film_Duration,Film_Status,Film_Created_At,Film_Update_At")] Film film)
        {
            if (id != film.Film_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(film);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(film.Film_ID))
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
            return View(film);
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .FirstOrDefaultAsync(m => m.Film_ID == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film != null)
            {
                _context.Films.Remove(film);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(long id)
        {
            return _context.Films.Any(e => e.Film_ID == id);
        }
    }
}
