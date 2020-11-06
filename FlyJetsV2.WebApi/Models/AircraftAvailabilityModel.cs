using FlyJetsV2.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class AircraftAvailabilityModel
    {
        public Guid? AircraftAvailabilityId { get; set; }

        public Guid AircraftId { get; set; }

        public string AircraftTailNumber { get; set; }

        public decimal? PricePerHour { get; set; }

        public decimal? MinimumAcceptablePrice { get; set; }

        public bool SellCharterSeat { get; set; }

        public int? ReroutingRadius { get; set; }

        public List<AircraftAvailabilityLocationDto> DepartureLocations { get; set; }
        public List<AircraftAvailabilityLocationDto> ArrivalLocations { get; set; }
        public List<AircraftAvailabilityPeriodDto> Periods { get; set; }
    }
}
