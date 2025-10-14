using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Customer;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/booking/{encodedId}")]
        public async Task<IActionResult> SelectSeats(string encodedId)
        {
            byte[] bytes = Convert.FromBase64String(encodedId);
            int showtimeId = BitConverter.ToInt32(bytes, 0);

            var showtime = await _context.Showtimes
            .Include(s => s.Hall)
            .ThenInclude(h => h.Cinema)
            .Include(s => s.Hall)
            .ThenInclude(h => h.Seats)
            .FirstOrDefaultAsync(s => s.Showtime_ID == showtimeId);

            var hallsSorted = await _context.Halls.Where(h => h.Cinema_ID == showtime.Hall.Cinema.Cinema_ID)
                .OrderBy(h => h.Hall_ID).ToListAsync();

            var hallIndex = hallsSorted.FindIndex(h => h.Hall_ID == showtime.Hall.Hall_ID) + 1;

            var vm = new SeatLayoutVm
            {
                ShowtimeId = showtime.Showtime_ID,
                DisplayHallName = $"{showtime.Hall.Cinema.Cinema_Name} - Phòng số {hallIndex}",
                Seats = showtime.Hall.Seats.OrderBy(s => s.RowNumber).ThenBy(s => s.SeatNumber).ToList()
            };

            return View(vm);
        }

        [HttpPost("/confirm-order")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOrder(int selectedSeatsCount, decimal totalPrice, string selectedSeats, long ShowtimeId)
        {
            // Kiểm tra dữ liệu hợp lệ
            if (selectedSeatsCount <= 0 || totalPrice <= 0 || string.IsNullOrEmpty(selectedSeats))
            {
                ModelState.AddModelError("", "Dữ liệu đặt vé không hợp lệ.");
                return RedirectToAction("Booking");
            }

            var seatIds = selectedSeats
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id =>
                {
                    // Chuyển đổi string sang long, nếu lỗi xử lý theo ý
                    bool success = long.TryParse(id.Trim(), out long seatIdLong);
                    if (!success)
                    {
                        // Có thể throw ngoại lệ hoặc loại bỏ id không hợp lệ
                        throw new Exception($"SeatId không hợp lệ: {id}");
                    }
                    return seatIdLong;
                })
                .ToList();

            // Lấy thông tin ghế từ db theo seatIds
            var seats = _context.Seats
                            .Where(s => seatIds.Contains(s.Seat_ID))
                            .Select(s => new SeatInfo
                            {
                                SeatId = s.Seat_ID,
                                RowNumber = s.RowNumber,
                                SeatNumber = s.SeatNumber.ToString()
                            })
                            .ToList();


            var vm = new ConfirmOrderVm
            {
                SelectedSeatsCount = selectedSeatsCount,
                TotalPrice = totalPrice,
                SelectedSeats = seats,
                ShowtimeId = ShowtimeId,
            };

            return View("ConfirmOrder", vm);
        }

        [HttpPost("/payment/checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(long showtimeId, decimal totalAmount, string selectedSeats)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId))
            {
                return Unauthorized();
            }

            if (totalAmount <= 0 || string.IsNullOrEmpty(selectedSeats))
            {
                ModelState.AddModelError("", "Dữ liệu thanh toán không hợp lệ.");
                return RedirectToAction("Booking", "UserMovie");
            }

            var seatIdStrings = selectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var seatIds = new List<long>();

            foreach (var id in seatIdStrings)
            {
                if (long.TryParse(id.Trim(), out long seatIdLong))
                {
                    seatIds.Add(seatIdLong);
                }
                else
                {
                    continue;
                }
            }

            var newOrder = new Order
            {
                User_ID = userId,
                Showtime_ID = showtimeId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            var seats = await _context.Seats.Where(s => seatIds.Contains(s.Seat_ID)).ToListAsync();

            foreach (var seat in seats)
            {
                var orderSeat = new OrderSeat
                {
                    Order_ID = newOrder.Order_ID,
                    Seat_ID = seat.Seat_ID,
                    Price = seat.SeatPrice
                };
                _context.OrderSeats.Add(orderSeat);
            }
            await _context.SaveChangesAsync(); // Save để orderSeat.OrderSeat_ID có giá trị

            // Lấy lại danh sách OrderSeats vừa tạo
            var orderSeats = await _context.OrderSeats.Where(os => os.Order_ID == newOrder.Order_ID).ToListAsync();

            foreach (var orderSeat in orderSeats)
            {
                var ticket = new Ticket
                {
                    OrderSeat_ID = orderSeat.OrderSeat_ID,
                    Showtime_ID = showtimeId,
                    Status = "Available",
                    BookingTime = DateTime.UtcNow,
                };
                _context.Tickets.Add(ticket);

                // Cập nhật SeatStatus
                var seatToUpdate = seats.FirstOrDefault(s => s.Seat_ID == orderSeat.Seat_ID);
                if (seatToUpdate != null)
                {
                    seatToUpdate.SeatStatus = "Booked";
                    _context.Seats.Update(seatToUpdate);
                }
            }

            // Trả về view thông báo thành công
            TempData["NotificationType"] = "success";
            TempData["NotificationTitle"] = "Thành công";
            TempData["NotificationMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("Profile", "User");
        }

    }
}
