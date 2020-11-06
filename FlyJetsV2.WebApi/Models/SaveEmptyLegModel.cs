using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class SaveEmptyLegModel
    {
        public Guid? EmptyLegId { get; set; }

        public Guid AircraftId { get; set; }

        public byte Direction { get; set; }

        public int DepartureAirportId { get; set; }

        public DateTime DepartureDate { get; set; }

        public int ArrivalAirportId { get; set; }

        public DateTime OutboundArrivalDate { get; set; }

        public DateTime? InboundDepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public decimal ExclusiveCost { get; set; }
    }
}
