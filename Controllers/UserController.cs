using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Project02.Data;
using Project02.Helpers;
using Project02.Models;
using Project02.Security;
using Project02.ViewModels;
using Project02.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Policy;

namespace Project02.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _ctx;

        public UserController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet("/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            return View(new LoginViewModel { returnUrl = returnUrl });
        }
        [HttpPost("/login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var user = await _ctx.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserName == vm.UserName);

            if (user == null || user.Status != "Active")
            {
                ModelState.AddModelError(" ", "Tài khoản hoặc mật khẩu không đúng!");
                return View(vm);
            }

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Tài khoản này không được truy cập vào đây!");
                return View(vm);
            }

            if (user.Password_Salt == null || !PasswordHasher.Verify(vm.Password, user.Password_Salt, user.Password_Hash))
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng!");
                return View(vm);
            }

            var userProfile = _ctx.Users.Where(a => a.Account_ID == user.Account_ID).FirstOrDefault();

            if (userProfile == null)
            {
                ModelState.AddModelError("", "Dữ liệu người dùng không hợp lệ.");
                return View(vm);
            }

            // Tạo claim và đăng nhập
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userProfile.User_ID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var identify = new ClaimsIdentity(claims, "UserScheme");
            var principal = new ClaimsPrincipal(identify);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(vm.RememberMe ? 43200 : 60)
            };

            await HttpContext.SignInAsync("UserScheme", principal, authProps);

            if (!string.IsNullOrEmpty(vm.returnUrl) && Url.IsLocalUrl(vm.returnUrl))
            {
                return Redirect(vm.returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/register")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Kiểm tra email đã tồn tại
            if (await _ctx.Users.AnyAsync(a => a.User_Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
                return View(vm);
            }

            // Kiểm tra trùng số điện thoại
            if (await _ctx.Users.AnyAsync(a => a.User_Phone == vm.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại đã được đăng ký.");
                return View(vm);
            }

            if (vm.Password != vm.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Xác nhận mật khẩu không khớp.");
                return View(vm);
            }


            var (hash, salt) = PasswordHasher.HashPassword(vm.Password);

            var acc = new Account
            {
                UserName = vm.UserName,
                Password_Hash = hash,
                Password_Salt = salt,
                Password_Algo = "PBKDF2",
                Password_Iterations = 100000,
                Role = "User",
                Status = "Active"
            };

            _ctx.Accounts.Add(acc);
            await _ctx.SaveChangesAsync();

            var usr = new User
            {
                User_Name = vm.FullName,
                User_Phone = vm.PhoneNumber,
                User_Email = vm.Email,
                Account_ID = acc.Account_ID,
            };

            _ctx.Users.Add(usr);
            await _ctx.SaveChangesAsync();

            return Redirect("/login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // làm rỗng user hiện tại
            return Redirect("/");
        }
        [HttpGet("/profile")]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.User_ID == userId);
            if (user == null) return NotFound();

            var orders = await _ctx.Orders.Include(o => o.Showtime).ThenInclude(s => s.Movie)
                .Include(o => o.Showtime)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                .Include(o => o.OrderSeats)
                    .ThenInclude(os => os.Seat)
                .Where(o => o.User_ID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var seats = await _ctx.OrderSeats
                .Include(os => os.Seat)
                .Include(os => os.Order)
                .Where(os => os.Order.User_ID == userId)
                .ToListAsync();

            var hallsByCinema = orders
            .Select(o => o.Showtime.Hall)
            .GroupBy(h => h.Cinema.Cinema_ID)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(h => h.Hall_ID)
                      .Select((hall, index) => new { hall.Hall_ID, RoomNumber = index + 1 })
                      .ToDictionary(x => x.Hall_ID, x => x.RoomNumber)
            );

            // Truyền hallsByCinema qua ViewBag hoặc ViewModel để sử dụng trong View
            ViewBag.HallsByCinema = hallsByCinema;

            var vm = new UserProfileVm
            {
                User = user,
                Orders = orders,
                Seat = seats
            };

            return View(vm);
        }

        [HttpGet("/profile/edit")]
        public async Task<IActionResult> ProfileEdit()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId))
                return Unauthorized();

            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.User_ID == userId);
            if (user == null)
                return NotFound();

            // Map từ User entity sang ViewModel
            var model = new UserEditVm
            {
                User_ID = user.User_ID,
                User_Name = user.User_Name,
                User_Email = user.User_Email,
                User_Phone = user.User_Phone,
                RowsVersion = user.RowsVersion
            };

            return View(model);
        }

        [HttpPost("/profile/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileEdit(UserEditVm model)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId))
                return Unauthorized();

            if (userId != model.User_ID)
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.User_ID == userId);
            if (user == null)
                return NotFound();

            // Cập nhật từ ViewModel sang entity
            user.User_Name = model.User_Name;
            user.User_Email = model.User_Email;
            user.User_Phone = model.User_Phone;

            // Thay đổi RowsVersion (để chết concurrency)
            user.RowsVersion = model.RowsVersion;

            try
            {
                await _ctx.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Dữ liệu đã bị thay đổi dữ liệu bởi người khác. Vui lòng tải lại và thử lại!");
                return View(model);
            }
            
        }

        [HttpGet("profile/order/{id}")]
        public async Task<IActionResult> OrderDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            long orderId;
            try
            {
                orderId = IdEncodingHelper.DecodeId(id);
            }
            catch
            {
                return BadRequest();
            }

            var cinemaHalls = await _ctx.Cinemas
                    .Include(c => c.Halls)
                    .ToListAsync();

            var hallOrderDict = new Dictionary<long, int>();
            foreach (var cinema in cinemaHalls)
            {
                // Sắp xếp phòng theo Hall_ID hoặc thuộc tính logic
                var sortedHalls = cinema.Halls.OrderBy(h => h.Hall_ID).ToList();
                for (int i = 0; i < sortedHalls.Count; i++)
                {
                    hallOrderDict[sortedHalls[i].Hall_ID] = i + 1; // Số thứ tự phòng trong rạp
                }
            }

            var order = await _ctx.Orders
                .Include(o => o.OrderSeats)
                .ThenInclude(n => n.Seat)
                .Include(o => o.Showtime)
                    .ThenInclude(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Include(o => o.Showtime)
                    .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(o => o.Order_ID == orderId);

            if (order == null)
                return NotFound();

            // Ví dụ tạo trường định dạng trong ViewModel hoặc ViewBag
            var hallNumber = hallOrderDict[order.Showtime.Hall_ID];
            var hallDisplayName = $"{order.Showtime.Hall.Cinema.Cinema_Name} - phòng số {hallNumber}";

            ViewBag.HallDisplayName = hallDisplayName;

            

            return View(order);
        }


    }
}
