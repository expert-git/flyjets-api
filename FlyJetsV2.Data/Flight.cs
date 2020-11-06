using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Flights")]
    public class Flight
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "VARCHAR(10)")]
        public string Number { get; set; }

        public Guid AircraftId { get; set; }

        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        public int DepartureId { get; set; }

        [ForeignKey("DepartureId")]
        public LocationTree Departure { get; set; }

        public int ArrivalId { get; set; }

        [ForeignKey("ArrivalId")]
        public LocationTree Arrival { get; set; }

        public DateTime DepartureDate { get; set; }

        public TimeSpan? DepartureTime { get; set; }

        public DateTime ArrivalDate { get; set; }

        public TimeSpan? ArrivalTime { get; set; }

        public TimeSpan Duration { get; set; }

        public double Distance { get; set; }

        public short NumberOfSeats { get; set; }

        public short NumberOfSeatsAvailable { get; set; }

        public byte FlightType { get; set; }

        public byte Status { get; set; }

        public byte Order { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }

        public virtual ICollection<BookingFlightStatus> StatusHistory { get; set; }
    }
}
