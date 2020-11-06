using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlyJetsV2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlyJetsV2.WebApi.Controllers
{
    [Route("api/locations")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private LocationService _locationService;

        public LocationsController(LocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        [Route("search/tree/{filter?}/{hasCoordinate}/{query}", Name = "SearchLocationsTree")]
        public JsonResult SearchTree(int? filter, bool hasCoordinate, string query)
        {
            var locations = _locationService.GetLocations(query, filter, hasCoordinate, 10);

            return new JsonResult(locations.Select(loc => new
            {
                id = loc.Id,
                name = loc.DisplayName,
                countryId = loc.CountryId,
                stateId = loc.StateId,
                cityId = loc.CityId,
                type = loc.Type,
                lat = loc.Lat,
                lng = loc.Lng
            }));
        }

        [HttpGet]
        [Route("inxmilesradius/{lat}/{lng}/{radius}/{locType}", Name = "GetLocationsInXMilesRadius")]
        public JsonResult GetLocationsInXMilesRadius(double lat, double lng, int radius, byte locType)
        {
            var locations = _locationService.GetLocationsWithinXMiles(lat, lng, radius, locType);

            return new JsonResult(locations.Select(loc => new
            {
                id = loc.Id,
                name = loc.DisplayName,
                lat = loc.Lat,
                lng = loc.Lng
            }));
        }
    }
}