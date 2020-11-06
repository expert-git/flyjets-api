using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftTypes")]
    public class AircraftType
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string Name { get; set; }

        public short Code { get; set; }
    }
}
