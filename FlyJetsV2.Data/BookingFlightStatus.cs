using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("BookingFlightStatuses")]
    public class BookingFlightStatus
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookingFlightId { get; set; }

        [ForeignKey("BookingFlightId")]
        public BookingFlight BookingFlight { get; set; }

        public byte Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "NVARCHAR(250)")]
        public string Params { get; set; }

        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }
    }
}
