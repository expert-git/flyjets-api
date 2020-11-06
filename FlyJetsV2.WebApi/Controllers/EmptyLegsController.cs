using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlyJetsV2.Services;
using FlyJetsV2.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlyJetsV2.WebApi.Controllers
{
    [Route("api/emptylegs")]
    [ApiController]
    public class EmptyLegsController : ControllerBase
    {
        private EmptyLegService _emptyLegService;

        public EmptyLegsController(EmptyLegService emptyLegService)
        {
            _emptyLegService = emptyLegService;
        }


        [Authorize]
        [HttpGet]
        [Route("list")]
        public JsonResult GetEmptyLegs()
        {
            var legs = _emptyLegService.GetList();

            return new JsonResult(legs.Select(leg => new {
                EmptyLegId = leg.Id,
                AircraftNumber = leg.Aircraft.TailNumber,
                Departure = leg.DepartureAirport.DisplayName,
                Arrival = leg.ArrivalAirport.DisplayName,
                leg.DepartureDate,
                leg.ReturnDate
            })
            .ToList());
        }

        [Authorize]
        [HttpPatch]
        [Route("remove-availability")]
        public JsonResult SetUnavailable(EditEmptyLegAvailabilityModel model)
        {
          var availableLegs = _emptyLegService.SetLegUnavailable(model.EmptyLegId);

          return new JsonResult(new {
              legs = availableLegs
          });
        }

        [Authorize]
        [HttpPost]
        [Route("save")]
        public IActionResult SaveEmptyLeg(SaveEmptyLegModel model)
        {
            if (model.EmptyLegId.HasValue)
            {
                _emptyLegService.Update(model.EmptyLegId.Value, model.AircraftId, model.Direction, model.DepartureAirportId,
                    model.ArrivalAirportId, model.DepartureDate, model.ReturnDate, model.ExclusiveCost);
            }
            else
            {
                _emptyLegService.Create(model.AircraftId, model.Direction, model.DepartureAirportId,
                    model.ArrivalAirportId, model.DepartureDate, model.ReturnDate, model.ExclusiveCost);
            }

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("{emptyLegId}")]
        public JsonResult GetEmptyLeg(Guid emptyLegId)
        {
            var emptyLeg = _emptyLegService.GetEmptyLeg(emptyLegId);

            return new JsonResult(new {
                emptyLeg.Id,
                emptyLeg.AircraftId,
                aircraftNumber = emptyLeg.Aircraft.TailNumber,
                emptyLeg.Direction,
                departureId = emptyLeg.DepartureAirportId,
                departure = emptyLeg.DepartureAirport.DisplayName,
                arrivalId = emptyLeg.ArrivalAirportId,
                arrival = emptyLeg.ArrivalAirport.DisplayName,
                departureDate = emptyLeg.DepartureDate.ToString("MM/dd/yyyy"),
                returnDate = emptyLeg.ReturnDate.HasValue ? emptyLeg.ReturnDate.Value.ToString("MM/dd/yyyy") : null,
                emptyLeg.ExclusiveCost
            });
        }
    }
}
