using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class AddFamilyMemberModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public DateTime? DateOfBirth { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Mobile { get; set; }
    }
}
