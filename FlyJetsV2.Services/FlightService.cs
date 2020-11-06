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
  public class FlightService
  {
    private IConfiguration _config;
    private IHttpContextAccessor _httpContextAccessor;
    private Guid _accountId;
    private LocationService _locationService;
    private NotificationService _notificationService;

    public FlightService(IConfiguration config, IHttpContextAccessor httpContextAccessor, LocationService locationService, NotificationService notificationService)
    {
      _config = config;
      _httpContextAccessor = httpContextAccessor;
      _locationService = locationService;
      _notificationService = notificationService;

      if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
      {
        _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
      }
    }

    public Flight CreateCharterAircraftFlight(Guid aircraftId, int departureId, DateTime departureDate, TimeSpan? departureTime, 
        int arrivalId, DateTime arrivalDate, TimeSpan? arrivalTime, byte order, short numberOfBookedSeats, byte flightType)
    {
      using(FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var aircraft = dbContext.Aircrafts
          .First(a => a.Id == aircraftId);

        var departure = _locationService.GetLocation(departureId);
        var arrival = _locationService.GetLocation(arrivalId);

        var distance = Utilities.GetDistance(departure.Lat.Value, departure.Lng.Value,
            arrival.Lat.Value, arrival.Lng.Value);

        var duration = CalculateFlightDuration(distance, aircraft.Speed);

        var newFlight = new Flight()
        {
          Id = Guid.NewGuid(),
             Number = aircraft.TailNumber,
             AircraftId = aircraft.Id,
             DepartureId = departureId,
             DepartureDate = departureDate,
             DepartureTime = departureTime,
             ArrivalId = arrivalId,
             ArrivalDate = arrivalDate,
             ArrivalTime = arrivalTime,
             Duration = duration,
             Distance = distance,
             NumberOfSeats = aircraft.MaxPassengers,
             NumberOfSeatsAvailable = (short)(aircraft.MaxPassengers - numberOfBookedSeats),
             Order = order,
             FlightType = flightType,
             Status = (byte)BookingFlightStatuses.OnSchedule,
             CreatedOn = DateTime.UtcNow,
             CreatedById = _accountId
        };

        dbContext.Flights.Add(newFlight);

        dbContext.SaveChanges();

        return newFlight;
      }
    }


    public static TimeSpan CalculateFlightDuration(double distance, short aircraftSpeed)
    {
      var duration = (double)(distance / aircraftSpeed);
      byte flightDurationHours = 0;
      byte flightDurationMinutes = 0;

      flightDurationHours = (byte)Math.Truncate(duration);
      flightDurationMinutes = (byte)Math.Round((double)(duration - Math.Truncate(duration)) * 60, 2);

      //add wait time at airport 
      flightDurationMinutes += 45;

      if (flightDurationMinutes >= 60)
      {
        flightDurationMinutes -= 60;
        flightDurationHours += 1;
      }

      return new TimeSpan(flightDurationHours, flightDurationMinutes, 0);
    }

    public Flight GetCharterSeatFlight(Guid aircraftId, int departureId, int arrivalId, DateTime departureDate)
    {
      using(FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        return dbContext.Flights
          .Where(flight => (flight.FlightType == (byte)BookingTypes.CharterAircraftSeat || flight.FlightType == (byte)BookingTypes.CharterFlightSeat)
              && flight.AircraftId == aircraftId
              && flight.DepartureId == departureId
              && flight.ArrivalId == arrivalId
              && flight.DepartureDate.Year == departureDate.Year
              && flight.DepartureDate.Month == departureDate.Month
              && flight.DepartureDate.Day == departureDate.Day)
          .FirstOrDefault();
      }
    }
  }
}
