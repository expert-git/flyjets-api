using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services
{
    public static class Utilities
    {
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var NAUTICAL_MILEAGE_CONVERSION = 0.539957; //converting to nautical miles
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in meter
            var N = d * NAUTICAL_MILEAGE_CONVERSION;
            return Math.Round(N);
        }

        private static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }

    }
}
