using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("BookingTaxes")]
    public class BookingTax
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        public Guid TaxTypeId { get; set; }

        [ForeignKey("TaxTypeId")]
        public TaxType TaxType { get; set; }

        public decimal Value { get; set; }
    }
}
