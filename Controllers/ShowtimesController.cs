using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;

namespace Project02.Controllers
{
    public class ShowtimesController : Controller
    {
        private readonly AppDbContext _context;

        public ShowtimesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Showtimes
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Showtimes.Include(s => s.Cinema).Include(s => s.Hall).Include(s => s.Movie);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Showtimes/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Showtime_ID == id);
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // GET: Showtimes/Create
        public IActionResult Create()
        {
            ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_ID");
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID");
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID");
            return View();
        }

        // POST: Showtimes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Showtime_ID,Movie_ID,Cinema_ID,Hall_ID,Start_Time,End_Time,Language,Format,Price")] Showtime showtime)
        {
            if (ModelState.IsValid)
            {
                _context.Add(showtime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_ID", showtime.Cinema_ID);
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // GET: Showtimes/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
            {
                return NotFound();
            }
            ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_ID", showtime.Cinema_ID);
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // POST: Showtimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Showtime_ID,Movie_ID,Cinema_ID,Hall_ID,Start_Time,End_Time,Language,Format,Price")] Showtime showtime)
        {
            if (id != showtime.Showtime_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(showtime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowtimeExists(showtime.Showtime_ID))
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
            ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_ID", showtime.Cinema_ID);
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // GET: Showtimes/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Showtime_ID == id);
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // POST: Showtimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShowtimeExists(long id)
        {
            return _context.Showtimes.Any(e => e.Showtime_ID == id);
        }
    }
}
