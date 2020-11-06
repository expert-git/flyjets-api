using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services.Dtos
{
    public class CreateBookingTravelerDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? Id { get; set; }
    }
}
