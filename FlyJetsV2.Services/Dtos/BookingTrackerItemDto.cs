using System;

namespace FlyJetsV2.Services.Dtos
{
    public class BookingTrackerItemDto
    {
        public Guid FlightId { get; set; }
        public byte NumPax { get; set; }
        
    }
}
