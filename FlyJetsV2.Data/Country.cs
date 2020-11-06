using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Countries")]
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "NVARCHAR(120)")]
        public string Name { get; set; }

        public List<State> States { get; set; }

        public List<City> Cities { get; set; }
    }
}
