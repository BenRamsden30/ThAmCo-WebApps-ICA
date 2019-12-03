using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{

    public class ReservationsController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:23652");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync("/api/reservations/");
            response.EnsureSuccessStatusCode();
            IEnumerable<Reservation> venue = await response.Content.ReadAsAsync<IEnumerable<Reservation>>();

            return View(venue);
        }
    }
}