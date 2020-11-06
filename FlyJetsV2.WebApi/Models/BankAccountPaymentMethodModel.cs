using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class BankAccountPaymentMethodModel
    {
        public string AccountHolderName { get; set; }
        public string AccountHolderType { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public byte UsedFor { get; set; }
    }
}
