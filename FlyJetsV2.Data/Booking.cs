using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "VARCHAR(30)")]
        public string Number { get; set; }

        public byte Direction { get; set; }

        public byte BookingType { get; set; }

        public Guid FlyerId { get; set; }

        [ForeignKey("FlyerId")]
        public Account Flyer { get; set; }

        public decimal TotalExclusiveCost { get; set; }

        public decimal TotalFees { get; set; }

        public decimal TotalTaxes { get; set; }

        public byte Status { get; set; }

        public Guid? FlightRequestId { get; set; }

        [ForeignKey("FlightRequestId")]
        public FlightRequest FlightRequest { get; set; }

        public decimal? FlyRewards { get; set; }

        public short? FlyRewardsPoints { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string PaymentReference { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public decimal? AdminOverrideCost { get; set; }

        public bool PriceOverridden { get; set; }

        public bool Confirmed { get; set; }

        public byte? NumPax { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }

        public virtual ICollection<BookingFee> Fees { get; set; }

        public virtual ICollection<BookingTax> Taxes { get; set; }

        public virtual ICollection<BookingFlight> BookingFlights { get; set; }
        public virtual ICollection<BookingStatus> StatusHistory { get; set; }

    }
}
