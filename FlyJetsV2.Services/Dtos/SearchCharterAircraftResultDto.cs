using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services.Dtos
{
    public class SearchCharterAircraftResultDto
    {
        public Guid AircraftAvailabilityId { get; set; }
        public Guid AircraftId { get; set; }
        public int DepartureId { get; set; }
        public string Departure { get; set; }
        public int ArrivalId { get; set; }
        public string Arrival { get; set; }
        public int FlightDurationHours { get; set; }
        public int FlightDurationMinutes { get; set; }
        public string AircraftModel { get; set; }
        public short AircraftPax { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ExclusiveTotalPrice { get; set; }
        public decimal TotalFees { get; set; }
        public decimal TotalTaxes { get; set; }
        public string AircraftType { get; set; }
        public string AircraftArgusSafetyRating { get; set; }
        public int AircraftSpeed { get; set; }
        public int AircraftRange { get; set; }
        public string DefaultImageUrl { get; set; }
        public bool WiFi { get; set; }
        public bool BookableDemo {get; set;}
        public short? NumberOfTelevision { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public byte Direction { get; set; }
        public byte BookingType { get; set; }
        public short Pax { get; set; }
    }
}
