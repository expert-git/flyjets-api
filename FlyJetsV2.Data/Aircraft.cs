using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Aircrafts")]
    public class Aircraft
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "VARCHAR(10)")]
        public string TailNumber { get; set; }

        public Guid TypeId { get; set; }

        [ForeignKey("TypeId")]
        public AircraftType Type { get; set; }

        public Guid ModelId { get; set; }

        [ForeignKey("ModelId")]
        public AircraftModel Model { get; set; }

        public int HomeBaseId { get; set; }

        [ForeignKey("HomeBaseId")]
        public LocationTree HomeBase { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string ArgusSafetyRating { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string WyvernSafetyRating { get; set; }

        public short? ManufactureYear { get; set; }

        public short? LastIntRefurbish { get; set; }

        public short? LastExtRefurbish { get; set; }

        public byte MaxPassengers { get; set; }

        public short? HoursFlown { get; set; }

        public short Speed { get; set; }

        public short Range { get; set; }

        public bool WiFi { get; set; }

        public bool Television { get; set; }

        public bool BookableDemo {get; set;}

        public short? NumberOfTelevision { get; set; }

        public short? CargoCapability { get; set; }

        public Guid ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public Account Provider { get; set; }

        public bool SellAsCharterAircraft { get; set; }

        public bool SellAsCharterSeat { get; set; }

        public decimal PricePerHour { get; set; }

        public short NumberOfSeats { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public bool Available { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }

        public ICollection<AircraftDocument> Documents { get; set; }

        public ICollection<AircraftImage> Images { get; set; }
    }
}
