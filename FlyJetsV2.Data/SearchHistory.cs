using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
  [Table("SearchHistory")]
  public class SearchHistory
  {
    [Key]
    public Guid Id { get; set; }
    
    public Guid FlyerId { get; set; }

    [ForeignKey("FlyerId")]
    public Account Flyer { get; set; }

    public byte BookingType { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedOn { get; set; }

    public int DepartureId { get; set; }

    [ForeignKey("DepartureId")]
    public LocationTree Departure { get; set; }

    public int ArrivalId { get; set; }

    [ForeignKey("ArrivalId")]
    public LocationTree Arrival { get; set; }

    public DateTime DepartureDate { get; set; }

    public DateTime? ArrivalDate { get; set; }

    public int Passengers { get; set; }
  }
}
