using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("EmptyLegs")]
    public class EmptyLeg
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AircraftId { get; set; }

        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        public byte Direction { get; set; }

        public int DepartureAirportId { get; set; }

        [ForeignKey("DepartureAirportId")]
        public LocationTree DepartureAirport { get; set; }

        public int ArrivalAirportId { get; set; }

        [ForeignKey("ArrivalAirportId")]
        public LocationTree ArrivalAirport { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public decimal ExclusiveCost { get; set; }

        public double Distance { get; set; }

        public TimeSpan Duration { get; set; }

        public Guid? UsedByFlightId { get; set; }

        [ForeignKey("UsedByFlightId")]
        public Flight UsedByFlight { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public bool Available { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }
    }
}
