using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftDocuments")]
    public class AircraftDocument
    {
        [Key]
        public Guid Id { get; set; }

        public byte Type { get; set; }

        public Guid AircraftId { get; set; }

        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        [Column(TypeName = "NVARCHAR(150)")]
        public string Name { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string FileName { get; set; }
    }
}
