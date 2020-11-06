using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class Customer
    {
        public string Email { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
