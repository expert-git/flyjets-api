using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services.Dtos
{
    public class AircraftAvailabilityLocationDto
    {
        public Guid? AvailabilityLocationId { get; set; }
        public int LocationTreeId { get; set; }
    }
}
