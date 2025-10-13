using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.Security;
using Project02.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project02.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _ctx;

        public UsersController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET: Users
        [HttpGet("/admin/user")]
        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page == 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<User> query = _ctx.Users.AsNoTracking();

            

            if (!string.IsNullOrEmpty(search))
            {
                var keyWord = search.Trim();
                query = query.Where(x => x.User_Name.Contains(keyWord)); 
            }

            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(u => u.User_Name),
                "name_desc" => query.OrderByDescending(u => u.User_Name),
                "email_asc" => query.OrderBy(u => u.User_Email),
                "email_desc" => query.OrderByDescending(u => u.User_Email),
                "status_asc" => query.OrderBy(u => u.Account.Status),  // đổi tên theo model của bạn
                "status_desc" => query.OrderByDescending(u => u.Account.Status),
                _ => query.OrderBy(u => u.User_ID) // mặc định
            };

            var total = await query.CountAsync();

            var maxPage = (int)Math.Ceiling((double)total / pageSize);
            if (maxPage == 0) maxPage = 1;
            if (page > maxPage) page = maxPage;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new UserRowVm
                {
                    User_ID = m.User_ID,
                    User_Name = m.User_Name,
                    User_Email = m.User_Email,
                    User_Phone = m.User_Phone,
                    Account_Status = m.Account.Status,
                    Username = m.Account.UserName,
                    Account_Id = m.Account_ID,
                }).ToListAsync();

            var vm = new UserIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                search = search,
                sortOrder = sortOrder
            };

            vm.SortOptions = new List<SelectListItem>
            {
                new("Sort by", "", string.IsNullOrEmpty(vm.sortOrder)),
                new("Name Ascending", "name_asc", vm.sortOrder == "name_asc"),
                new("Name Descending", "name_desc", vm.sortOrder == "name_desc"),
                new("Email Ascending", "email_asc", vm.sortOrder == "email_asc"),
                new("Email Descending", "email_desc", vm.sortOrder == "email_desc"),
                new("Status Ascending", "status_asc", vm.sortOrder == "status_asc"),
                new("Status Descending", "status_desc", vm.sortOrder == "status_desc")
            };

            return View(vm);
        }

        [HttpGet("admin/user/detail/{id}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = await _ctx.Users.Where(a => a.User_ID == id)
                .AsNoTracking()
                .Select(m => new UserDetailVm
                {
                    UserId = m.User_ID,
                    UserName = m.Account.UserName,
                    FullName = m.User_Name,
                    UserEmail = m.User_Email,
                    UserPhone = m.User_Phone,
                    Account_Status = m.Account.Status,
                }).FirstOrDefaultAsync();
            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpGet("/admin/user/create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("admin/user/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateVm vm)
        {
            if (vm == null)
            {
                return NotFound();
            }
            var userNameExists = await _ctx.Accounts.AnyAsync(a => a.UserName == vm.UserName);
            if (userNameExists)
            {
                ModelState.AddModelError(nameof(vm.UserName), "Tên này đã tồn tại");
                return View(vm);
            }
            var emailExists = await _ctx.Users.AnyAsync(a => a.User_Email == vm.User_Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(vm.User_Email), "Email đã tồn tại!");
                return View(vm);
            }

            var (hash, salt) = PasswordHasher.HashPassword(vm.Password);

            var account = new Account
            {
                UserName = vm.UserName,
                Password_Hash = hash,
                Password_Salt = salt,
                Password_Algo = "PBKDF2",
                Password_Iterations = 100000,
                Role = "User",
                Status = "Active"
            };
            _ctx.Accounts.Add(account);
            _ctx.SaveChanges();

            var user = new User
            {
                User_Name = vm.User_Name,
                User_Email = vm.User_Email,
                User_Phone = vm.User_Phone,
                Account_ID = account.Account_ID,
            };
            _ctx.Users.Add(user);
            _ctx.SaveChanges();
             return RedirectToAction("Index");
        }

        [HttpGet("/admin/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute]long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = await _ctx.Users
                .Where(a => a.User_ID == id)
                .Include(a => a.Account)
                .AsNoTracking()
                .Select(m => new UserEditVm
                {
                    User_ID = m.User_ID,
                    UserName = m.Account.UserName,
                    User_Email = m.User_Email,
                    User_Phone = m.User_Phone,
                    User_Name = m.User_Name,
                    Account_Status = m.Account.Status,
                    RowsVersion = m.RowsVersion,
                })
                .FirstOrDefaultAsync();

            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        [HttpPost("/admin/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute]long id, UserEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var user = await _ctx.Users.Include(a => a.Account).FirstOrDefaultAsync(m => m.User_ID == id);
            if (user == null)
            {
                return NotFound();
            }

            user.User_Name = vm.User_Name;
            user.User_Phone = vm.User_Phone;
            user.User_Email = vm.User_Email;
            user.Account.Status = vm.Account_Status;
            user.Account.UserName = vm.UserName;

            _ctx.Entry(user).Property("RowsVersion").OriginalValue = vm.RowsVersion;

            try
            {
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Bản ghi đã được thay đổi trước đó!");
                return View(vm);
            }
        }

        // GET: User/Delete/5

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _ctx.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(m => m.User_ID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, string rowVersionBase64)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(m => m.User_ID == id);

            if (user == null)
            {
                return NotFound();
            }

            if(!string.IsNullOrEmpty(rowVersionBase64))
            {

                var rowVersion = Convert.FromBase64String(rowVersionBase64);
                _ctx.Entry(user).Property("RowsVersion").OriginalValue = rowVersion;
            }
            _ctx.Users.Remove(user);
            _ctx.Accounts.Remove(user.Account);

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { ok = false, message = "Bản ghi đã được thay đổi trước đó!" });
            }
            return Json(new { ok = true });

        }
    }
}
