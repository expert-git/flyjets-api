using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services.Dtos
{
    public class BookingPaymentDto
    {
        public decimal TotalExclusiveCost { get; set; }
        public decimal TotalFeesCost { get; set; }
        public decimal TotalTaxesCost { get; set; }
        public decimal TotalCost { get; set; }
        public bool Paid { get; set; }
    }
}
