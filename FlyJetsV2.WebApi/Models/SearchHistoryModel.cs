using System;
using System.Text;

namespace FlyJetsV2.WebApi.Models
{
  public class SearchHistoryModel
  {
    public int DepartureId { get; set; }

    public int ArrivalId { get; set; }

    public DateTime DepartureDate { get; set; }

    public DateTime? ArrivalDate { get; set; }

    public byte BookingType { get; set; }
    
    public int Passengers { get; set; }

  }
}
