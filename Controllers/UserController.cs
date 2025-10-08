using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Project02.Data;
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
        public async Task<IActionResult> Register()
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

            
            var(hash, salt) = PasswordHasher.HashPassword(vm.Password);

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
    }
}
