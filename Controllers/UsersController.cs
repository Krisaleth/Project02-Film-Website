using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.User;

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
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            if (page == 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<User> query = _ctx.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                var keyWord = search.Trim();
                query = query.Where(x => x.Users_FullName.Contains(keyWord)); 
            }

            var total = await _ctx.Users.CountAsync();

            var items = await query.OrderBy(m => m.Users_ID)
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
