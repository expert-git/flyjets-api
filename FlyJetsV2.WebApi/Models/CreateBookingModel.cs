using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class CreateBookingModel
    {
        public byte Direction { get; set; }

        public byte BookingType { get; set; }

        public int DepartureId { get; set; }

        public int ArrivalId { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public byte PassengersNum { get; set; }

        public Guid AircraftAvailabilityId { get; set; }

        public List<CreateBookingTravelerModel> Travelers { get; set; }

        public Guid? PaymentMethodId { get; set; }

        public byte BookingPax { get; set;}
    }

    public class CreateBookingTravelerModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Guid? Id { get; set; }
    }
}
