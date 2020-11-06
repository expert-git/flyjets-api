using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("FlightRequests")]
    public class FlightRequest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(30)")]
        public string Number { get; set; }

        [Required]
        public Guid RequesterId { get; set; }

        [ForeignKey("RequesterId")]
        public Account Requester { get; set; }

        [Required]
        public byte Direction { get; set; }

        [Required]
        public byte BookingType { get; set; }

        [Required]
        public int DepartureId { get; set; }

        [ForeignKey("DepartureId")]
        public LocationTree Departure { get; set; }

        [Required]
        public int ArrivalId { get; set; }

        [ForeignKey("ArrivalId")]
        public LocationTree Arrival { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Required]
        public int PassengersNumber { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        [Column(TypeName = "NVARCHAR(500)")]
        public string Notes { get; set; }

        [Column(TypeName = "Datetime2")]
        public DateTime CreatedOn { get; set; }

        public byte Status { get; set; }

        public byte? AircraftType { get; set; }
    }
}
