using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("TaxesTypes")]
    public class TaxType
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "NVARCHAR(120)")]
        public string Name { get; set; }

        public decimal Percentage { get; set; }

        public byte TaxableItems { get; set; }

        public byte Order { get; set; }
    }
}
