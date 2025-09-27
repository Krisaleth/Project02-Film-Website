using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project02.Data;
using Project02.Models;
using Project02.ViewModels.Seat;
using Project02.ViewModels.Showtime;

namespace Project02.Controllers
{
    public class ShowtimesController : Controller
    {
        private readonly AppDbContext _context;

        private async Task<SelectList> GetHallSelectListAsync()
        {
            var halls = await _context.Halls
                .Include(h => h.Cinema)
                .OrderBy(h => h.Cinema.Cinema_Name)
                .ThenBy(h => h.Hall_ID)
                .ToListAsync();

            var hallsByCinema = halls.GroupBy(h => h.Cinema_ID);

            var selectListItems = new List<SelectListItem>();

            foreach (var group in hallsByCinema)
            {
                var cinemaName = group.First().Cinema.Cinema_Name;
                var orderedHalls = group.OrderBy(h => h.Hall_ID).ToList();

                for (int i = 0; i < orderedHalls.Count; i++)
                {
                    var hall = orderedHalls[i];
                    var text = $"{cinemaName} - Hall {i + 1}";
                    var value = hall.Hall_ID.ToString();

                    selectListItems.Add(new SelectListItem { Text = text, Value = value });
                }
            }

            return new SelectList(selectListItems, "Value", "Text");
        }


        public ShowtimesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Showtimes
        [HttpGet("/admin/showtime")]
        public async Task<IActionResult> Index(string? search, string sortOrder, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

             IQueryable<Showtime> showtimesQuery = _context.Showtimes
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .AsQueryable();
            if (!string.IsNullOrEmpty(search))
                {
                showtimesQuery = showtimesQuery.Where(s =>
                    s.Movie.Movie_Name.Contains(search));
            }

            showtimesQuery = sortOrder switch
            {
                "movie_desc" => showtimesQuery.OrderByDescending(s => s.Movie.Movie_Name),
                "movie_asc" => showtimesQuery.OrderBy(s => s.Movie.Movie_Name),
                "cinema_desc" => showtimesQuery.OrderByDescending(s => s.Hall.Cinema.Cinema_Name),
                "cinema_asc" => showtimesQuery.OrderBy(s => s.Hall.Cinema.Cinema_Name),
                "starttime_desc" => showtimesQuery.OrderByDescending(s => s.Start_Time),
                "starttime_asc" => showtimesQuery.OrderBy(s => s.Start_Time),
                _ => showtimesQuery.OrderBy(s => s.Showtime_ID),
            };

            var totalItems = await showtimesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var showtimes = await showtimesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShowtimeRowVm
                {
                    Showtime_ID = s.Showtime_ID,
                    Movie_ID = s.Movie_ID,
                    MovieTitle = s.Movie.Movie_Name,
                    CinemaName = s.Hall.Cinema.Cinema_Name,
                    Hall_ID = s.Hall_ID,
                    StartTime = s.Start_Time,
                    EndTime = s.End_Time
                }).ToListAsync();
            var vm = new ShowtimeIndexVm
            {
                Showtimes = showtimes,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                SortOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "movie_asc", Text = "Movie Name Ascending" },
                    new SelectListItem { Value = "movie_desc", Text = "Movie Name Descending" },
                    new SelectListItem { Value = "cinema_asc", Text = "Cinema Name Ascending" },
                    new SelectListItem { Value = "cinema_desc", Text = "Cinema Name Descending" },
                    new SelectListItem { Value = "starttime_asc", Text = "Start Time Ascending" },
                    new SelectListItem { Value = "starttime_desc", Text = "Start Time Descending" }
                },
                SearchString = search,
                SortOrder = sortOrder,
                PageSize = pageSize
            };

            return View(vm);

        }

        // GET: Showtimes/Details/5
        [HttpGet("/admin/showtime/{id}")]
        public async Task<IActionResult> Details([FromRoute]long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(m => m.Movie)
                .Include(s => s.Hall)
                .ThenInclude(c => c.Cinema)
            
            .FirstOrDefaultAsync(s => s.Showtime_ID == id);

            if (showtime == null)
            {
                return NotFound();
            }

            // Lấy hall trong cinema theo thứ tự tăng dần Hall_ID
            var hallsInCinema = await _context.Halls
                .Where(h => h.Cinema_ID == showtime.Hall.Cinema_ID)
                .OrderBy(h => h.Hall_ID)
                .ToListAsync();

            // Tính thứ tự hall
            int order = hallsInCinema.FindIndex(h => h.Hall_ID == showtime.Hall_ID) + 1;

            var vm = new ShowtimeDetailsVm
            {
                Showtime_ID = showtime.Showtime_ID,
                Movie_ID = showtime.Movie_ID,
                MovieTitle = showtime.Movie.Movie_Name,
                CinemaName = showtime.Hall.Cinema.Cinema_Name,
                Language = showtime.Language,
                Format = showtime.Format,
                Hall_ID = showtime.Hall_ID,
                HallName = $"Hall {order}",
                StartTime = showtime.Start_Time,
                EndTime = showtime.End_Time
            };
            return View(vm);
        }

        // GET: Showtimes/Create
        [HttpGet("/admin/showtime/create")]
        public async Task<IActionResult> Create()
        {
            var model = new ShowtimeCreateVm();
            ViewBag.Hall_ID = await GetHallSelectListAsync();
            ViewBag.Movie_ID = new SelectList(_context.Movies, "Movie_ID", "Movie_Name");
            return View(model);
        }

        // POST: Showtimes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/admin/showtime/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowtimeCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                var model = new ShowtimeCreateVm();
                ViewBag.Hall_ID = await GetHallSelectListAsync();
                ViewBag.Movie_ID = new SelectList(_context.Movies, "Movie_ID", "Movie_Name");
                return View(model);

            }
            
            var showtime = new Showtime
            {
                Movie_ID = vm.Movie_ID,
                Hall_ID = vm.Hall_ID,
                Start_Time = vm.StartTime,
                End_Time = vm.EndTime,
                Language = vm.Language,
                Format = vm.Format,
            };

            _context.Add(showtime);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Showtimes/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
            {
                return NotFound();
            }
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // POST: Showtimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Showtime_ID,Movie_ID,Cinema_ID,Hall_ID,Start_Time,End_Time,Language,Format,Price")] Showtime showtime)
        {
            if (id != showtime.Showtime_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(showtime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowtimeExists(showtime.Showtime_ID))
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
            ViewData["Hall_ID"] = new SelectList(_context.Halls, "Hall_ID", "Hall_ID", showtime.Hall_ID);
            ViewData["Movie_ID"] = new SelectList(_context.Movies, "Movie_ID", "Movie_ID", showtime.Movie_ID);
            return View(showtime);
        }

        // GET: Showtimes/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Showtime_ID == id);
            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // POST: Showtimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShowtimeExists(long id)
        {
            return _context.Showtimes.Any(e => e.Showtime_ID == id);
        }
    }
}
