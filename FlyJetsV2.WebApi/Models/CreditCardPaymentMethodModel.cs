using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class CreditCardPaymentMethodModel
    {
        public string Number { get; set; }
        public long ExpiryMonth { get; set; }
        public long ExpiryYear { get; set; }
        public string Cvc { get; set; }
        public byte UsedFor { get; set; }
    }
}
