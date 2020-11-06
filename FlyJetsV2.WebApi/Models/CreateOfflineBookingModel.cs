using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class CreateOfflineBookingModel
    {
        public Guid FlightRequestId { get; set; }
        public Guid AircraftProviderId { get; set; }
        public Guid AircraftId { get; set; }
        public short Pax { get; set; }
        public byte BookingType { get; set; }
        public byte Direction { get; set; }
        public int OutboundFlightDepartureId { get; set; }
        public int OutboundFlightArrivalId { get; set; }
        public DateTime OutboundFlightDepartureDate { get; set; }
        public DateTime OutboundFlightArrivalDate { get; set; }
        public int? InboundFlightDepartureId { get; set; }
        public int? InboundFlightArrivalId { get; set; }
        public DateTime? InboundFlightDepartureDate { get; set; }
        public DateTime? InboundFlightArrivalDate { get; set; }
        public decimal ExclusiveBookingCost { get; set; }
        public byte BookingPax { get; set; }
    }
}