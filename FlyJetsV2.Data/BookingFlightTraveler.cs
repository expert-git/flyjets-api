using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("BookingFlightTravelers")]
    public class BookingFlightTraveler
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookingFlightId { get; set; }

        [ForeignKey("BookingFlightId")]
        public BookingFlight BookingFlight { get; set; }

        public Guid? FlyerId { get; set; }

        public Account Flyer { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
