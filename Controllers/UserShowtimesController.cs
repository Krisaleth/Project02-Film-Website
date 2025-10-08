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
            var vm = new ShowtimeShowVm { 
                Showtimes = showtimes, 
                HallsGroupedByCinema = hallsGrouped,
                MovieName = showtimes.Where(s => s.Movie.Movie_Slug == id).First().Movie.Movie_Name
            };

            return View(vm);
        }

        // GET: UserShowtimesControler/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Showtime_ID == id);
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // GET: UserShowtimesControler/Create
        public IActionResult Create()
        {
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID");
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID");
            return View();
        }

        // POST: UserShowtimesControler/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Showtime_ID,Movie_ID,Hall_ID,Start_Time,End_Time,Language,Format")] Showtime showtime)
        {
            if (ModelState.IsValid)
            {
                _context.Add(showtime);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // GET: UserShowtimesControler/Edit/5
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
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // POST: UserShowtimesControler/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Showtime_ID,Movie_ID,Hall_ID,Start_Time,End_Time,Language,Format")] Showtime showtime)
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
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // GET: UserShowtimesControler/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Showtime_ID == id);
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // POST: UserShowtimesControler/Delete/5
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
