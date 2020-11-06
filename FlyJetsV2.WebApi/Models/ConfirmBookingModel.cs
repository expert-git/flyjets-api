using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class ConfirmBookingModel
    {
        public List<ConfirmBookingTravelerModel> Travelers { get; set; }
        public Guid PaymentMethodId { get; set; }
    }

    public class ConfirmBookingTravelerModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Guid? Id { get; set; }
    }
}
