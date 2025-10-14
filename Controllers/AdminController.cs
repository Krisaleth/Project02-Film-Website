using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Security;
using Project02.ViewModels;
using Project02.ViewModels.Admin;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        [HttpGet("/admin")]
        [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]

        public async Task<IActionResult> Index()
        {
            var userCount = await _db.Users.CountAsync();

            var publishedMoviesCount = await _db.Movies.CountAsync(m => m.Movie_Status == "Published");
            var ticketSoldCount = await _db.OrderSeats.Where(os => os.Order.Status == "Completed").CountAsync();

            var totalRevenue = await _db.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var model = new DashboardVm
            {
                UserCount = userCount,
                PublishedMoviesCount = publishedMoviesCount,
                TicketSoldCount = ticketSoldCount,
                TotalRevenue = totalRevenue
            };

            return View(model);
        }

        [HttpGet("/admin/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Kiểm tra User có role Admin không
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (!User.IsInRole("Admin"))
                {
                    await HttpContext.SignOutAsync("UserScheme");
                    return View(new AdminLoginVm { ReturnUrl = returnUrl });
                }
                return Redirect("/admin");
            }
            return View(new AdminLoginVm { ReturnUrl = returnUrl });
        }

        [HttpPost("/admin/login")]
        
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AdminLoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Lấy user
            var user = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserName == vm.UserName);

            // Check user + status
            if (user is null || user.Status != "Active")
            {
                ModelState.AddModelError("", "Tài khoản và mật khẩu không đúng!!");
                return View(vm);
            }

            // Check role = Admin
            if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Tài khoản này không có quyền truy cập vào đây!");
                return View(vm);
            }

            // Check pass
            if (user.Password_Salt is null || !PasswordHasher.Verify(vm.Password, user.Password_Salt, user.Password_Hash))
            {
                ModelState.AddModelError("", "Tài khoản và mật khẩu không đúng!");
                return View(vm);
            }
            // Tạo claim và đăng nhập
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Account_ID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var identify = new ClaimsIdentity(claims, "AdminScheme");
            var principal = new ClaimsPrincipal(identify);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(vm.RememberMe ? 43200 : 60)
            };

            await HttpContext.SignOutAsync("UserScheme");
            await HttpContext.SignOutAsync("AdminScheme");
            await HttpContext.SignInAsync("AdminScheme", principal, authProps);

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            {
                return Redirect(vm.ReturnUrl);
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost("/admin/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync("AdminScheme");
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // làm rỗng user hiện tại
            return Redirect("/admin/login");
        }

    }
}
