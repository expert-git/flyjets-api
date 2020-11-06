using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace FlyJetsV2.Services
{
    public class CustomPrincipal : IPrincipal
    {
        private Guid _id;

        public Guid Id
        {
            get
            {
                return _id;
            }
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AccountTypes Type { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public int? TimeZoneOffset { get; set; }
        public AccountStatuses Status { get; set; }
        public byte? NotificationsChannel { get; set; }

        public IIdentity Identity { get; }

        public bool IsInRole(string role)
        {
            if (Type == AccountTypes.Flyer)
            {
                return string.Compare(role, "Flyer", true) == 0;
            }
            else if (Type == AccountTypes.AircraftProvider)
            {
                return string.Compare(role, "Provider", true) == 0;
            }
            else if (Type == AccountTypes.Admin)
            {
                return string.Compare(role, "Admin", true) == 0;
            }

            return false;
        }

        public CustomPrincipal(Guid accountId)
        {
            _id = accountId;
            Identity = new GenericIdentity(accountId.ToString(), "Custom");
        }
    }
}
