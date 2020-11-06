using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class SearchCharterAircraftsModel
    {
        public int DepartureId { get; set; }
        public int ArrivalId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public short Pax { get; set; }
        public byte BookingType { get; set; }
        public byte Direction { get; set; }
    }
}
