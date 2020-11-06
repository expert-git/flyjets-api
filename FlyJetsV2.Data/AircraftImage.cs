using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftImages")]
    public class AircraftImage
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string Name { get; set; }

        public Guid AircraftId { get; set; }

        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string FileName { get; set; }

        public byte Order { get; set; }
    }
}
