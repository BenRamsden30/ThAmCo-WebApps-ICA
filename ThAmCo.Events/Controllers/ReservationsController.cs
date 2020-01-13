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
            //Checks if event is valid adn not null.
            var @event = await _context.Events.FirstOrDefaultAsync(a => a.Id == id);
            if(@event == null)
            {
                return BadRequest();
            }
            //Builds the url to find the avlability table in the venues project.
            var client = new HttpClient();
            var RequestBuilder = new UriBuilder("http://localhost");
            RequestBuilder.Port = 23652;
            RequestBuilder.Path = "api/Availability";

            //Builds the query to filter the venues based on the date and the event type.
            var QueryBuilder = HttpUtility.ParseQueryString(RequestBuilder.Query);
            QueryBuilder["eventType"] = @event.TypeId;
            QueryBuilder["beginDate"] = @event.Date.ToString("yyyy/MM/dd HH:mm:ss");
            QueryBuilder["endDate"] = @event.Date.Add(@event.Duration.Value).ToString("yyyy/MM/dd HH:mm:ss");
            RequestBuilder.Query = QueryBuilder.ToString();

            String url = RequestBuilder.ToString();

            //Checks if the desired location has been reached.
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            var response = await client.GetAsync(url);


            if (response.IsSuccessStatusCode)
            {
                //IF a response is accepted then it adds the venues to the view bag to be displayed in the view, i used viewbag as it is easier to populat e adrop down box using this method.
                var avalibleVenues = await response.Content.ReadAsAsync<IEnumerable<Venue>>();

                ViewData["venueList"] = new SelectList(avalibleVenues, "Code", "Name");

                return View(@event);
            }
            return View();
        }

        // GET: Reviews/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View();
        }


        //To actually reserve a venue with check for if a venue is already booked.
        //Done this way as i have the best udnerstanding of this method and i feel it allows for easier checking when attempting to reserve the venue.
        public async Task<ActionResult> Reservations(int id, string reservations)
        {
            //Checks if the vent is valid.
            var @event = await _context.Events.FirstOrDefaultAsync(a => a.Id == id);
            if (@event == null)
            {
                return BadRequest();
            }


            if (@event.reservations != null)
            {
                //Builds the url to find the avlability table in the venues project, this time filtering based on the reservation.
                HttpClient clientWhenDeletingFirst = new HttpClient();
                var RequestBuilder = new UriBuilder("http://localhost");
                RequestBuilder.Port = 23652;
                RequestBuilder.Path = "api/Reservations/" + @event.reservations;
                String url = RequestBuilder.ToString();

                clientWhenDeletingFirst.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                HttpResponseMessage responseWhenDeleting = await clientWhenDeletingFirst.DeleteAsync(url);

                if(!responseWhenDeleting.IsSuccessStatusCode)
                {
                    //If failed then displays an error.
                    ModelState.AddModelError("", "Previous reservation could not be removed.");
                    return RedirectToAction(nameof(Index), "Events");
                }
                //Updates the reservations.
                @event.reservations = null;
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:23652");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            //client.Timeout = TimeSpan.FromSeconds(5);

            //USes the post model using the data passed.
            ReservationPostDto reg = new ReservationPostDto
            {
                EventDate = @event.Date,
                VenueCode = reservations,
                StaffId = "staff"
            };
            //Checks if the response is successful and if it is then it updates the database.
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