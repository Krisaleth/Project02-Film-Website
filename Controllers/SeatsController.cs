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
            long selectedHallId = hallId ?? 1;

            IQueryable<Seat> seatsQuery = _context.Seats.Include(s => s.Hall).AsNoTracking();

            if (hallId.HasValue)
            {
                seatsQuery = seatsQuery.Where(s => s.Hall_ID == hallId);
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
                Description = s.Description,
                Cinema_Name = s.Hall.Cinema.Cinema_Name,
                Status = s.SeatStatus,

            }).ToListAsync();

            var cinemasWithHalls = _context.Cinemas
                .Select(cinema => new
                {
                    Cinema = cinema,
                    Halls = _context.Halls.Where(h => h.Cinema_ID == cinema.Cinema_ID).ToList()
                })
                .ToList();

            var hallOptions = new List<SelectListItem>();

            foreach (var item in cinemasWithHalls)
            {
                var cinemaName = item.Cinema.Cinema_Name;
                var halls = item.Halls;
                if (halls.Count == 1)
                {
                    hallOptions.Add(new SelectListItem
                    {
                        Value = halls[0].Hall_ID.ToString(),
                        Text = $"{cinemaName} - Hall"
                    });
                }
                else
                {
                    for (int i = 0; i < halls.Count; i++)
                    {
                        hallOptions.Add(new SelectListItem
                        {
                            Value = halls[i].Hall_ID.ToString(),
                            Text = $"{cinemaName} - Hall {i + 1}"
                        });
                    }
                }
            }

            var vm = new SeatIndexVm
            {
                Items = seats,
                selectedHallId = hallId,
                hallOptions = hallOptions,
            };

            return View(vm);
        }



        // GET: Seats/Create
        [HttpGet("/admin/seat/create")]
        public IActionResult Create()
        {
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Cinema_Name");
            return View();
        }

        // POST: Seats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/seat/create")]
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
