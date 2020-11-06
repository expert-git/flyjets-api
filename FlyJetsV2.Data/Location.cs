using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Locations")]
    public class Location
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(120)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(600)")]
        public string DisplayName { get; set; }

        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }

        public double? Distance { get; set; }

        public byte Type { get; set; }

        public string Code1 { get; set; }

        public int CityId { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        public int? StateId { get; set; }

        [ForeignKey("StateId")]
        public State State { get; set; }

        public int CountryId { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; }
    }
}
