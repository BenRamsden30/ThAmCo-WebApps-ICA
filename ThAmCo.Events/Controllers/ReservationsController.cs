using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;
using ThAmCo.Venues.Models;

namespace ThAmCo.Events.Controllers
{

    public class ReservationsController : Controller
    {
        private readonly  EventsDbContext _context;
        public ReservationsController(EventsDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index(int id)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(a => a.Id == id);
            if(@event == null)
            {
                return BadRequest();
            }

            var client = new HttpClient();
            var RequestBuilder = new UriBuilder("http://localhost");
            RequestBuilder.Port = 23652;
            RequestBuilder.Path = "api/Availability";

            var QueryBuilder = HttpUtility.ParseQueryString(RequestBuilder.Query);
            QueryBuilder["eventType"] = @event.TypeId;
            QueryBuilder["beginDate"] = @event.Date.ToString("yyyy/MM/dd HH:mm:ss");
            QueryBuilder["endDate"] = @event.Date.Add(@event.Duration.Value).ToString("yyyy/MM/dd HH:mm:ss");
            RequestBuilder.Query = QueryBuilder.ToString();

            String url = RequestBuilder.ToString();

            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            var response = await client.GetAsync(url);


            if (response.IsSuccessStatusCode)
            {
                var avalibleVenues = await response.Content.ReadAsAsync<IEnumerable<Venue>>();

                ViewData["venueList"] = new SelectList(avalibleVenues, "Code", "Name");

                return View(@event);
            }
                //var response = await client.GetAsync("/api/Reservations/");
            ////response.EnsureSuccessStatusCode();
            //IEnumerable<Reservation> venue = await response.Content.ReadAsAsync<IEnumerable<Reservation>>();

            return View();
        }

        // GET: Reviews/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View();
        }


        //To actually reserve a venue with check for if a venue is already booked.
        //Done this way as i have the best udnerstanding of this method and i feel it allows for easier checking when attempting to reserve the venue.
        public async Task<ActionResult> Reservations(int id, string venueRef)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(a => a.Id == id);
            if (@event == null)
            {
                return BadRequest();
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:23652");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            //client.Timeout = TimeSpan.FromSeconds(5);

            ReservationPostDto reg = new ReservationPostDto
            {
                EventDate = @event.Date,
                VenueCode = venueRef,
                StaffId = "staff"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("api/Reservations", reg);
            if(response.IsSuccessStatusCode)
            {
                var isSuccess = await response.Content.ReadAsAsync<ReservationGetDto>();
                @event.reservations = isSuccess.Reference;
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), "Events");
        }


    }
}