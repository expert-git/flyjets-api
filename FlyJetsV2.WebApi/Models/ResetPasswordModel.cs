using System;
using System.Text;

namespace FlyJetsV2.WebApi.Models
{
  public class ResetPasswordModel
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
  }
}
