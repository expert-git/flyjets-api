using FlyJetsV2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyJetsV2.Services
{
    public class EmptyLegService
    {
        private IConfiguration _config;
        private IHttpContextAccessor _httpContextAccessor;
        private Guid _accountId;
        private LocationService _locationService;

        public EmptyLegService(IConfiguration config, IHttpContextAccessor httpContextAccessor,
            LocationService locationService)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _locationService = locationService;

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
            }
        }

        public List<EmptyLeg> GetList()
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.EmptyLegs
                    .Include(leg => leg.Aircraft)
                    .Include(leg => leg.DepartureAirport)
                    .Include(leg => leg.ArrivalAirport)
                    .Where(el => el.Aircraft.ProviderId == _accountId && el.Available == true)
                    .OrderByDescending(el => el.CreatedOn)
                    .ToList();
            };
        }

        public List<EmptyLeg> SetLegUnavailable(Guid emptyLegId)
        {
          using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
          {
            var emptyLeg = dbContext.EmptyLegs
              .FirstOrDefault(leg => leg.Id == emptyLegId);

            dbContext.EmptyLegs.Attach(emptyLeg);

            emptyLeg.Available = false;

            dbContext.SaveChanges();

            return GetList();
          }
        }

        public ServiceOperationResult<EmptyLeg> Create(Guid aircraftId, byte direction, int departureAirportId,
            int arrivalAirportId, DateTime departureDate,  DateTime? returnDate, decimal exclusiveCost)
        {
            using(FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                ServiceOperationResult<EmptyLeg> result = new ServiceOperationResult<EmptyLeg>();
                result.IsSuccessfull = true;

                var departure = _locationService.GetLocation(departureAirportId);
                var arrival = _locationService.GetLocation(arrivalAirportId);

                var distance = Utilities.GetDistance(departure.Lat.Value, departure.Lng.Value,
                        arrival.Lat.Value, arrival.Lng.Value);

                var aircraftSpeed = (from aircraft in dbContext.Aircrafts
                                     where aircraft.Id == aircraftId
                                     select aircraft.Speed)
                                     .First();

                var duration = FlightService.CalculateFlightDuration(distance, aircraftSpeed);

                EmptyLeg emptyLeg = new EmptyLeg()
                {
                    Id = Guid.NewGuid(),
                    AircraftId = aircraftId,
                    Direction = direction,
                    DepartureAirportId = departureAirportId,
                    ArrivalAirportId = arrivalAirportId,
                    DepartureDate = departureDate,
                    ReturnDate = returnDate,
                    ExclusiveCost = exclusiveCost,
                    Distance = distance,
                    Duration = duration,
                    CreatedById = _accountId,
                    CreatedOn = DateTime.UtcNow,
                    Available = true
                };

                dbContext.EmptyLegs.Add(emptyLeg);
                dbContext.SaveChanges();

                result.Item = emptyLeg;

                return result;
            }
        }

        public ServiceOperationResult<EmptyLeg> Update(Guid emptyLegId, Guid aircraftId, byte direction, int departureAirportId,
            int arrivalAirportId, DateTime departureDate, DateTime? returnDate, decimal exclusiveCost)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                ServiceOperationResult<EmptyLeg> result = new ServiceOperationResult<EmptyLeg>();
                result.IsSuccessfull = true;

                var departure = _locationService.GetLocation(departureAirportId);
                var arrival = _locationService.GetLocation(arrivalAirportId);

                var distance = Utilities.GetDistance(departure.Lat.Value, departure.Lng.Value,
                        arrival.Lat.Value, arrival.Lng.Value) / 1852;

                var aircraftSpeed = (from aircraft in dbContext.Aircrafts
                                     where aircraft.Id == aircraftId
                                     select aircraft.Speed)
                                     .First();

                var duration = FlightService.CalculateFlightDuration(distance, aircraftSpeed);

                EmptyLeg emptyLeg = dbContext.EmptyLegs
                    .First(el => el.Id == emptyLegId);

                emptyLeg.AircraftId = aircraftId;
                emptyLeg.Direction = direction;
                emptyLeg.DepartureAirportId = departureAirportId;
                emptyLeg.ArrivalAirportId = arrivalAirportId;
                emptyLeg.DepartureDate = departureDate;
                emptyLeg.ReturnDate = returnDate;
                emptyLeg.ExclusiveCost = exclusiveCost;
                emptyLeg.Distance = distance;
                emptyLeg.Duration = duration;

                dbContext.SaveChanges();

                result.Item = emptyLeg;

                return result;
            }
        }

        public EmptyLeg GetEmptyLeg(Guid emptyLegId)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.EmptyLegs
                    .Include(leg => leg.Aircraft)
                    .Include(leg => leg.DepartureAirport)
                    .Include(leg => leg.ArrivalAirport)
                    .FirstOrDefault(leg => leg.Id == emptyLegId);
            };
        }
    }
}
