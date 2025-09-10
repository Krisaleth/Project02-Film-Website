using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
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
                query = query.Where(x => x.Users_FullName.Contains(keyWord)); 
            }

            query = sortOrder switch
            {
                "name_asc" => query.OrderBy(u => u.Users_FullName),
                "name_desc" => query.OrderByDescending(u => u.Users_FullName),
                "email_asc" => query.OrderBy(u => u.Users_Email),
                "email_desc" => query.OrderByDescending(u => u.Users_Email),
                "status_asc" => query.OrderBy(u => u.Account.Status),  // đổi tên theo model của bạn
                "status_desc" => query.OrderByDescending(u => u.Account.Status),
                _ => query.OrderBy(u => u.Users_ID) // mặc định
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
                    Users_ID = m.Users_ID,
                    Users_FullName = m.Users_FullName,
                    Users_Email = m.Users_Email,
                    Users_Phone = m.Users_Phone,
                    Account_Status = m.Account.Status,
                    Username = m.Account.UserName,  
                }).ToListAsync();

            var vm = new UserIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItem = total,
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

        // GET: Users/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _ctx.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(m => m.Users_ID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["Account_ID"] = new SelectList(_ctx.Accounts, "Account_ID", "Account_ID");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Users_ID,Users_FullName,Users_Email,Users_Phone,RowsVersion,Account_ID")] User user)
        {
            if (ModelState.IsValid)
            {
                _ctx.Add(user);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Account_ID"] = new SelectList(_ctx.Accounts, "Account_ID", "Account_ID", user.Account_ID);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _ctx.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["Account_ID"] = new SelectList(_ctx.Accounts, "Account_ID", "Account_ID", user.Account_ID);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Users_ID,Users_FullName,Users_Email,Users_Phone,RowsVersion,Account_ID")] User user)
        {
            if (id != user.Users_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ctx.Update(user);
                    await _ctx.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Users_ID))
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
            ViewData["Account_ID"] = new SelectList(_ctx.Accounts, "Account_ID", "Account_ID", user.Account_ID);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _ctx.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(m => m.Users_ID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var user = await _ctx.Users.FindAsync(id);
            if (user != null)
            {
                _ctx.Users.Remove(user);
            }

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(long id)
        {
            return _ctx.Users.Any(e => e.Users_ID == id);
        }
    }
}
