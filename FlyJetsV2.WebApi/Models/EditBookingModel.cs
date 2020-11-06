using System;
using System.Collections.Generic;
using FlyJetsV2.Data;
using FlyJetsV2.Services.Dtos;

namespace FlyJetsV2.WebApi.Models 
{
  public class EditBookingModel
  {
    public string BookingNo { get; set; }

    public List<CreateBookingTravelerDto> Travelers { get; set; }
  }
}
