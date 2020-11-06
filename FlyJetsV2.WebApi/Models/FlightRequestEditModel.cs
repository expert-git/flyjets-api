using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class FligthRequestEditModel
    {
        public byte Direction { get; set; }
    
        public byte BookingType { get; set; }

        public int DepartureId { get; set; }

        public int ArrivalId { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public int Pax { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string Notes { get; set; }

        public byte? AircraftType { get; set; }
    }
}
