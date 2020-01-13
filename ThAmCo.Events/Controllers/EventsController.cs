using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;


namespace ThAmCo.Events.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsDbContext _context;

        public HttpResponseMessage HttpResponseMessage { get; private set; }

        public EventsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            //Building the query to refine the search when accessing an event through a different page, this limits the events that are displayed.
            var @event = await _context.Events
                .Include(e => e.StaffBookings)
                .ThenInclude(e => e.staff)
                .Include(c => c.Bookings)
                .ThenInclude(c => c.Customer)
                .ToListAsync();
            return View(@event);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //This finds the details for only the event clicked in order to be displayed afterwards.
            var @event = await _context.Events
                .Include(e => e.StaffBookings)
                .ThenInclude(e => e.staff)
                .Include(c => c.Bookings)
                .ThenInclude(c => c.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Date,Duration,TypeId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Finds details of the evnt to be displayed.
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }



        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Title, TimeSpan Duration)
        {
            Event e = await _context.Events.FirstOrDefaultAsync(b => b.Id == Id);
            if (string.IsNullOrEmpty(Title) || Duration == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    //Updates the event with the data passed and set on the view.
                        
                        e.Title = Title;
                        e.Duration = Duration;
                        await _context.SaveChangesAsync();
                        _context.Events.Update(e);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(e.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //Returns to the events index.
                return RedirectToAction(nameof(Index));
            }
            //return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //finds event that is trying to be deleted.
            var @event = await _context.Events.FindAsync(id);


            if (@event.reservations != null)
            {
                //Building query to message the venues database.
                HttpClient clientWhenDeletingFirst = new HttpClient();
                var RequestBuilder = new UriBuilder("http://localhost");
                RequestBuilder.Port = 23652;
                RequestBuilder.Path = "api/Reservations/" + @event.reservations;
                String url = RequestBuilder.ToString();

                //Checks if query was recieved.
                clientWhenDeletingFirst.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                HttpResponseMessage responseWhenDeleting = await clientWhenDeletingFirst.DeleteAsync(url);

                if (!responseWhenDeleting.IsSuccessStatusCode)
                {
                    //If unsuccessful it then it returns an error and returns to the events index.
                    ModelState.AddModelError("", "Previous reservation could not be removed.");
                    return RedirectToAction(nameof(Index), "Events");
                }

                //If successful it then deletes the booking 
                @event.reservations = null;
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            //Removes any staff bookings that are related to the event freeing up staff.
            var @event2 = await _context.StaffBookings.FindAsync(id);
            _context.StaffBookings.RemoveRange(_context.StaffBookings.Where(s => s.EventId == id));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        // GET: Events/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            //finds event that is trying to be deleted.
            var @event = await _context.Events.FindAsync(id);


            if (@event.reservations != null)
            {
                //Building query to message the venues database.
                HttpClient clientWhenDeletingFirst = new HttpClient();
                var RequestBuilder = new UriBuilder("http://localhost");
                RequestBuilder.Port = 23652;
                RequestBuilder.Path = "api/Reservations/" + @event.reservations;
                String url = RequestBuilder.ToString();

                //Checks if query was recieved.
                clientWhenDeletingFirst.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                HttpResponseMessage responseWhenDeleting = await clientWhenDeletingFirst.DeleteAsync(url);

                if (!responseWhenDeleting.IsSuccessStatusCode)
                {
                    //If unsuccessful it then it returns an error and returns to the events index.
                    ModelState.AddModelError("", "Previous reservation could not be removed.");
                    return RedirectToAction(nameof(Index), "Events");
                }

                //If successful it then deletes the booking 
                @event.reservations = null;
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }

           
            await _context.SaveChangesAsync();
            //Removes any staff bookings that are related to the event freeing up staff.
            var @event2 = await _context.StaffBookings.FindAsync(id);
            _context.StaffBookings.RemoveRange(_context.StaffBookings.Where(s => s.EventId == id));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
