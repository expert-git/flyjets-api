using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services.Dtos
{
    public class AircraftAvailabilityPeriodDto
    {
        public Guid? AvailabilityPeriodId { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }
    }
}
