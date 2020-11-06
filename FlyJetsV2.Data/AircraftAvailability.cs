using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftsAvailability")]
    public class AircraftAvailability
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AircraftId { get; set; }

        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        public decimal? PricePerHour { get; set; }

        public decimal? MinimumAcceptablePricePerTrip { get; set; }

        public bool SellCharterSeat { get; set; }

        public int? ReroutingRadius { get; set; }

        public bool Available { get; set; }

        public List<AircraftAvailabilityPeriod> Periods { get; set; }

        public List<AircraftAvailabilityLocation> Locations { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }
    }
}
