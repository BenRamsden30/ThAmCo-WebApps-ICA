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
using ThAmCo.Venues.Data;

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
            return View(await _context.Events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
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

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
            //return RedirectToAction(nameof(Index));
        }



        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Title, TimeSpan Duration, Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(Title) || Duration == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                        Event e = await  _context.Events.FindAsync(id);
                        e.Title = Title;
                        e.Duration = Duration;
                        await _context.SaveChangesAsync();
                        _context.Events.Update(e);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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
            return View(@event);
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
            var @event = await _context.Events.FindAsync(id);
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        // GET: Events/Reservations/5
        public async Task<IActionResult> Reservations(int? id)
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

            HttpClient client = new HttpClient();
            var Builder = new UriBuilder("http://localhost");
            Builder.Port = 23652;
            Builder.Path = "api/Availability";
            var query = HttpUtility.ParseQueryString(Builder.Query);
            query["eventType"] = @event.TypeId;
            query["beginDate"] = @event.Date.ToString("yyyy/MM/dd HH:mm:ss");
            query["endDate"] = @event.Date.Add(@event.Duration.Value).ToString("yyyy/MM/dd HH:mm:ss");

            Builder.Query = query.ToString();
            string url = Builder.ToString();

            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var venue = await response.Content.ReadAsAsync<IEnumerable<Venue>>();
                if(venue.Count() == 0)
                {
                    ModelState.AddModelError("", "No Suitable venues could be found.");
                }

                ViewData["venues"] = new SelectList(venue, "Code", "Name");
                
                
                
                

                return View(@event);
            }


            if (response.IsSuccessStatusCode)
            {
                ViewData["Reservations"] = response;

                return View(@event);
            }

            return View(@event);
            //return RedirectToAction(nameof(Index));
        }



        // POST: Events/Reservations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservations(int id, string TypeId, TimeSpan Duration, Event @event, DateTime date)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(TypeId) || Duration == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    var Builder = new UriBuilder("http://localhost");
                    Builder.Port = 23652;
                    Builder.Path = "api/Availability";

                    var query = HttpUtility.ParseQueryString(Builder.Query);
                    query["eventType"] = TypeId;
                    query["beginDate"] = date.ToString("yyyy/MM/dd HH:mm:ss");
                    query["endDate"] = date.Add(Duration).ToString("yyyy/MM/dd HH:mm:ss");

                    Builder.Query = query.ToString();
                    string url = Builder.ToString();

                    // put the update in here

                    await _context.SaveChangesAsync();

                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                    HttpResponseMessage response = await client.GetAsync(url);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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
            return View(@event);
            //Must save to reservation table, requires: Staff Id, Date made, Event date, Venue code, Avalibility, Reference
            //Must load from find where event type == suitable event type
        }
    }
}
