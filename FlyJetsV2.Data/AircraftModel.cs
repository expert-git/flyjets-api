using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AircraftModels")]
    public class AircraftModel
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "NVARCHAR(50)")]
        public string Name { get; set; }

        public Guid AircraftTypeId { get; set; }

        [ForeignKey("AircraftTypeId")]
        public AircraftType AircraftType { get; set; }
    }
}
