using FlyJetsV2.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class SaveAircraftModel
    {
        public Guid? AircraftId { get; set; }

        public string TailNumber { get; set; }

        public Guid TypeId { get; set; }

        public Guid ModelId { get; set; }

        public int HomebaseId { get; set; }

        public string ArgusSafetyRating { get; set; }

        public string WyvernSafetyRating { get; set; }

        public short? ManufactureYear { get; set; }

        public short? LastIntRefurbish { get; set; }

        public short? LastExtRefurbish { get; set; }

        public byte MaxPassengers { get; set; }

        public short? HoursFlown { get; set; }

        public short Speed { get; set; }

        public short Range { get; set; }

        public bool WiFi { get; set; }

        public bool BookableDemo {get; set;}

        public short? NumberOfTelevision { get; set; }

        public short? CargoCapability { get; set; }

        public bool SellAsCharterAircraft { get; set; }

        public bool SellAsCharterSeat { get; set; }

        public decimal PricePerHour { get; set; }

        public List<AircraftDocumentDto> Images { get; set; }
        public List<AircraftDocumentDto> Documents { get; set; }
    }
}
