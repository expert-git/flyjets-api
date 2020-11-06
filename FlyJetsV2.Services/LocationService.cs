using FlyJetsV2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlyJetsV2.Services
{
    public class LocationService
    {
        private IConfiguration _config;

        public LocationService(IConfiguration config)
        {
            _config = config;
        }

        public LocationTree GetLocation(int id)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                var location = dbContext.LocationsTree
                    .Include("City")
                    .FirstOrDefault(loc => loc.Id == id);

                return location;
            }
        }

        public List<LocationTree> GetLocations(string keyword, int? filter, bool hasCoordinate, int top)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.LocationsTree
                    .Where(loc => loc.Name.StartsWith(keyword)
                        && (filter.HasValue == false 
                            ||
                            (
                            (filter & loc.Type) == (int)LocationsTypes.Airport
                            ||
                            (filter & loc.Type) == (int)LocationsTypes.Camp
                            ||
                            (filter & loc.Type) == (int)LocationsTypes.City
                            ||
                            (filter & loc.Type) == (int)LocationsTypes.Country
                            ||
                            (filter & loc.Type) == (int)LocationsTypes.Location
                            ||
                            (filter & loc.Type) == (int)LocationsTypes.State
                            )
                            )
                         && (hasCoordinate == false || (loc.Lat.HasValue && loc.Lng.HasValue)))
                    .OrderBy(loc => loc.Name)
                    .Take(top)
                    .ToList();
            }
        }

        public List<LocationTree> GetLocationsTree(string keyword, int top)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.LocationsTree
                    .Where(loc => loc.Name.StartsWith(keyword))
                    .OrderBy(loc => loc.Name)
                    .ToList();
            }
        }

        public List<LocationTree> GetLocationsWithinXMiles(double latitude, double longitude, double radius, byte locType)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                double cLatitude, cLongitude;

                GetCenterCoordinates(out cLatitude, out cLongitude);

                var dummyAirport = new LocationTree { Lat = latitude, Lng = longitude };

                var center = new { Lat = cLatitude, Lng = cLongitude };
                var dummyLoc = new { dummyAirport.Lat, dummyAirport.Lng };

                dummyAirport.Distance = Utilities.GetDistance(center.Lat, center.Lng, dummyLoc.Lat.Value, dummyLoc.Lng.Value);

                double minDistanceRange = dummyAirport.Distance.Value - radius;

                if (minDistanceRange < 0)
                {
                    minDistanceRange = 0;
                }

                IQueryable<LocationTree> locationsSearchQuery = dbContext.LocationsTree;

                locationsSearchQuery = locationsSearchQuery.Where(m => m.Type == locType
                                                && m.Distance >= minDistanceRange
                                                && m.Distance <= dummyAirport.Distance + radius);

                List<LocationTree> locations = locationsSearchQuery
                    .ToList();

                FillDistancesFromLocation(latitude, longitude, locations);

                List<LocationTree> result = new List<LocationTree>();

                int count = int.Parse(_config["NumberOfNearestLocations"]);

                return locations.Where(loc => loc.Distance <= radius && loc.Distance != 0)
                    .OrderBy(m => m.Distance)
                    .Take(count)
                    .ToList();
            }
        }

        private void FillDistancesFromLocation(double latitude, double longitude, IEnumerable<LocationTree> locations)
        {
            var refernce = new { Lat = latitude, Lng = longitude };

            foreach (LocationTree t in locations)
            {
                var loc = new { t.Lat, t.Lng };

                t.Distance = Utilities.GetDistance(refernce.Lat, refernce.Lng, loc.Lat.Value, loc.Lng.Value);
            }
        }

        private decimal GetLocationDistance(double latitude, double longitude, Location location)
        {
            //var mlat = location.Lat;
            //var mlng = location.Lng;
            //var dLat = Rad(mlat - latitude);
            //var dLong = Rad(mlng - longitude);
            //var a = Math.Sin((double)(dLat / 2)) * Math.Sin((double)(dLat / 2)) +
            //        Math.Cos((double)Rad(latitude)) * Math.Cos((double)Rad(latitude)) * Math.Sin((double)(dLong / 2)) *
            //        Math.Sin((double)(dLong / 2));
            //var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //var d = RadiusOfTheEarth * c;
            //return (decimal)d;

            return 0;
        }

        private void GetCenterCoordinates(out double latitude, out double longitude)
        {
            string[] centerPoint = _config["CenterCoordinates"].ToString().Split(',');
            latitude = double.Parse(centerPoint[0]);
            longitude = double.Parse(centerPoint[1]);
        }

        private const int RadiusOfTheEarth = 6371;

        private static decimal Rad(decimal number)
        {
            return number * (decimal)Math.PI / 180;
        }
    }
}
