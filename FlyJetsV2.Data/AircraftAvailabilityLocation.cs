using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftsAvailabilityLocations")]
    public class AircraftAvailabilityLocation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AircraftAvailabilityId { get; set; }

        [ForeignKey("AircraftAvailabilityId")]
        public AircraftAvailability AircraftAvailability { get; set; }

        public int LocationTreeId { get; set; }

        [ForeignKey("LocationTreeId")]
        public LocationTree Location { get; set; }

        public bool IsForDeparture { get; set; }

        public bool Rerouting { get; set; }
    }
}
