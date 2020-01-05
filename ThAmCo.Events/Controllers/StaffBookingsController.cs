using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{
    public class StaffBookingsController : Controller
    {
        private readonly EventsDbContext _context;

        public StaffBookingsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: StaffBookings
        public async Task<IActionResult> Index(int? id )
        {
            var eventsDbContext = _context.StaffBookings
                .Include(s => s.Event)
                .AsQueryable();
            if(id.HasValue)
            {
                eventsDbContext = eventsDbContext.Where(s => s.StaffId == id);
            }
            return View(await eventsDbContext.ToListAsync());
        }

        // GET: StaffBookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staffBookings = await _context.StaffBookings
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.id == id);
            if (staffBookings == null)
            {
                return NotFound();
            }

            return View(staffBookings);
        }




        // GET: StaffBookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title");
            ViewData["StaffId"] = new SelectList(_context.Staff, "Id", "Surname");
            return View();
        }

        // POST: StaffBookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,StaffId,EventId,Date")] StaffBookings staffBookings)
        {
            if (ModelState.IsValid)
            {
                _context.Add(staffBookings);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", staffBookings.EventId);
            return View(staffBookings);
        }

        // GET: StaffBookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staffBookings = await _context.StaffBookings.FindAsync(id);
            if (staffBookings == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", staffBookings.EventId);
            return View(staffBookings);
        }

        // POST: StaffBookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,StaffId,EventId,Date")] StaffBookings staffBookings)
        {
            if (id != staffBookings.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    StaffBookings s = await _context.StaffBookings.FindAsync(id);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffBookingsExists(staffBookings.id))
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
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", staffBookings.EventId);
            return View(staffBookings);
        }

        // GET: StaffBookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staffBookings = await _context.StaffBookings
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.id == id);
            if (staffBookings == null)
            {
                return NotFound();
            }

            return View(staffBookings);
        }

        // POST: StaffBookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staffBookings = await _context.StaffBookings.FindAsync(id);
            _context.StaffBookings.Remove(staffBookings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StaffBookingsExists(int id)
        {
            return _context.StaffBookings.Any(e => e.id == id);
        }
    }
}
