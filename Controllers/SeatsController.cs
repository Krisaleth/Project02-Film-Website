using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Seat;

namespace Project02.Controllers
{
    public class SeatsController : Controller
    {
        private readonly AppDbContext _context;

        public SeatsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Seats
        [HttpGet("/admin/seat")]
        public async Task<IActionResult> Index(long? hallId)
        {
            IQueryable<Seat> seatsQuery = _context.Seats.Include(s => s.Hall).AsNoTracking();

            if (hallId.HasValue)
            {
                seatsQuery = seatsQuery.Where(s => s.Hall_ID == hallId.Value);
            }

            seatsQuery = (hallId ?? 1) switch
            {
                var id when id > 0 => seatsQuery.Where(s => s.Hall_ID == id),
                _ => seatsQuery
            };

            var seats = await seatsQuery.Select(s => new SeatStatusVm
            {
                Seat_ID = s.Seat_ID,
                Hall_ID = s.Hall_ID,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                SeatType = s.SeatType,
                Cinema_Name = s.Hall.Cinema.Cinema_Name,
                St

            }).ToListAsync();
        }

        // GET: Seats/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.Seat_ID == id);
            if (seat == null)
            {
                return NotFound();
            }

            return View(seat);
        }

        // GET: Seats/Create
        public IActionResult Create()
        {
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID");
            return View();
        }

        // POST: Seats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Seat_ID,Hall_ID,RowNumber,SeatNumber,SeatType")] Seat seat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", seat.Hall_ID);
            return View(seat);
        }

        // GET: Seats/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", seat.Hall_ID);
            return View(seat);
        }

        // POST: Seats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Seat_ID,Hall_ID,RowNumber,SeatNumber,SeatType")] Seat seat)
        {
            if (id != seat.Seat_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeatExists(seat.Seat_ID))
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
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", seat.Hall_ID);
            return View(seat);
        }

        // GET: Seats/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.Seat_ID == id);
            if (seat == null)
            {
                return NotFound();
            }

            return View(seat);
        }

        // POST: Seats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat != null)
            {
                _context.Seats.Remove(seat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeatExists(long id)
        {
            return _context.Seats.Any(e => e.Seat_ID == id);
        }
    }
}
