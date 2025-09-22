using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Ticket;

namespace Project02.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/ticket")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                .ThenInclude(s => s.Hall)
                .Include(t => t.User)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                var keyword = search.Trim();
                query = query.Where(t => t.Seat.SeatNumber.Contains(keyword) ||
                                         t.Showtime.Movie.Movie_Name.Contains(keyword) ||
                                         t.User.User_Name.Contains(keyword));
            }
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(t => t.Price),
                "price_desc" => query.OrderByDescending(t => t.Price),
                "bookingtime_asc" => query.OrderBy(t => t.BookingTime),
                "bookingtime_desc" => query.OrderByDescending(t => t.BookingTime),
                _ => query.OrderBy(t => t.Ticket_ID),
            };
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page > totalPages && totalPages > 0)
                page = totalPages;
            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .Select(t => new TicketRowVm
                                   {
                                       Ticket_ID = t.Ticket_ID,
                                       Movie_Name = t.Showtime.Movie.Movie_Name,
                                       Start_Time = t.Showtime.Start_Time,
                                       SeatNumber = t.Seat.SeatNumber,
                                       User_Name = t.User.User_Name,
                                       User_Phone = t.User.User_Phone,
                                       User_Email = t.User.User_Email,
                                       Price = t.Price,
                                       Payment_Method = t.Payments.OrderByDescending(p => p.PaymentTime).Select(p => p.PaymentMethod).FirstOrDefault() ?? "N/A",
                                       Payment_Status = t.Payments.OrderByDescending(p => p.PaymentTime).Select(p => p.PaymentStatus).FirstOrDefault() ?? "N/A",
                                       Status = t.Status,
                                   })
                                   .ToListAsync();
            var vm = new TicketIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                search = search,
                sortOrder = sortOrder,
                SortOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Sort by", Selected = string.IsNullOrEmpty(sortOrder) },
                    new SelectListItem { Value = "price_asc", Text = "Price Ascending", Selected = sortOrder == "price_asc" },
                    new SelectListItem { Value = "price_desc", Text = "Price Descending", Selected = sortOrder == "price_desc" },
                    new SelectListItem { Value = "bookingtime_asc", Text = "Booking Time Ascending", Selected = sortOrder == "bookingtime_asc" },
                    new SelectListItem { Value = "bookingtime_desc", Text = "Booking Time Descending", Selected = sortOrder == "bookingtime_desc" },
                }
            };
            return View(vm);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Showtime)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Ticket_ID == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["Seat_ID"] = new SelectList(_context.Seats, "Seat_ID", "Seat_ID");
            ViewData["Showtime_ID"] = new SelectList(_context.Showtimes, "Showtime_ID", "Showtime_ID");
            ViewData["User_ID"] = new SelectList(_context.Users, "User_ID", "User_ID");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ticket_ID,Showtime_ID,Seat_ID,User_ID,Price,Status,BookingTime")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Seat_ID"] = new SelectList(_context.Seats, "Seat_ID", "Seat_ID", ticket.Seat_ID);
            ViewData["Showtime_ID"] = new SelectList(_context.Showtimes, "Showtime_ID", "Showtime_ID", ticket.Showtime_ID);
            ViewData["User_ID"] = new SelectList(_context.Users, "User_ID", "User_ID", ticket.User_ID);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["Seat_ID"] = new SelectList(_context.Seats, "Seat_ID", "Seat_ID", ticket.Seat_ID);
            ViewData["Showtime_ID"] = new SelectList(_context.Showtimes, "Showtime_ID", "Showtime_ID", ticket.Showtime_ID);
            ViewData["User_ID"] = new SelectList(_context.Users, "User_ID", "User_ID", ticket.User_ID);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Ticket_ID,Showtime_ID,Seat_ID,User_ID,Price,Status,BookingTime")] Ticket ticket)
        {
            if (id != ticket.Ticket_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Ticket_ID))
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
            ViewData["Seat_ID"] = new SelectList(_context.Seats, "Seat_ID", "Seat_ID", ticket.Seat_ID);
            ViewData["Showtime_ID"] = new SelectList(_context.Showtimes, "Showtime_ID", "Showtime_ID", ticket.Showtime_ID);
            ViewData["User_ID"] = new SelectList(_context.Users, "User_ID", "User_ID", ticket.User_ID);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Showtime)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Ticket_ID == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(long id)
        {
            return _context.Tickets.Any(e => e.Ticket_ID == id);
        }
    }
}
