using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("BookingFlights")]
    public class BookingFlight
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        public Guid FlightId { get; set; }

        [ForeignKey("FlightId")]
        public Flight Flight { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }

        public virtual List<BookingFlightTraveler> Travelers { get; set; }
    }
}
