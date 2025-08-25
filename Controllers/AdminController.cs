using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Project02.Data;
using Project02.ViewModels;
using Project02.Security;

namespace Project02.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        [HttpGet("/admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var username = User.FindFirst(ClaimTypes.Name).Value;
            ViewBag.Username = username;
            return View();
        }
        [HttpGet("/admin/login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/admin");
            }
            return View(new AdminLoginVm { ReturnUrl = returnUrl });
        }
        [ValidateAntiForgeryToken]
        [HttpPost("/admin/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AdminLoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Lấy user
            var user = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.UserName == vm.UserName);

            // Check user + status
            if(user is null || !user.Status)
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

            var identify = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identify);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(vm.RememberMe ? 43200 : 60)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            if(!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            {
                return Redirect(vm.ReturnUrl);
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out cookie scheme -> xoá cookie -> mất claim
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // làm rỗng user hiện tại
            return Redirect("/admin/login");
        }
    }
}
