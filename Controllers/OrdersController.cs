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
                        hall.Hall_ID,
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
                s.Showtime_ID,
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
        [HttpPost("/admin/order/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateVm vm)
        {
            if (!ModelState.IsValid)
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
                            hall.Hall_ID,
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
                    s.Showtime_ID,
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo Order mới
                var order = new Order
                {
                    User_ID = vm.User_ID,
                    Showtime_ID = vm.Showtime_ID,
                    OrderDate = DateTime.Now,
                    TotalAmount = vm.TotalAmount,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Lấy danh sách seat đã chọn, dạng string "1,2,3", tách ra
                var seatIdsStr = vm.SelectedSeats; // Ví dụ: "1,2,3"
                var seatIds = seatIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => long.Parse(s))
                                       .ToList();

                // Lấy giá từng seat từ db (giả sử SeatPrice trong table Seats)
                var seats = await _context.Seats.Where(s => seatIds.Contains(s.Seat_ID)).ToListAsync();

                // Tạo các OrderSeat
                var orderSeats = seats.Select(seat => new OrderSeat
                {
                    Order_ID = order.Order_ID,
                    Seat_ID = seat.Seat_ID,
                    Price = seat.SeatPrice
                }).ToList();

                _context.OrderSeats.AddRange(orderSeats);
                await _context.SaveChangesAsync();

                foreach (var seat in seats)
                {
                    seat.SeatStatus = "Booked";
                }
                await _context.SaveChangesAsync();

                var tickets = orderSeats.Select(os => new Ticket
                {
                    OrderSeat_ID = os.OrderSeat_ID,
                    Showtime_ID = order.Showtime_ID,
                    Status = "Available",
                    BookingTime = DateTime.Now
                }).ToList();

                _context.Tickets.AddRange(tickets);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Lỗi khi tạo order: " + ex.Message);
                // Tải lại ViewBag nếu cần
                return View(vm);
            }
        }

        // GET: Orders/Edit/5
        [HttpGet("/admin/order/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute]long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
            .Include(o => o.Showtime)
                .ThenInclude(s => s.Hall)
                .ThenInclude(h => h.Cinema)
            .FirstOrDefaultAsync(o => o.Order_ID == id);

            var selectedSeatIds = await _context.OrderSeats
                .Where(os => os.Order_ID == id)
                .Select(os => os.Seat_ID)
                .ToListAsync();

            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderEditVm
            {
                Order_ID = order.Order_ID,
                User_ID = order.User_ID,
                Showtime_ID = order.Showtime_ID,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Showtime = order.Showtime,
                SelectedSeatIds = selectedSeatIds,
            };

            var users = await _context.Users
                .Select(u => new { u.User_ID, u.User_Name, u.User_Phone, u.User_Email })
                .ToListAsync();
            var halls = await _context.Halls
                        .Include(h => h.Cinema)
                        .OrderBy(h => h.Cinema_ID)
                        .ThenBy(h => h.Hall_ID)
                        .ToListAsync();

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

            var hallDtos = halls
                .GroupBy(h => h.Cinema_ID)
                .SelectMany(group =>
                    group.Select((hall, idx) => new
                    {
                        hall.Hall_ID,
                        Hall_Name = hall.Cinema.Cinema_Name + " - phòng " + hallIdToCount[hall.Hall_ID],
                    })
                ).ToList();

            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ThenInclude(s => s.Cinema)
                .ToListAsync();

            var finalShowtimeList = showtimes.Select(s => new
            {
                s.Showtime_ID,
                Showtime_Name = s.Movie.Movie_Name
                    + " | " + s.Hall.Cinema.Cinema_Name
                    + " | Phòng " + hallIdToCount[s.Hall_ID]
                    + " | " + s.Start_Time.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            ViewBag.SelectedSeats = selectedSeatIds;
            ViewBag.Showtimes = new SelectList(finalShowtimeList, "Showtime_ID", "Showtime_Name");
            ViewBag.Users = new SelectList(users, "User_ID", "User_Name");
            ViewBag.Halls = new SelectList(hallDtos, "Hall_ID", "Hall_Name");
            ViewBag.UsersData = users;
            ViewBag.HallIdToOrder = hallIdToCount;
            return View(vm);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/order/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute]long id, OrderEditVm model)
        {
            if (!ModelState.IsValid)
            {
                // Load đầy đủ dữ liệu cho các dropdown và viewbags để view hiển thị đúng
                var users = await _context.Users
                    .Select(u => new { u.User_ID, u.User_Name, u.User_Phone, u.User_Email })
                    .ToListAsync();
                var halls = await _context.Halls
                    .Include(h => h.Cinema)
                    .OrderBy(h => h.Cinema_ID)
                    .ThenBy(h => h.Hall_ID)
                    .ToListAsync();

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

                var hallDtos = halls
                    .GroupBy(h => h.Cinema_ID)
                    .SelectMany(group =>
                        group.Select((hall, idx) => new
                        {
                            hall.Hall_ID,
                            Hall_Name = hall.Cinema.Cinema_Name + " - phòng " + hallIdToCount[hall.Hall_ID],
                        })
                    ).ToList();

                var showtimes = await _context.Showtimes
                    .Include(s => s.Movie)
                    .Include(s => s.Hall)
                    .ThenInclude(s => s.Cinema)
                    .ToListAsync();

                var finalShowtimeList = showtimes.Select(s => new
                {
                    s.Showtime_ID,
                    Showtime_Name = s.Movie.Movie_Name
                        + " | " + s.Hall.Cinema.Cinema_Name
                        + " | Phòng " + hallIdToCount[s.Hall_ID]
                        + " | " + s.Start_Time.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

                ViewBag.Showtimes = new SelectList(finalShowtimeList, "Showtime_ID", "Showtime_Name");
                ViewBag.Users = new SelectList(users, "User_ID", "User_Name");
                ViewBag.Halls = new SelectList(hallDtos, "Hall_ID", "Hall_Name");
                ViewBag.UsersData = users;
                ViewBag.HallIdToOrder = hallIdToCount;

                // Trả lại model hiện tại để view giữ nguyên dữ liệu người dùng đã nhập
                return View(model);
            }

            // Lấy danh sách các OrderSeat cũ cùng Seat
            var oldOrderSeats = await _context.OrderSeats
                .Where(os => os.Order_ID == model.Order_ID)
                .Include(os => os.Seat)
                .ToListAsync();

            // Lấy ID OrderSeat cũ
            var oldOrderSeatIds = oldOrderSeats.Select(os => os.OrderSeat_ID).ToList();

            // Xóa các Ticket liên quan
            var oldTickets = await _context.Tickets
                .Where(t => oldOrderSeatIds.Contains(t.OrderSeat_ID))
                .ToListAsync();
            _context.Tickets.RemoveRange(oldTickets);
            await _context.SaveChangesAsync();

            // Đặt lại trạng thái ghế cũ về "Available"
            foreach (var os in oldOrderSeats)
            {
                os.Seat.SeatStatus = "Available";
            }
            await _context.SaveChangesAsync();

            // Xóa các OrderSeat cũ
            _context.OrderSeats.RemoveRange(oldOrderSeats);
            await _context.SaveChangesAsync();

            // Lấy seatIds mới và tạo OrderSeat mới, cập nhật trạng thái ghế Booked
            var seatIds = model.SelectedSeats?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(s => long.TryParse(s, out _))
                .Select(long.Parse)
                .ToList() ?? new List<long>();

            var seats = await _context.Seats.Where(s => seatIds.Contains(s.Seat_ID)).ToListAsync();

            foreach (var seat in seats)
            {
                seat.SeatStatus = "Booked";

                var orderSeat = new OrderSeat
                {
                    Order_ID = model.Order_ID,
                    Seat_ID = seat.Seat_ID,
                    Price = seat.SeatPrice,
                    Status = "Booked"
                };
                _context.OrderSeats.Add(orderSeat);
            }

            // Cập nhật order và lưu thay đổi
            var order = await _context.Orders.FindAsync(model.Order_ID);
            order.User_ID = model.User_ID;
            order.Showtime_ID = model.Showtime_ID;
            order.TotalAmount = model.TotalAmount;
            order.Status = model.Status;

            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

        [HttpPost("/admin/order/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { ok = false, message = "Đơn hàng không tồn tại." });
            }

            try
            {
                var orderSeats = await _context.OrderSeats
                    .Where(os => os.Order_ID == id)
                    .Include(os => os.Seat)
                    .ToListAsync();

                // Chỉnh trạng thái ghế về "available"
                foreach (var os in orderSeats)
                {
                    os.Seat.SeatStatus = "Available";
                }
                await _context.SaveChangesAsync();

                var orderSeatIds = orderSeats.Select(os => os.OrderSeat_ID).ToList();
                var tickets = await _context.Tickets.Where(t => orderSeatIds.Contains(t.OrderSeat_ID)).ToListAsync();
                _context.Tickets.RemoveRange(tickets);

                _context.OrderSeats.RemoveRange(orderSeats);
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "Không thể xoá đơn hàng: " + ex.Message });
            }
        }


    }
}