using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Seat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class SeatsController : Controller
    {
        private readonly AppDbContext _context;

        private void PopulateDropdowns()
        {
            var halls = _context.Halls
                    .Include(h => h.Cinema)
                    .OrderBy(h => h.Cinema.Cinema_Name)
                    .ThenBy(h => h.Hall_ID)
                    .ToList();

            var hallList = new List<SelectListItem>();

            string currentCinema = null;
            int hallNumber = 0;

            foreach (var hall in halls)
            {
                if (currentCinema != hall.Cinema.Cinema_Name)
                {
                    currentCinema = hall.Cinema.Cinema_Name;
                    hallNumber = 1;
                }
                else
                {
                    hallNumber++;
                }

                hallList.Add(new SelectListItem
                {
                    Value = hall.Hall_ID.ToString(),
                    Text = $"{currentCinema} - Hall {hallNumber}"
                });
            }

            ViewBag.Hall_ID = hallList;


            ViewBag.SeatType = new SelectList(new[]
            {
                new { Value = "Normal", Text = "Normal" },
                new { Value = "VIP", Text = "VIP" },
                new { Value = "Couple", Text = "Couple" }
            }, "Value", "Text");

            ViewBag.SeatStatus = new SelectList(new[]
            {
                new { Value = "Available", Text = "Available" },
                new { Value = "Booked", Text = "Booked" },
                new { Value = "Broken", Text = "Broken" }
            }, "Value", "Text");
        }

        public SeatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/seat")]
        public async Task<IActionResult> Index(long? hallId)
        {
            var cinemasWithHalls = _context.Cinemas
                .Select(cinema => new
                {
                    Cinema = cinema,
                    Halls = _context.Halls
                        .Where(h => h.Cinema_ID == cinema.Cinema_ID)
                        .OrderBy(h => h.Hall_ID)
                        .ToList()
                })
                .ToList();

            var hallOptions = new List<SelectListItem>();
            var hallNumberDict = new Dictionary<long, int>();

            foreach (var item in cinemasWithHalls)
            {
                var halls = item.Halls;
                for (int i = 0; i < halls.Count; i++)
                {
                    hallNumberDict[halls[i].Hall_ID] = i + 1;
                    hallOptions.Add(new SelectListItem
                    {
                        Value = halls[i].Hall_ID.ToString(),
                        Text = halls.Count == 1
                            ? $"{item.Cinema.Cinema_Name} - Hall 1"
                            : $"{item.Cinema.Cinema_Name} - Hall {i + 1}"
                    });
                }
            }

            long selectedHallId = hallId ?? (hallOptions.Any() ? long.Parse(hallOptions[0].Value) : 0);

            IQueryable<Seat> seatsQuery = _context.Seats.Include(s => s.Hall).ThenInclude(h => h.Cinema).AsNoTracking();

            seatsQuery = seatsQuery.Where(s => s.Hall_ID == selectedHallId);

            var seatsList = await seatsQuery.ToListAsync();

            var seats = seatsList.Select(s => new SeatStatusVm
            {
                Seat_ID = s.Seat_ID,
                Hall_ID = s.Hall_ID,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                SeatType = s.SeatType,
                Description = s.Description,
                Cinema_Name = s.Hall.Cinema.Cinema_Name,
                Status = s.SeatStatus,
                HallNumber = hallNumberDict.ContainsKey(s.Hall_ID) ? hallNumberDict[s.Hall_ID] : 0
            }).ToList();

            var vm = new SeatIndexVm
            {
                Items = seats,
                selectedHallId = selectedHallId,
                hallOptions = hallOptions,
            };

            return View(vm);
        }

        [HttpGet("/admin/seat/details/{id}")]
        public async Task<IActionResult> Details([FromRoute] long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var seat = await _context.Seats
                .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(m => m.Seat_ID == id);
            if (seat == null)
            {
                return NotFound();
            }
            var vm = new SeatDetailVm
            {
                Seat_ID = seat.Seat_ID,
                Hall_ID = seat.Hall_ID,
                RowNumber = seat.RowNumber,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType,
                Description = seat.Description,
                Price = seat.SeatPrice,
                Cinema_Name = seat.Hall.Cinema.Cinema_Name,
                Status = seat.SeatStatus,
            };
            return View(vm);
        }

        [HttpGet("/admin/seat/create")]
        public IActionResult Create()
        {
            var hallList = _context.Halls
                .Include(h => h.Cinema)
                .Select(h => new SelectListItem
                {
                    Value = h.Hall_ID.ToString(),
                    Text = $"{h.Cinema.Cinema_Name} - Hall {h.Hall_ID}"
                }).ToList();
            ViewBag.Hall_ID = hallList;
            ViewBag.SeatType = new SelectList(new[]
            {
                new { Value = "Normal", Text = "Normal" },
                new { Value = "VIP", Text = "VIP" },
                new { Value = "Couple", Text = "Couple" }
            }, "Value", "Text");

            return View();
        }

        [HttpPost("/admin/seat/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeatCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                var hallList = _context.Halls
                .Include(h => h.Cinema)
                .Select(h => new SelectListItem
                {
                    Value = h.Hall_ID.ToString(),
                    Text = $"{h.Cinema.Cinema_Name} - Hall {h.Hall_ID}"
                }).ToList();
                ViewBag.SeatType = new SelectList(new[]
                {
                    new { Value = "Normal", Text = "Normal" },
                    new { Value = "VIP", Text = "VIP" },
                    new { Value = "Couple", Text = "Couple" }
                }, "Value", "Text");
                ViewBag.Hall_ID = hallList;
                return View(vm);
            }

            // Lấy số lượng ghế hiện có trong hall
            int currentSeatCount = await _context.Seats
                .CountAsync(s => s.Hall_ID == vm.Hall_ID);

            // Lấy capacity phòng hall
            int hallCapacity = await _context.Halls
                .Where(h => h.Hall_ID == vm.Hall_ID)
                .Select(h => h.Capacity)
                .FirstOrDefaultAsync();

            // Kiểm tra nếu số lượng ghế đã đạt capacity
            if (currentSeatCount >= hallCapacity)
            {
                ModelState.AddModelError("", "Không thể thêm ghế mới. Số lượng ghế đã đạt giới hạn capacity của phòng.");
                PopulateDropdowns();
                return View(vm);
            }

            bool seatExists = await _context.Seats.AnyAsync(s =>
            s.Hall_ID == vm.Hall_ID &&
            s.RowNumber == vm.RowNumber &&
            s.SeatNumber == vm.SeatNumber);

            if (seatExists)
            {
                ModelState.AddModelError("", "Seat with this Row Number and Seat Number already exists in this hall.");

                // Tạo lại dropdown trước khi trả về view
                var hallList = _context.Halls
                    .Include(h => h.Cinema)
                    .Select(h => new SelectListItem
                    {
                        Value = h.Hall_ID.ToString(),
                        Text = $"{h.Cinema.Cinema_Name} - Hall {h.Hall_ID}"
                    }).ToList();
                ViewBag.SeatType = new SelectList(new[]
                {
                new { Value = "Normal", Text = "Normal" },
                new { Value = "VIP", Text = "VIP" },
                new { Value = "Couple", Text = "Couple" }
                }, "Value", "Text");
                    ViewBag.Hall_ID = hallList;
                    return View(vm);
                }

            var seat = new Seat
            {
                Hall_ID = vm.Hall_ID,
                RowNumber = vm.RowNumber,
                SeatNumber = vm.SeatNumber,
                SeatType = vm.SeatType,
                SeatPrice = vm.SeatType switch
                {
                    "Normal" => 60000m,
                    "VIP" => 65000m,
                    "Couple" => 95000m,
                    _ => 60000m
                },
                Description = vm.Description,
                SeatStatus = "Available",
            };

            _context.Add(seat);
            await _context.SaveChangesAsync();
            TempData["NotificationType"] = "success";
            TempData["NotificationTitle"] = "Thành công";
            TempData["NotificationMessage"] = "Thêm mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/admin/seat/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id)
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
            var vm = new SeatEditVm
            {
                Seat_ID = seat.Seat_ID,
                Hall_ID = seat.Hall_ID,
                RowNumber = seat.RowNumber,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType,
                Description = seat.Description,
                Status = seat.SeatStatus,
            };
            PopulateDropdowns();
            return View(vm);                    
        }

        [HttpPost("/admin/seat/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute]long id, SeatEditVm vm)
        {
            if (id != vm.Seat_ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage)
                                  .ToList();

                // Debug hoặc log các lỗi
                foreach (var error in errors)
                {
                    Debug.WriteLine(error);
                }
                PopulateDropdowns();
                return View(vm);
            }

            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }
            seat.Hall_ID = vm.Hall_ID;
            seat.RowNumber = vm.RowNumber;
            seat.SeatNumber = vm.SeatNumber;
            seat.SeatType = vm.SeatType;
            seat.SeatPrice = vm.SeatType switch
            {
                "Normal" => 60000m,
                "VIP" => 65000m,
                "Couple" => 95000m,
                _ => 60000m
            };
            seat.Description = vm.Description;
            seat.SeatStatus = vm.Status;
            
            _context.Update(seat);
            await _context.SaveChangesAsync();
            TempData["NotificationType"] = "success";
            TempData["NotificationTitle"] = "Thành công";
            TempData["NotificationMessage"] = "Sửa thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/admin/seat/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute]long id)
        {
            var seat = await _context.Seats.FirstOrDefaultAsync(i => i.Seat_ID == id);
            if (seat != null)
            {
                _context.Seats.Remove(seat);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private bool SeatExists(long id)
        {
            return _context.Seats.Any(e => e.Seat_ID == id);
        }
    }
}
