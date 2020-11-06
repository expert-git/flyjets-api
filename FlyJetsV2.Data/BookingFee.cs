using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("BookingFees")]
    public class BookingFee
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        public Guid FeeTypeId { get; set; }

        [ForeignKey("FeeTypeId")]
        public FeeType FeeType { get; set; }

        public decimal Value { get; set; }
    }
}
