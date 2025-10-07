using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Hall;

namespace Project02.Controllers
{
    public class HallsController : Controller
    {
        private readonly AppDbContext _context;

        public HallsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Halls
        [HttpGet("/admin/hall")]
        public async Task<IActionResult> Index(string? Q, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            IQueryable<Hall> hallsQuery = _context.Halls.Include(h => h.Cinema).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                hallsQuery = hallsQuery.Where(h => h.Cinema.Cinema_Name.Contains(Q));
            }
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                hallsQuery = sortOrder switch
                {
                    "cinema_asc" => hallsQuery.OrderBy(h => h.Cinema.Cinema_Name),
                    "cinema_desc" => hallsQuery.OrderByDescending(h => h.Cinema.Cinema_Name),
                    "capacity_asc" => hallsQuery.OrderBy(h => h.Capacity),
                    "capacity_desc" => hallsQuery.OrderByDescending(h => h.Capacity),
                    _ => hallsQuery.OrderBy(h => h.Hall_ID),
                };
            }
            else
            {
                hallsQuery = hallsQuery.OrderBy(h => h.Hall_ID);

            }

            var totalItems = await hallsQuery.CountAsync();

            var halls = await hallsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HallRowVm
                {
                    Hall_ID = h.Hall_ID,
                    Cinema_Name = h.Cinema.Cinema_Name,
                    Capacity = h.Capacity
                })
                .ToListAsync();

            var vm = new HallIndexVm
            {
                Items = halls,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Q = Q,
                sortOrder = sortOrder,
                SortOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "cinema_asc", Text = "Cinema Name Ascending" },
                    new SelectListItem { Value = "cinema_desc", Text = "Cinema Name Descending" },
                    new SelectListItem { Value = "capacity_asc", Text = "Capacity Ascending" },
                    new SelectListItem { Value = "capacity_desc", Text = "Capacity Descending" },
                }
            };
            return View(vm);
        }


        // GET: Halls/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .Include(h => h.Cinema)
                .FirstOrDefaultAsync(m => m.Hall_ID == id);
            if (hall == null)
            {
                return NotFound();
            }

            return View(hall);
        }

        // GET: Halls/Create
        [HttpGet("/admin/hall/create")]
        public IActionResult Create()
        {
            ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_Name");
            return View();
        }

        // POST: Halls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/hall/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HallCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var hall = new Hall
            {
                Cinema_ID = vm.Cinema_ID,
                Capacity = vm.Capacity
            };
            _context.Add(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Halls/Edit/5
        [HttpGet("/admin/hall/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute] long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls.Where(h => h.Hall_ID == id).Select(n => new HallEditVm
            {
                Hall_ID = n.Hall_ID,
                Cinema_ID = n.Cinema_ID,
                Capacity = n.Capacity
            }).FirstOrDefaultAsync();

            if (hall == null)
            {
                return NotFound();
            }
            ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_Name", hall.Cinema_ID);
            return View(hall);
        }

        // POST: Halls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/hall/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute]long id, HallEditVm vm)
        {
            if (id != vm.Hall_ID)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Cinema_ID"] = new SelectList(_context.Cinemas, "Cinema_ID", "Cinema_Name", vm.Cinema_ID);
                return View(vm);
            }
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
            {
                return NotFound();
            }
            hall.Cinema_ID = vm.Cinema_ID;
            hall.Capacity = vm.Capacity;

            try {                
                _context.Update(hall);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HallExists(hall.Hall_ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: Halls/Delete/5
        [HttpPost("/admin/hall/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var hall = await _context.Halls.FirstOrDefaultAsync(h => h.Hall_ID == id);
            if (hall != null)
            {
                _context.Halls.Remove(hall);
                
                
            }
            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = "Deleted successfully" });
        }

        private bool HallExists(long id)
        {
            return _context.Halls.Any(e => e.Hall_ID == id);
        }
    }
}
