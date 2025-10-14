using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/ticket")]
        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1, int pageSize = 10)
        {
            var query = _context.Tickets
                .Include(t => t.OrderSeat)
                    .ThenInclude(os => os.Seat)
                .Include(t => t.OrderSeat)
                    .ThenInclude(os => os.Order)
                    .ThenInclude(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .AsQueryable();

            // Tìm kiếm theo tên phim
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.OrderSeat.Order.Showtime.Movie.Movie_Name.Contains(search));
            }

            // Sắp xếp theo sortOrder
            query = sortOrder switch
            {
                "start_time_asc" => query.OrderBy(t => t.OrderSeat.Order.Showtime.Start_Time),
                "start_time_desc" => query.OrderByDescending(t => t.OrderSeat.Order.Showtime.Start_Time),
                "booking_time_asc" => query.OrderBy(t => t.BookingTime),
                "booking_time_desc" => query.OrderByDescending(t => t.BookingTime),
                "status_asc" => query.OrderBy(t => t.Status),
                "status_desc" => query.OrderByDescending(t => t.Status),
                _ => query.OrderByDescending(t => t.BookingTime),
            };

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TicketRowVm
                {
                    Ticket_ID = t.Ticket_ID,
                    OrderSeat_ID = t.OrderSeat_ID,
                    Showtime_ID = t.Showtime_ID,
                    Movie_Name = t.OrderSeat.Order.Showtime.Movie.Movie_Name,
                    Start_Time = t.OrderSeat.Order.Showtime.Start_Time,
                    SeatNumber = $"{t.OrderSeat.Seat.RowNumber}{t.OrderSeat.Seat.SeatNumber}",
                    BookingTime = t.BookingTime,
                    ShowtimeStartTime = t.OrderSeat.Order.Showtime.Start_Time,
                    Status = t.Status
                })
                .ToListAsync();

            var model = new TicketIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                search = search,
                sortOrder = sortOrder,
                SortOptions = new List<SelectListItem>
        {
            new SelectListItem("Thời gian chiếu ↑", "start_time_asc"),
            new SelectListItem("Thời gian chiếu ↓", "start_time_desc"),
            new SelectListItem("Thời gian đặt ↑", "booking_time_asc"),
            new SelectListItem("Thời gian đặt ↓", "booking_time_desc"),
            new SelectListItem("Trạng thái ↑", "status_asc"),
            new SelectListItem("Trạng thái ↓", "status_desc"),
        }
            };

            return View(model);
        }



        [HttpGet("/admin/ticket/{id}")]
        public async Task<IActionResult> Detail(long id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.OrderSeat)
                    .ThenInclude(os => os.Seat)
                .Include(t => t.OrderSeat)
                .ThenInclude(t => t.Order)
                .ThenInclude(n =>n.Showtime)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(t => t.Ticket_ID == id);

            if (ticket == null)
                return NotFound();

            // Tự động cập nhật trạng thái expired nếu qua thời gian chiếu
            if (ticket.OrderSeat.Order.Showtime.Start_Time < DateTime.Now && ticket.Status != "Expired")
            {
                ticket.Status = "Expired";
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }

            var vm = new TicketRowVm
            {
                Ticket_ID = ticket.Ticket_ID,
                OrderSeat_ID = ticket.OrderSeat_ID,
                Showtime_ID = ticket.Showtime_ID,
                Movie_Name = ticket.OrderSeat.Order.Showtime.Movie.Movie_Name,
                Start_Time = ticket.OrderSeat.Order.Showtime.Start_Time,
                SeatNumber = ticket.OrderSeat.Seat.SeatNumber,
                BookingTime = ticket.BookingTime,
                ShowtimeStartTime = ticket.OrderSeat.Order.Showtime.Start_Time,
                Status = ticket.Status
            };

            return View(vm);
        }


        // GET: hiển thị form chỉnh trạng thái vé
        [HttpGet("/admin/ticket/edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            var vm = new TicketRowVm
            {
                Ticket_ID = ticket.Ticket_ID,
                Status = ticket.Status,
            };

            return View(vm);
        }

        // POST: cập nhật trạng thái vé
        [HttpPost("/admin/ticket/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, TicketRowVm model)
        {
            if (id != model.Ticket_ID)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            ticket.Status = model.Status;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = ticket.Ticket_ID });
        }


        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.OrderSeat)
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
