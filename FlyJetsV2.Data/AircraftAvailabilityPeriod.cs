using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftsAvailabilityPeriods")]
    public class AircraftAvailabilityPeriod
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AircraftAvailabilityId { get; set; }

        [ForeignKey("AircraftAvailabilityId")]
        public AircraftAvailability AircraftAvailability { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }
    }
}
