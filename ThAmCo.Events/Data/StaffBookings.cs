using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class StaffBookings
    {
        public int id { get; set; }

        public int StaffId { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; }

        public DateTime Date { get; set; }

    }
}
