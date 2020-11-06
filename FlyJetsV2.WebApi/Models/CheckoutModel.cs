using System;
using System.Text;

namespace FlyJetsV2.WebApi.Models
{
  public class CheckoutModel
  {
    public Guid BookingId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
  }
}
