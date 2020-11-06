using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AccountPaymentMethods")]
    public class AccountPaymentMethod
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        public byte PaymentMethod { get; set; }

        public string CreditCardBrand { get; set; }

        public string CreditCardLast4 { get; set; }

        public long CreditCardExpMonth { get; set; }

        public long CreditCardExYear { get; set; }

        public string BankName { get; set; }

        public string RoutingNumber { get; set; }

        public string HolderName { get; set; }

        public string AccountLast4 { get; set; }

        public string TokenId { get; set; }

        public string ReferencePaymentMethodId { get; set; }

        public byte UsedFor { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
