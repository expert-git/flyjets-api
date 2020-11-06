using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftAvailabilityCache")]
    public class AircraftAvailabilityCacheItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? AircraftAvailabilityId { get; set; }

        public Guid? FlightId { get; set; }

        public Guid DepartureLocationId { get; set; }

        public Guid ArrivalLocationId { get; set; }

        public DateTime AvailableFrom { get; set; }

        public DateTime AvailableTo { get; set; }
    }
}
