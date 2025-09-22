using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Cinema;

namespace Project02.Controllers
{
    public class CinemasController : Controller
    {
        private readonly AppDbContext _context;

        public CinemasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cinemas
        [HttpGet("/admin/cinema")]

        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Cinema> query = _context.Cinemas.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                var keyword = search.Trim();
                query = query.Where(c => c.Cinema_Name.Contains(keyword));
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(c => c.Cinema_Name),
                "location_asc" => query.OrderBy(c => c.Location),
                "location_desc" => query.OrderByDescending(c => c.Location),
                _ => query.OrderBy(c => c.Cinema_Name),
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page > totalPages && totalPages > 0)
                page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CinemaRowVm
                {
                    Cinema_ID = c.Cinema_ID,
                    Cinema_Name = c.Cinema_Name,
                    Location = c.Location,
                    Contact_Info = c.Contact_Info
                })
                .ToListAsync();
            var vm = new CinemaIndexVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                search = search,
                sortOrder = sortOrder,
                SortOptions = new List<SelectListItem>
                {
                    new("Sort by", "", string.IsNullOrEmpty(sortOrder)),
                    new SelectListItem { Value = "name_asc", Text = "Name Ascending" },
                    new SelectListItem { Value = "name_desc", Text = "Name Descending" },
                    new SelectListItem { Value = "location_asc", Text = "Location Ascending" },
                    new SelectListItem { Value = "location_desc", Text = "Location Descending" },
                }
            };

            return View(vm);
        }

        [HttpGet("/admin/cinema/{id}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .AsNoTracking()
                .Where(m => m.Cinema_ID == id)
                .Select(m => new CinemaDetailVm
                {
                    Cinema_ID = m.Cinema_ID,
                    Cinema_Name = m.Cinema_Name,
                    Location = m.Location,
                    Contact_Info = m.Contact_Info
                }).FirstOrDefaultAsync();

            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }

        // GET: Cinemas/Create
        [HttpGet("/admin/cinema/create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cinemas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/cinema/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CinemaCreateVm vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            var cinema = new Cinema
            {
                Cinema_Name = vm.Cinema_Name,
                Location = vm.Location,
                Contact_Info = vm.Contact_Info
            };
            _context.Add(cinema);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cinemas/Edit/5
        [HttpGet("/admin/cinema/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute]long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            var vm = new CinemaEditVm
            {
                Cinema_ID = cinema.Cinema_ID,
                Cinema_Name = cinema.Cinema_Name,
                Location = cinema.Location,
                Contact_Info = cinema.Contact_Info
            };
            return View(vm);
        }

        // POST: Cinemas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/cinema/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute]long id, CinemaEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }

            cinema.Cinema_Name = vm.Cinema_Name;
            cinema.Location = vm.Location;
            cinema.Contact_Info = vm.Contact_Info;

            _context.Update(cinema);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        [HttpPost("/admin/cinema/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var cinema = await _context.Cinemas.FirstOrDefaultAsync(m => m.Cinema_ID == id);
            if (cinema != null)
            {
                _context.Cinemas.Remove(cinema);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        private bool CinemaExists(long id)
        {
            return _context.Cinemas.Any(e => e.Cinema_ID == id);
        }
    }
}
