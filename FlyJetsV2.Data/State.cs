﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("States")]
    public class State
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "NVARCHAR(120)")]
        public string Name { get; set; }

        public int CountryId { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; }
    }
}
