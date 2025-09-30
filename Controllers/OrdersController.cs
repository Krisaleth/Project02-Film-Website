using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Order;

namespace Project02.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        [HttpGet("/admin/order")]
        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(u => u.User)
                .AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                ordersQuery = ordersQuery.Where(o => o.Status.Contains(search) || o.User_ID.ToString().Contains(search));
            }
            switch (sortOrder)
            {
                case "date_asc":
                    ordersQuery = ordersQuery.OrderBy(o => o.OrderDate);
                    break;
                case "date_desc":
                    ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);
                    break;
                case "amount_asc":
                    ordersQuery = ordersQuery.OrderBy(o => o.TotalAmount);
                    break;
                case "amount_desc":
                    ordersQuery = ordersQuery.OrderByDescending(o => o.TotalAmount);
                    break;
                default:
                    ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);
                    break;
            }
            var totalItems = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) page = 1;
            else if (page > totalPages) page = totalPages;
            var orders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderRowVm
                {
                    Order_ID = o.Order_ID,
                    User_ID = o.User_ID,
                    UserName = o.User.User_Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToListAsync();
            var vm = new OrderIndexVm
            {
                Orders = orders,
                Page = page,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchString = search,
                SortOrder = sortOrder,
                SortOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "date_asc", Text = "Date Ascending" },
                    new SelectListItem { Value = "date_desc", Text = "Date Descending" },
                    new SelectListItem { Value = "amount_asc", Text = "Amount Ascending" },
                    new SelectListItem { Value = "amount_desc", Text = "Amount Descending" }
                },
                TotalItems = totalItems,
                PageSize = pageSize
            };
            return View(vm);
        }

        // GET: Orders/Details/5
        [HttpGet("/admin/order/details/{id}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return BadRequest("Order ID is required.");
            }

            var order = await _context.Orders
                .Where(o => o.Order_ID == id)
                .Select(o => new OrderDetailVm
                {
                    Order_ID = o.Order_ID,
                    User_ID = o.User_ID,
                    UserName = o.User.User_Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderSeats = o.OrderSeats.Select(os => new OrderSeatVm
                    {
                        OrderSeat_ID = os.OrderSeat_ID,
                        Seat_ID = os.Seat_ID,
                        Price = os.Price,
                        Status = os.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return View(order);
        }

        [HttpGet("/admin/order/getseats/{hallId}")]
        public async Task<IActionResult> GetSeatsByHall(long hallId)
        {
            var seats = await _context.Seats
                .Where(s => s.Hall_ID == hallId)
                .Select(s => new {
                    s.Seat_ID,
                    SeatName = s.RowNumber + s.SeatNumber.ToString(),
                    s.RowNumber,
                    s.SeatNumber,
                    s.SeatPrice,
                    s.SeatStatus,
                })
                .ToListAsync();

            return Json(seats);
        }


        [HttpGet("/admin/order/getshowtime/{id}")]
        public async Task<IActionResult> GetShowtimesByHall(long hallId)
        {
            // Lấy hall đã chọn, lấy Cinema_ID để áp hall count trong cinema đó
            var hall = await _context.Halls.Include(h => h.Cinema).FirstOrDefaultAsync(h => h.Hall_ID == hallId);
            if (hall == null)
                return Json(new List<object>());

            // Lấy danh sách halls cùng cinema
            var hallsInCinema = await _context.Halls
                .Where(h => h.Cinema_ID == hall.Cinema_ID)
                .OrderBy(h => h.Hall_ID)
                .ToListAsync();

            // Map Hall_ID -> HallCount
            var hallIdToCount = hallsInCinema
                .Select((h, idx) => new { h.Hall_ID, HallCount = idx + 1 })
                .ToDictionary(x => x.Hall_ID, x => x.HallCount);

            // Lấy showtime cùng hall
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
                .Where(s => s.Hall_ID == hall.Hall_ID)
                .ToListAsync();

            var result = showtimes.Select(s => new
            {
                s.Showtime_ID,
                ShowtimeInfo = s.Movie.Movie_Name
                    + " | " + s.Hall.Cinema.Cinema_Name
                    + " | Phòng " + hallIdToCount[s.Hall_ID]
                    + " | " + s.Start_Time.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            return Json(result);
        }

        // GET: Orders/Create
        [HttpGet("/admin/order/create")]
        public async Task<IActionResult> Create()
        {
            var users = await _context.Users
                .Select(u => new { u.User_ID, u.User_Name, u.User_Phone, u.User_Email })
                .ToListAsync();
            var halls = await _context.Halls
                        .Include(h => h.Cinema)
                        .OrderBy(h => h.Cinema_ID)
                        .ThenBy(h => h.Hall_ID)
                        .ToListAsync();

            var hallDtos = halls
                .GroupBy(h => h.Cinema_ID)
                .SelectMany(group =>
                    group.Select((hall, idx) => new
                    {
                        Hall_ID = hall.Hall_ID,
                        Hall_Name = hall.Cinema.Cinema_Name + " " + (idx + 1)
                    })
                ).ToList();
            var cinemaHalls = await _context.Cinemas
                            .Include(c => c.Halls)
                            .ToListAsync();

            var hallIdToCount = new Dictionary<long, int>();
            foreach (var cinema in cinemaHalls)
            {
                var sortedHalls = cinema.Halls.OrderBy(h => h.Hall_ID).ToList();
                for (int i = 0; i < sortedHalls.Count; i++)
                {
                    hallIdToCount[sortedHalls[i].Hall_ID] = i + 1;
                }
            }

            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ThenInclude(s => s.Cinema)
                .ToListAsync();

            var finalShowtimeList = showtimes.Select(s => new
            {
                Showtime_ID = s.Showtime_ID,
                Showtime_Name = s.Movie.Movie_Name
                    + " | " + s.Hall.Cinema.Cinema_Name
                    + " | Phòng " + hallIdToCount[s.Hall_ID]
                    + " | " + s.Start_Time.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            ViewBag.Showtimes = new SelectList(finalShowtimeList, "Showtime_ID", "Showtime_Name");
            ViewBag.Users = new SelectList(users, "User_ID", "User_Name");
            ViewBag.Halls = new SelectList(hallDtos, "Hall_ID", "Hall_Name");
            ViewBag.UsersData = users;
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Order_ID,User_ID,OrderDate,TotalAmount,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Order_ID,User_ID,OrderDate,TotalAmount,Status")] Order order)
        {
            if (id != order.Order_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Order_ID))
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
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Order_ID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(long id)
        {
            return _context.Orders.Any(e => e.Order_ID == id);
        }
    }
}
