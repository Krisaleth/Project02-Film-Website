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
            await HttpContext.SignOutAsync("AdminScheme");
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return View(new LoginViewModel { returnUrl = returnUrl });
        }

        [HttpPost("/login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { ok = false, message = "Dữ liệu không hợp lệ" });
            }

            var user = await _ctx.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserName == vm.UserName);
            if (user == null || user.Status != "Active")
            {
                return Json(new { ok = false, message = "Tài khoản hoặc mật khẩu không đúng" });
            }

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { ok = false, message = "Tài khoản hoặc mật khẩu không đúng" });
            }

            if (user.Password_Salt == null || !PasswordHasher.Verify(vm.Password, user.Password_Salt, user.Password_Hash))
            {
                return Json(new { ok = false, message = "Tài khoản hoặc mật khẩu không đúng" });
            }

            var userProfile = _ctx.Users.FirstOrDefault(a => a.Account_ID == user.Account_ID);
            if (userProfile == null)
            {
                return Json(new { ok = false, message = "Tài khoản hoặc mật khẩu không đúng" });
            }

            await HttpContext.SignOutAsync("UserScheme");
            await HttpContext.SignOutAsync("AdminScheme");
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

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

            // Nếu có ReturnUrl hợp lệ, trả về url qua json
            if (!string.IsNullOrEmpty(vm.returnUrl) && Url.IsLocalUrl(vm.returnUrl))
            {
                return Json(new { ok = true, redirectUrl = vm.returnUrl });
            }

            return Json(new { ok = true, redirectUrl = Url.Action("Index", "Home") });
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
                return Json(new { ok = false, message = "Dữ liệu không hợp lệ" });

            if (await _ctx.Accounts.AnyAsync(a => a.UserName == vm.UserName))
                return Json(new { ok = false, message = "Tên đăng nhập đã tồn tại!" });

            if (await _ctx.Users.AnyAsync(a => a.User_Email == vm.Email))
                return Json(new { ok = false, message = "Email đã tồn tại!" });

            if (await _ctx.Users.AnyAsync(a => a.User_Phone == vm.PhoneNumber))
                return Json(new { ok = false, message = "SĐT đã tồn tại!" });

            if (vm.Password != vm.ConfirmPassword)
                return Json(new { ok = false, message = "Mật khẩu không khớp!" });

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

            // Trả success json, bạn có thể trả url khác để redirect nếu cần
            return Json(new { ok = true, message = "Đăng ký thành công!", redirectUrl = Url.Action("Login", "User") });
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // làm rỗng user hiện tại
            TempData["NotificationType"] = "success";
            TempData["NotificationTitle"] = "Thành công";
            TempData["NotificationMessage"] = "Đăng xuất thành công!";
            return Redirect("/");
        }

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
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

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
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

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
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
                TempData["NotificationType"] = "success";
                TempData["NotificationTitle"] = "Thành công";
                TempData["NotificationMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["NotificationType"] = "error";
                TempData["NotificationTitle"] = "Thất bại";
                TempData["NotificationMessage"] = "Thông tin đã bị thay đổi trước đó, vui lòng thử lại!";
                return View(model);
            }

        }

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
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

            var hallNumber = hallOrderDict[order.Showtime.Hall_ID];
            var hallDisplayName = $"{order.Showtime.Hall.Cinema.Cinema_Name} - phòng số {hallNumber}";

            ViewBag.HallDisplayName = hallDisplayName;
            return View(order);
        }

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
        [HttpGet("/change-password")]
        public IActionResult ChangePassword()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();
            return View();
        }

        [Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
        [HttpPost("/change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm vm)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var user = await _ctx.Users.Include(u => u.Account).FirstOrDefaultAsync(u => u.User_ID == userId);
            if (user == null || user.Account == null)
            {
                return NotFound();
            }
            if (vm.NewPassword != vm.ConfirmNewPassword)
            {
                TempData["NotificationType"] = "error";
                TempData["NotificationTitle"] = "Thất bại";
                TempData["NotificationMessage"] = "Mật khẩu không khớp";
                return View(vm);
            }
            if (!PasswordHasher.Verify(vm.CurrentPassword, user.Account.Password_Salt, user.Account.Password_Hash))
            {
                TempData["NotificationType"] = "error";
                TempData["NotificationTitle"] = "Thất bại";
                TempData["NotificationMessage"] = "Mật khẩu cũ không đúng!";
                return View(vm);
            }
            var (newHash, newSalt) = PasswordHasher.HashPassword(vm.NewPassword);
            user.Account.Password_Hash = newHash;
            user.Account.Password_Salt = newSalt;
            user.Account.Password_Algo = "PBKDF2";
            user.Account.Password_Iterations = 100000;
            await _ctx.SaveChangesAsync();
            TempData["NotificationType"] = "success";
            TempData["NotificationTitle"] = "Thành công";
            TempData["NotificationMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}
