using FlyJetsV2.Data;
using FlyJetsV2.Services.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace FlyJetsV2.Services
{
  public class BookingService : BaseService
  {

    private IConfiguration _config;
    private FeeTypeService _feeTypeService;
    private TaxTypeService _taxTypeService;
    private IHttpContextAccessor _httpContextAccessor;
    private Guid _accountId;
    private FlightService _flightService;
    private PaymentService _paymentService;
    private AccountService _accountService;
    private LocationService _locationService;
    private AircraftService _aircraftService;
    private NotificationService _notificationService;
    private EmptyLegService _emptyLegService;
    private MailerService _mailerService;

    public BookingService(IConfiguration config, FeeTypeService feeTypeService,
        TaxTypeService taxTypeService, IHttpContextAccessor httpContextAccessor,
        FlightService flightService, PaymentService paymentService, AccountService accountService,
        LocationService locationService, AircraftService aircraftService, NotificationService notificationService, EmptyLegService emptyLegService, MailerService mailerService)
    {
      _config = config;
      _feeTypeService = feeTypeService;
      _taxTypeService = taxTypeService;
      _httpContextAccessor = httpContextAccessor;
      _flightService = flightService;
      _paymentService = paymentService;
      _accountService = accountService;
      _locationService = locationService;
      _aircraftService = aircraftService;
      _notificationService = notificationService;
      _emptyLegService = emptyLegService;
      _mailerService = mailerService;

      if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
      {
        _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
      }
    }

    public ServiceOperationResult CreateFlightRequest(byte direction, byte bookingType, int departureId, int arrivalId, DateTime departureDate,
        DateTime? returnDate, int passengersNum, decimal? minPrice, decimal? maxPrice, string notes, byte? aircraftType)
    {
      try
      {
        using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
        {
          var operationResult = new ServiceOperationResult();

          operationResult.IsSuccessfull = true;

          var currentYear = DateTime.UtcNow.Year;

          SqlParameter[] @params =
          {
            new SqlParameter("ReturnVal", SqlDbType.Int) {Direction = ParameterDirection.Output},
            new SqlParameter("ReferenceCode", "FLIGHTREQ"),
            new SqlParameter("Year", currentYear)
          };

          dbContext.Database.ExecuteSqlCommand("FJSP_GetNextNumber @ReferenceCode, @Year, @ReturnVal Output",
              @params);

          var flightRequestNumber = @params[0].Value.ToString();

          var request = new FlightRequest();

          request.Id = Guid.NewGuid();
          request.Number = string.Format("FR{0}{1}", currentYear, flightRequestNumber);
          request.RequesterId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
          request.CreatedOn = DateTime.UtcNow;
          request.Direction = direction;
          request.BookingType = bookingType;
          request.DepartureId = departureId;
          request.ArrivalId = arrivalId;
          request.DepartureDate = departureDate;
          request.ReturnDate = direction == (byte)BookingDirection.Roundtrip ? returnDate : (DateTime?)null;
          request.MinPrice = minPrice;
          request.MaxPrice = maxPrice;
          request.PassengersNumber = passengersNum;
          request.Notes = notes;
          request.Status = (byte)RequestStatuses.Pending;
          request.AircraftType = aircraftType;

          dbContext.FlightRequests.Add(request);
          dbContext.SaveChanges();

          var flyJetsAdmin = dbContext.Accounts
            .Where(account => account.Type == (byte)AccountTypes.Admin)
            .Select(account => account.Id)
            .First();

          _notificationService.NewCreate(flyJetsAdmin,
              NotificationsTypes.NewFlightRequest,
              "New Flight Request",
              new List<NotificationService.NotificationParam>() {
              new NotificationService.NotificationParam() {
              Key = "FlightRequestId",
              Value = request.Id.ToString()
              }
              });
          _notificationService.GetNotifications(flyJetsAdmin);

          return operationResult;
        }
      }
      catch (Exception e)
      {
        throw;
      }
    }

    public List<FlightRequest> GetFlightsRequests(bool getCurrent)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var accountType = (AccountTypes)(from account in dbContext.Accounts
            where account.Id == _accountId
            select account.Type)
          .First();

        var query = dbContext.FlightRequests
          .Include("Requester")
          .Include("Departure")
          .Include("Arrival")
          .Where(req => (getCurrent == true && req.Status == (byte)RequestStatuses.Pending)
              || (getCurrent == false && req.Status != (byte)RequestStatuses.Pending));

        if (accountType == AccountTypes.Flyer)
        {
          return query
            .Where(req => req.RequesterId == _accountId)
            .ToList();
        }
        else if (accountType == AccountTypes.Admin)
        {
          return query.OrderByDescending(req => req.CreatedOn).ToList();
        }

        return new List<FlightRequest>();
      }
    }

    public FlightRequest GetFlightRequest(Guid flightRequestId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var accountType = (AccountTypes)(from account in dbContext.Accounts
            where account.Id == _accountId
            select account.Type)
          .First();

        var query = dbContext.FlightRequests
          .Include("Requester")
          .Include("Departure")
          .Include("Arrival");

        if (accountType == AccountTypes.Flyer)
        {
          return query
            .Where(req => req.RequesterId == _accountId
                && req.Id == flightRequestId)
            .FirstOrDefault();
        }
        else if (accountType == AccountTypes.Admin)
        {
          return query
            .Where(req => req.Id == flightRequestId)
            .FirstOrDefault();
        }

        return null;
      }
    }

    public BookingPaymentDto CalculateCost(decimal exclusiveBookingCost)
    {
      decimal totalFees = 0;
      decimal totalFeesWithoutDonations = 0;
      decimal totalTaxes = 0;

      var feesTypes = _feeTypeService.GetFeesTypes();

      foreach (var feeType in feesTypes)
      {
        var feeValue = feeType.Percentage * exclusiveBookingCost;

        totalFees += feeValue;

        if (feeType.IsForDonation == false)
        {
          totalFeesWithoutDonations += feeValue;
        }
      }

      var taxesTypes = _taxTypeService.GetTaxesTypes();

      foreach (var taxType in taxesTypes)
      {
        decimal taxValue = 0;

        if (taxType.TaxableItems == (byte)TaxableItems.InclusiveCostAndAllFees)
        {
          taxValue = (exclusiveBookingCost + totalFees) * taxType.Percentage;
          totalTaxes += taxValue;
        }
        else if (taxType.TaxableItems == (byte)TaxableItems.InclusiveCostAndFeesExceptDonations)
        {
          taxValue = (exclusiveBookingCost + totalFeesWithoutDonations) * taxType.Percentage;
          totalTaxes += taxValue;
        }
        else if (taxType.TaxableItems == (byte)TaxableItems.InclusiveCostOnly)
        {
          taxValue = exclusiveBookingCost * taxType.Percentage;
          totalTaxes += taxValue;
        }
        else if (taxType.TaxableItems == (byte)TaxableItems.FeesExceptDonations)
        {
          taxValue = totalFeesWithoutDonations * taxType.Percentage;
          totalTaxes += taxValue;
        }
      }

      return new BookingPaymentDto()
      {
        TotalCost = exclusiveBookingCost + totalFees + totalTaxes,
                  TotalExclusiveCost = exclusiveBookingCost,
                  TotalFeesCost = totalFees,
                  TotalTaxesCost = totalTaxes
      };

    }

    public BookingPaymentDto CalculateCost(byte bookingType, byte direction, TimeSpan duration, decimal pricePerFlightHour, decimal? minimumAcceptablePricePerTrip, byte aircraftPax, short pax)
    {
      decimal totalExclusiveCost = 0;

      if (bookingType == (byte)BookingTypes.CharterAircraft || bookingType == (byte)BookingTypes.CharterFlight)
      {
        var totalPrice = pricePerFlightHour * duration.Hours;
        var minutesPrice = (pricePerFlightHour / 60) * duration.Minutes;

        totalPrice += minutesPrice;

        if (minimumAcceptablePricePerTrip.HasValue)
        {
          if (totalPrice < minimumAcceptablePricePerTrip.Value)
          {
            totalExclusiveCost = minimumAcceptablePricePerTrip.Value;
          }
          else
          {
            totalExclusiveCost = totalPrice;
          }
        }
        else
        {
          totalExclusiveCost = totalPrice;
        }
      }
      else if (bookingType == (byte)BookingTypes.CharterAircraftSeat)
      {
        var totalPrice = pricePerFlightHour * duration.Hours;
        var minutesPrice = (pricePerFlightHour / 60) * duration.Minutes;

        totalPrice += minutesPrice;

        totalExclusiveCost = totalPrice / aircraftPax;
      }

      if (direction == (byte)BookingDirection.Roundtrip)
      {
        totalExclusiveCost *= 2;
      }

      return CalculateCost(totalExclusiveCost);
    }

    public JsonResult GetFullBooking(Guid bookingId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var fullBookingInfo = dbContext.BookingFlights
          .Include("Flight")
          .Include("Flight.Aircraft")
          .Include("Flight.Aircraft.Provider")
          .Include("Flight.Aircraft.Model")
          .Include("Flight.Arrival")
          .Include("Flight.Departure")
          .Include("Booking")
          .Include("Booking.BookingFlights.Flight")
          .Include("CreatedBy")
          .Include("Travelers")
          .Where(bf => bf.Flight.Order == 1)
          .FirstOrDefault(bf => bf.BookingId == bookingId);

        return new JsonResult(new {
            booking = fullBookingInfo.Booking,
            bookingFlights = fullBookingInfo.Booking.BookingFlights,
            aircraft = fullBookingInfo.Flight.Aircraft,
            travelers = fullBookingInfo.Travelers,
            flight = fullBookingInfo.Flight,
            departure = fullBookingInfo.Flight.Departure,
            arrival = fullBookingInfo.Flight.Arrival,
            provider = fullBookingInfo.Flight.Aircraft.Provider,
            model = fullBookingInfo.Flight.Aircraft.Model
        });
      }
    }

    public void SendBookingConfirmationEmail(Guid bookingId, string email, string firstName, string lastName) {
      string message = string.Format("Dear {0} {1} - <br /> <br /> Thank you for booking with FLYJETS. Our administration has confirmed your flight, and your invoice can be found <a href={2}/app/Checkout/{3}>here</a>. <br /> <br /> Regards, <br/> The FLYJETS Team", firstName, lastName, _config.GetSection("MailerUrl").Value, bookingId);
      var flyEmail = _config.GetSection("FlyEmail").Value;
      var flyPass = _config.GetSection("FlyPassword").Value;
      _mailerService.Send(email, string.Format("{0} {1}", firstName, lastName), message, "Your Recent FLYJETS Booking", flyEmail, flyPass);
    }

    public List<Booking> GetBookings(bool confirmed, byte bookingType, byte alternative)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var currentAccount = (from account in dbContext.Accounts
            where account.Id == _accountId
            select account)
          .First();

        IQueryable<Booking> query;
        List<byte> bookingStatuses = new List<byte>();

        if (currentAccount.Type == (byte)AccountTypes.AircraftProvider)
        {
          bookingStatuses.Add((byte)BookingStatuses.Confirmed);

          query = dbContext.Bookings
            .Include("Flyer")
            .Where(b => b.BookingFlights.Any(bf => bf.Flight.Aircraft.ProviderId == _accountId)
                && b.Confirmed == true && (b.BookingType == bookingType || b.BookingType == alternative));
        }
        else if (currentAccount.Type == (byte)AccountTypes.Admin)
        {
          bookingStatuses.Add((byte)BookingStatuses.PendingConfirmation);
          bookingStatuses.Add((byte)BookingStatuses.PendingPayment);

          query = dbContext.Bookings
            .Include("Flyer")
            .Include("BookingFlights")
            .Include("BookingFlights.Travelers")
            .Where(b => bookingStatuses.Contains(b.Status) && b.Confirmed == confirmed && (b.BookingType == bookingType || b.BookingType == alternative));
        }
        else
        {
          query = dbContext.Bookings
            .Include("Flyer")
            .Where(b => b.FlyerId == _accountId && b.Confirmed == true);
        }

        _notificationService.SetRead("New Booking");
        return query
          .OrderByDescending(b => b.CreatedOn)
          .ToList();
      }
    }

    public Booking GetBooking(Guid bookingId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var booking = dbContext.Bookings
          .FirstOrDefault(b => b.Id == bookingId);

        return booking;
      }
    }

    public List<BookingFlight> GetBookingFlights(Guid bookingId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var bookingFlights = dbContext.BookingFlights
          .Include(bf => bf.Flight.Departure)
          .Include(bf => bf.Flight.Arrival)
          .Where(b => b.BookingId == bookingId)
          .OrderBy(bf => bf.Flight.Order)
          .ToList();

        return bookingFlights;
      }
    }

    public ServiceOperationResult CreateOfflineBooking(Guid flightRequestId, Guid aircraftProviderId,
        Guid aircraftId, short pax, byte bookingType, byte direction,
        int outboundFlightDepartureId, int outboundFlightArrivalId,
        DateTime outboundFlightDepartureDate, DateTime outboundFlightArrivalDate,
        int? inboundFlightDepartureId, int? inboundFlightArrivalId,
        DateTime? inboundFlightDepartureDate, DateTime? inboundFlightArrivalDate, decimal exclusiveBookingCost, byte bookingPax)
    {
      List<Flight> flights = new List<Flight>();
      Flight outboundFlight = null, inboundFlight = null;

      if (bookingType == (byte)BookingTypes.CharterAircraft)
      {
        outboundFlight = _flightService.CreateCharterAircraftFlight(aircraftId, outboundFlightDepartureId,
            outboundFlightDepartureDate, null, outboundFlightArrivalId, outboundFlightArrivalDate, null,
            1, 0, (byte)BookingTypes.CharterAircraft);

        flights.Add(outboundFlight);

        if (direction == (byte)BookingDirection.Roundtrip)
        {
          inboundFlight = _flightService.CreateCharterAircraftFlight(aircraftId, inboundFlightDepartureId.Value,
              inboundFlightDepartureDate.Value, null, inboundFlightArrivalId.Value, inboundFlightArrivalDate.Value, null,
              2, 0, (byte)BookingTypes.CharterAircraft);

          flights.Add(inboundFlight);
        }
      }

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        DateTime transactionDate = DateTime.UtcNow;

        var bookingNumber = dbContext.Database.ExecuteSqlCommand("FJSP_GetNextNumber @ReferenceCode, @Year",
            new SqlParameter("ReferenceCode", "BOOKING"),
            new SqlParameter("Year", transactionDate.Year));

        var aircraft = dbContext.Aircrafts
          .First(a => a.Id == aircraftId);

        var flightRequest = dbContext.FlightRequests
          .First(a => a.Id == flightRequestId);

        var bookingCost = CalculateCost(exclusiveBookingCost);

        Booking newBooking = CreateBookingObject(flightRequest.RequesterId, flightRequestId, bookingNumber,
            direction, bookingType, (byte)BookingStatuses.PendingPayment, flights, transactionDate, bookingPax);

        newBooking.TotalExclusiveCost = bookingCost.TotalExclusiveCost;
        newBooking.TotalFees = bookingCost.TotalFeesCost;
        newBooking.TotalTaxes = bookingCost.TotalTaxesCost;

        dbContext.Bookings.Add(newBooking);

        ServiceOperationResult result = new ServiceOperationResult();

        result.IsSuccessfull = true;

        //save changes
        try
        {
          dbContext.SaveChanges();

          return result;
        }
        catch (Exception ex)
        {
          throw;
        }
      }

      //send email confrimation to the flyer

    }

    private Booking CreateBookingObject(Guid flyerId, Guid? flightRequestId,
        int bookingNumber, byte direction, byte bookingType, byte bookingStatus,
        List<Flight> flights, DateTime transactionDate, byte bookingPax)
    {
      //create booking
      Booking newBooking;
      Guid newBookingId = Guid.NewGuid();

      newBooking = new Booking()
      {
        Id = newBookingId,
           Direction = direction,
           BookingType = bookingType,
           Number = string.Format("BO{0}{1}", DateTime.UtcNow.Year, bookingNumber),
           FlyerId = flyerId,
           Status = bookingStatus,
           FlightRequestId = flightRequestId,
           CreatedOn = transactionDate,
           CreatedById = _accountId,
           NumPax = bookingPax
      };

      newBooking.StatusHistory = new List<BookingStatus>()
      {
        new BookingStatus()
        {
          Id = Guid.NewGuid(),
             BookingId = newBooking.Id,
             CreatedOn = transactionDate,
             CreatedById = _accountId,
             Status = bookingStatus
        }
      };

      newBooking.BookingFlights = new List<BookingFlight>();

      foreach (var flight in flights)
      {
        newBooking.BookingFlights.Add(new BookingFlight()
            {
            Id = Guid.NewGuid(),
            BookingId = newBooking.Id,
            FlightId = flight.Id,
            CreatedOn = transactionDate,
            CreatedById = _accountId
            });
      }

      return newBooking;
    }

    public BookingPaymentDto CalculateCost(Guid bookingId)
    {
      decimal totalFees = 0;
      decimal totalFeesWithoutDonations = 0;
      decimal totalTaxes = 0;
      decimal exclusiveBookingCost = 0;

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        exclusiveBookingCost = (from booking in dbContext.Bookings
            where booking.Id == bookingId
            select booking.TotalExclusiveCost)
          .First();
      }

      var feesTypes = _feeTypeService.GetFeesTypes();
      var bookingFees = new List<BookingFee>();

      foreach (var feeType in feesTypes)
      {
        var feeValue = feeType.Percentage * exclusiveBookingCost;

        totalFees += feeValue;

        if (feeType.IsForDonation == false)
        {
          totalFeesWithoutDonations += feeValue;
        }
      }

      var taxesTypes = _taxTypeService.GetTaxesTypes();
      var bookingTaxes = new List<BookingTax>();

      foreach (var taxType in taxesTypes)
      {
        decimal taxValue = 0;

        if (taxType.TaxableItems == (byte)TaxableItems.InclusiveCostAndAllFees)
        {
          taxValue = (exclusiveBookingCost + totalFees) * taxType.Percentage;
          totalTaxes += taxValue;
        }
        else if (taxType.TaxableItems == (byte)TaxableItems.InclusiveCostAndFeesExceptDonations)
        {
          taxValue = (exclusiveBookingCost + totalFeesWithoutDonations) * taxType.Percentage;
          totalTaxes += taxValue;
        }
        else if (taxType.TaxableItems == (byte)TaxableItems.InclusiveCostOnly)
        {
          taxValue = exclusiveBookingCost * taxType.Percentage;
          totalTaxes += taxValue;
        }
        else if (taxType.TaxableItems == (byte)TaxableItems.FeesExceptDonations)
        {
          taxValue = totalFeesWithoutDonations * taxType.Percentage;
          totalTaxes += taxValue;
        }
      }

      return new BookingPaymentDto()
      {
        TotalCost = exclusiveBookingCost + totalFees + totalTaxes,
                  TotalExclusiveCost = exclusiveBookingCost,
                  TotalFeesCost = totalFees,
                  TotalTaxesCost = totalTaxes
      };

    }

    public ServiceOperationResult<Guid> ConfirmOfflineBooking(Guid bookingId, Guid paymentMethodId, List<CreateBookingTravelerDto> travelers)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var bookingResult = new ServiceOperationResult<Guid>();
        bookingResult.IsSuccessfull = false;

        var booking = dbContext.Bookings
          .Include("StatusHistory")
          .Include("BookingFlights.Flight")
          .First(b => b.Id == bookingId);

        //create travelers
        var outboundFlight = booking.BookingFlights.First(bf => bf.Flight.Order == 1);

        outboundFlight.Flight.NumberOfSeatsAvailable -= (short)booking.NumPax;
        outboundFlight.Travelers = new List<BookingFlightTraveler>();

        foreach (var traveler in travelers)
        {
          var outboundTraveler = new BookingFlightTraveler()
          {
            Id = Guid.NewGuid(),
               BookingFlightId = outboundFlight.Id,
               Email = traveler.Email,
               FirstName = traveler.FirstName,
               LastName = traveler.LastName
          };

          outboundFlight.Travelers.Add(outboundTraveler);
        }

        var inboundFlight = booking.BookingFlights.FirstOrDefault(bf => bf.Flight.Order == 2);

        if (inboundFlight != null)
        {
          inboundFlight.Travelers = new List<BookingFlightTraveler>();
          inboundFlight.Flight.NumberOfSeatsAvailable -= (short)booking.NumPax;

          foreach (var traveler in travelers)
          {
            var inboundTraveler = new BookingFlightTraveler()
            {
              Id = Guid.NewGuid(),
                 BookingFlightId = inboundFlight.Id,
                 Email = traveler.Email,
                 FirstName = traveler.FirstName,
                 LastName = traveler.LastName
            };

            inboundFlight.Travelers.Add(inboundTraveler);
          }
        }

        //pay
        /* var account = _accountService.GetAccount(_accountId); */
        /* var paymentMethod = _paymentService.GetPaymentMethod(paymentMethodId); */
        /* var totalCost = (long)(Math.Round(booking.TotalExclusiveCost + booking.TotalFees + booking.TotalTaxes, 2) * 100); */

        /* var paymentResult = _paymentService.Charge(totalCost, "Payment for Booking #" + booking.Number, */
        /*     account.StripeCustomerId, paymentMethod.ReferencePaymentMethodId, */
        /*     new Dictionary<string, string>() { { "Booking#", booking.Number }, { "BookingId", booking.Id.ToString() } }, true); */

        /* if (paymentResult.IsSuccessfull == false) */
        /* { */
        /*   throw new Exception("Payment Failed"); */
        /* } */

        /* booking.Status = (byte)BookingStatuses.Confirmed; */
        /* booking.PaymentReference = paymentResult.Item.Id; */

        booking.StatusHistory.Add(new BookingStatus()
            {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Status = (byte)BookingStatuses.PendingPayment,
            CreatedById = _accountId,
            CreatedOn = DateTime.UtcNow,
            Params = null
            });

        dbContext.SaveChanges();

        bookingResult.IsSuccessfull = true;
        bookingResult.Item = booking.Id;

        return bookingResult;
      }
    }

    public List<BookingFlight> GetBookingsFlights(FilterBookedFlightsBy filterBy)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var accountType = (AccountTypes)(from account in dbContext.Accounts
            where account.Id == _accountId
            select account.Type)
          .First();

        IQueryable<BookingFlight> query = dbContext.BookingFlights
          .Include("Booking.Flyer")
          .Include("Flight.Departure")
          .Include("Flight.Arrival"); ;

        if (accountType == AccountTypes.Flyer)
        {
          query = query
            .Where(bf => bf.Booking.FlyerId == _accountId);
        }
        else if (accountType == AccountTypes.AircraftProvider)
        {
          query = query
            .Where(bf => bf.Flight.Aircraft.ProviderId == _accountId);
        }

        var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

        if (filterBy == FilterBookedFlightsBy.Current)
        {
          query = query
            .Where(bf => bf.Flight.DepartureDate <= now && bf.Flight.ArrivalDate >= now);
        }
        else if (filterBy == FilterBookedFlightsBy.Upcoming)
        {
          query = query
            .Where(bf => bf.Flight.DepartureDate > now);
        }
        else if (filterBy == FilterBookedFlightsBy.Historical)
        {
          query = query
            .Where(bf => bf.Flight.ArrivalDate < now);
        }

        return query
          .OrderByDescending(bf => bf.Flight.DepartureDate)
          .ToList();
      }
    }

    public Booking Create(Guid aircraftAvailabilityId, byte direction, byte bookingType, int departureId,
        int arrivalId, DateTime departureDate, DateTime? returnDate, byte pax, Guid? paymentMethodId,
        List<CreateBookingTravelerDto> travelers, byte bookingPax)
    {
      ServiceOperationResult<Guid> result = new ServiceOperationResult<Guid>();
      result.IsSuccessfull = true;

      var transactionDate = DateTime.UtcNow;

      AircraftAvailability aircraftAvailability = _aircraftService.GetAvailability(aircraftAvailabilityId);
      EmptyLeg emptyLegAvailability = _emptyLegService.GetEmptyLeg(aircraftAvailabilityId);
      int bookingNumber;

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        SqlParameter[] @params =
        {
          new SqlParameter("ReturnVal", SqlDbType.Int) {Direction = ParameterDirection.Output},
          new SqlParameter("ReferenceCode", "BOOKING"),
          new SqlParameter("Year", transactionDate.Year)
        };

        dbContext.Database.ExecuteSqlCommand("FJSP_GetNextNumber @ReferenceCode, @Year, @ReturnVal Output",
            @params);

        bookingNumber = (int)@params[0].Value;
      }

      var departure = _locationService.GetLocation(departureId);
      var arrival = _locationService.GetLocation(arrivalId);

      var distance = Utilities.GetDistance(departure.Lat.Value, departure.Lng.Value, arrival.Lat.Value, arrival.Lng.Value);

      TimeSpan flightDuration;
      if (bookingType == (byte)BookingTypes.CharterAircraft || bookingType == (byte)BookingTypes.CharterAircraftSeat)
      {
        flightDuration = FlightService.CalculateFlightDuration(distance, aircraftAvailability.Aircraft.Speed);
      } else {
        flightDuration = FlightService.CalculateFlightDuration(distance, emptyLegAvailability.Aircraft.Speed);
      }

      decimal pricePerHour;

      if (bookingType == (byte)BookingTypes.CharterAircraft || bookingType == (byte)BookingTypes.CharterAircraftSeat)
      {
        pricePerHour = aircraftAvailability.PricePerHour.HasValue ? aircraftAvailability.PricePerHour.Value :
        aircraftAvailability.Aircraft.PricePerHour;
      } else {
        pricePerHour = emptyLegAvailability.ExclusiveCost;
      }

      BookingPaymentDto bookingCost;

      if(bookingType == (byte)BookingTypes.CharterAircraft || bookingType == (byte)BookingTypes.CharterAircraftSeat)
      {
        bookingCost = CalculateCost(bookingType, direction, flightDuration, pricePerHour,
          aircraftAvailability.MinimumAcceptablePricePerTrip, aircraftAvailability.Aircraft.MaxPassengers,
          bookingPax);
      } else {
        if (bookingType == (byte)BookingTypes.CharterFlightSeat)
        {
          bookingCost = CalculateCost(emptyLegAvailability.ExclusiveCost / emptyLegAvailability.Aircraft.MaxPassengers * bookingPax);
        } 
        else
        {
          bookingCost = CalculateCost(emptyLegAvailability.ExclusiveCost);
        }
      }


      List<Flight> flights = new List<Flight>();
      Flight outboundFlight = null;
      Flight inboundFlight = null;

      if (bookingType == (byte)BookingTypes.CharterAircraft)
      {
        outboundFlight = _flightService.CreateCharterAircraftFlight(aircraftAvailability.AircraftId, departureId,
            departureDate, null, arrivalId, departureDate, null,
            1, bookingPax, (byte)BookingTypes.CharterAircraft);

        flights.Add(outboundFlight);

        if (direction == (byte)BookingDirection.Roundtrip)
        {
          inboundFlight = _flightService.CreateCharterAircraftFlight(aircraftAvailability.AircraftId, arrivalId,
              returnDate.Value, null, departureId, returnDate.Value, null,
              2, bookingPax, (byte)BookingTypes.CharterAircraft);

          flights.Add(inboundFlight);
        }
      }
      else if(bookingType == (byte)BookingTypes.CharterAircraftSeat)
      {
        outboundFlight = _flightService.GetCharterSeatFlight(aircraftAvailability.AircraftId, departureId, arrivalId,
            departureDate);

        if(outboundFlight == null)
        {
        outboundFlight = _flightService.CreateCharterAircraftFlight(aircraftAvailability.AircraftId, departureId,
            departureDate, null, arrivalId, departureDate, null,
            1, bookingPax, (byte)BookingTypes.CharterAircraftSeat);

        flights.Add(outboundFlight);
        }
        if (direction == (byte)BookingDirection.Roundtrip)
        {
          inboundFlight = _flightService.GetCharterSeatFlight(aircraftAvailability.AircraftId, arrivalId, departureId,
              returnDate.Value);

          if (inboundFlight == null)
          {   
            inboundFlight = _flightService.CreateCharterAircraftFlight(aircraftAvailability.AircraftId, arrivalId,
            returnDate.Value, null, departureId, returnDate.Value, null,
            2, bookingPax, (byte)BookingTypes.CharterAircraftSeat);

          }
          flights.Add(inboundFlight);
        }
      }
      else if (bookingType == (byte)BookingTypes.CharterFlight)
      {
        outboundFlight = _flightService.CreateCharterAircraftFlight(emptyLegAvailability.AircraftId, departureId, departureDate, null, arrivalId, departureDate, null, 1, bookingPax, (byte)BookingTypes.CharterFlight);
        flights.Add(outboundFlight);
        if(direction == (byte)BookingDirection.Roundtrip) {
          inboundFlight = _flightService.CreateCharterAircraftFlight(emptyLegAvailability.AircraftId, arrivalId, returnDate.Value, null, departureId, returnDate.Value, null, 2, bookingPax, (byte)BookingTypes.CharterFlight);
          flights.Add(inboundFlight);
        }
      }
      else if (bookingType == (byte)BookingTypes.CharterFlightSeat)
      {
        outboundFlight = _flightService.GetCharterSeatFlight(emptyLegAvailability.AircraftId, departureId, arrivalId,
            departureDate);

        if(outboundFlight == null)
        {
        outboundFlight = _flightService.CreateCharterAircraftFlight(emptyLegAvailability.AircraftId, departureId,
            departureDate, null, arrivalId, departureDate, null,
            1, 0, (byte)BookingTypes.CharterAircraftSeat);

        flights.Add(outboundFlight);
        }

        if (direction == (byte)BookingDirection.Roundtrip)
        {
          inboundFlight = _flightService.GetCharterSeatFlight(aircraftAvailability.AircraftId, arrivalId, departureId,
              returnDate.Value);

          if (inboundFlight == null)
          {   
            inboundFlight = _flightService.CreateCharterAircraftFlight(aircraftAvailability.AircraftId, arrivalId,
            returnDate.Value, null, departureId, returnDate.Value, null,
            2, bookingPax, (byte)BookingTypes.CharterFlightSeat);

          }
          flights.Add(inboundFlight);
        }

      }

      Booking newBooking = CreateBookingObject(_accountId, null, bookingNumber, direction, bookingType, (byte)BookingStatuses.New,
          flights, transactionDate, bookingPax);

      newBooking.TotalExclusiveCost = bookingCost.TotalExclusiveCost;
      newBooking.TotalFees = bookingCost.TotalFeesCost;
      newBooking.TotalTaxes = bookingCost.TotalTaxesCost;

      newBooking.Status = (byte)BookingStatuses.New;
      newBooking.StatusHistory = new List<BookingStatus>()
      {
        new BookingStatus
        {
          Id = Guid.NewGuid(),
             CreatedById = _accountId,
             CreatedOn = DateTime.UtcNow,
             Status = (byte)BookingStatuses.New
        }
      };

      newBooking.BookingFlights = new List<BookingFlight>();

      BookingFlight outboundBookingFlight = new BookingFlight()
      {
        Id = Guid.NewGuid(),
           BookingId = newBooking.Id,
           FlightId = outboundFlight.Id,
           CreatedById = _accountId,
           CreatedOn = DateTime.UtcNow,
           Travelers = new List<BookingFlightTraveler>()
      };

      foreach (var traveler in travelers)
      {
        outboundBookingFlight.Travelers.Add(new BookingFlightTraveler()
            {
            Id = Guid.NewGuid(),
            FlyerId = traveler.Id,
            FirstName = traveler.Id.HasValue ? null : traveler.FirstName,
            LastName = traveler.Id.HasValue ? null : traveler.LastName,
            Email = traveler.Id.HasValue ? null : traveler.Email
            });
      }

      newBooking.BookingFlights.Add(outboundBookingFlight);

      if (direction == (byte)BookingDirection.Roundtrip)
      {
        BookingFlight inboundBookingFlight = new BookingFlight()
        {
          Id = Guid.NewGuid(),
             BookingId = newBooking.Id,
             FlightId = inboundFlight.Id,
             CreatedById = _accountId,
             CreatedOn = DateTime.UtcNow,
             Travelers = new List<BookingFlightTraveler>()
        };

        if (travelers != null)
        {
          foreach (var traveler in travelers)
          {
            inboundBookingFlight.Travelers.Add(new BookingFlightTraveler()
                {
                Id = Guid.NewGuid(),
                FlyerId = traveler.Id,
                FirstName = traveler.Id.HasValue ? null : traveler.FirstName,
                LastName = traveler.Id.HasValue ? null : traveler.LastName,
                Email = traveler.Id.HasValue ? null : traveler.Email
                });
          }
        }

        newBooking.BookingFlights.Add(inboundBookingFlight);
      }

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        dbContext.Bookings.Add(newBooking);
        dbContext.SaveChanges();
      }

      var account = _accountService.GetAccount(_accountId);
      /* var paymentMethod = _paymentService.GetPaymentMethod(paymentMethodId); */
      /* var totalCost = (long)(Math.Round(newBooking.TotalExclusiveCost + newBooking.TotalFees + newBooking.TotalTaxes, 2) * 100); */

      /* var paymentResult = _paymentService.Charge(totalCost, "Payment for Booking #" + newBooking.Number, */
      /*     account.StripeCustomerId, paymentMethod.ReferencePaymentMethodId, */
      /*     new Dictionary<string, string>() { { "Booking#", newBooking.Number }, { "BookingId", newBooking.Id.ToString() } }, false); */

      /* if (paymentResult.IsSuccessfull == false) */
      /* { */
      /*   return paymentResult; */
      /* } */

      //update booking status
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        dbContext.Bookings.Attach(newBooking);

        newBooking.Status = (byte)BookingStatuses.PendingPayment;
        /* if (newBooking.BookingType == (byte)BookingTypes.CharterAircraft) */
        /* { */
        /*   newBooking.Status = (byte)BookingStatuses.PendingConfirmation; */
        /* } */
        /* else */
        /* { */
        /*   newBooking.Status = (byte)BookingStatuses.PendingMinimumTravelers; */
        /* } */

        newBooking.StatusHistory.Add(new BookingStatus()
            {
            Id = Guid.NewGuid(),
            BookingId = newBooking.Id,
            Status = newBooking.Status,
            CreatedById = _accountId,
            CreatedOn = DateTime.UtcNow,
            Params = null
            });

        /* newBooking.PaymentReference = paymentResult.Item.Id; */

        dbContext.SaveChanges();

        var flyJetsAdmin = dbContext.Accounts
          .Where(acc => acc.Type == (byte)AccountTypes.Admin)
          .Select(acc => acc.Id)
          .First();

        Guid providerId;
        if (bookingType == (byte)BookingTypes.CharterAircraft || bookingType == (byte)BookingTypes.CharterAircraftSeat) {
          providerId = aircraftAvailability.Aircraft.ProviderId;
        } else {
          providerId = emptyLegAvailability.Aircraft.ProviderId;
        }

        //create notification for admin
        _notificationService.NewCreate(flyJetsAdmin, NotificationsTypes.NewBooking, "New Booking", new List<NotificationService.NotificationParam>() {
            new NotificationService.NotificationParam {
            Key = "BookingId",
            Value = newBooking.Id.ToString()
            },
            new NotificationService.NotificationParam {
            Key = "BookerFirstName",
            Value = account.FirstName
            },
            new NotificationService.NotificationParam {
            Key = "BookerLastName",
            Value = account.LastName
            }
            });
        _notificationService.GetNotifications(flyJetsAdmin);

        //create notification for provider. wish we could find a different way to do this, but c'est la vie for now
        _notificationService.NewCreate(providerId, NotificationsTypes.NewBooking, "New Booking", new List<NotificationService.NotificationParam>() {
            new NotificationService.NotificationParam {
            Key = "BookingId",
            Value = newBooking.Id.ToString()
            },
            new NotificationService.NotificationParam {
            Key = "BookerFirstName",
            Value = account.FirstName
            },
            new NotificationService.NotificationParam {
            Key = "BookerLastName",
            Value = account.LastName
            }

            });
        _notificationService.GetNotifications(providerId);
      }


      return newBooking;
    }

    public JsonResult EditBooking(string bookingNo, List<CreateBookingTravelerDto> travelers)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var booking = dbContext.Bookings
          .Include("BookingFlights.Flight")
          .FirstOrDefault(b => b.Number == bookingNo);

        var bookingFlight = booking.BookingFlights.First(bf => bf.Flight.Order == 1);

        dbContext.Bookings.Attach(booking);
        booking.Confirmed = true;

        bookingFlight.Travelers = new List<BookingFlightTraveler>();
        
        if (travelers != null) {
          foreach (var traveler in travelers)
          {
            bookingFlight.Travelers.Add(new BookingFlightTraveler()
            {
              Id = Guid.NewGuid(),
              FlyerId = traveler.Id,
              FirstName = traveler.FirstName,
              LastName = traveler.LastName,
              Email = traveler.Email
            });
          }
        }

        dbContext.SaveChanges();
        return new JsonResult(new {
            booking = booking,
            flight = bookingFlight,
            travelersList = bookingFlight.Travelers
            });
      }
    }


    public List<SearchCharterAircraftResultDto> SearchCharterAircrafts(int departureId, int arrivalId, DateTime departureDate,
        DateTime? returnDate, short pax, byte bookingType, byte direction)
    {
      var departure = _locationService.GetLocation(departureId);
      var arrival = _locationService.GetLocation(arrivalId);
      List<AircraftImage> aircraftsDefaultImages;
      List<LocationTree> possibleDepartureAirports;
      List<LocationTree> possibleArrivalAirports;

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var estimatedDistance = Utilities.GetDistance(departure.Lat.Value, departure.Lng.Value, arrival.Lat.Value, arrival.Lng.Value);
        //get all aircrafts matches the search citeria within the limits of 
        //availability locations and periods
        var aircrafts = (from aircraft in dbContext.Aircrafts
            join model in dbContext.AircraftModels on aircraft.ModelId equals model.Id
            join type in dbContext.AircraftTypes on aircraft.TypeId equals type.Id
            join homebase in dbContext.LocationsTree on aircraft.HomeBaseId equals homebase.Id
            join availability in dbContext.AircraftsAvailability on aircraft.Id equals availability.AircraftId
            where dbContext.AircraftAvailabilityLocations
            .Any(aal => aal.AircraftAvailabilityId == availability.Id
              && aal.IsForDeparture == true
              && (
                aal.Location.LocationId == departure.LocationId
                ||
                (aal.Location.LocationId == null && aal.Location.CityId == departure.CityId)
                ||
                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId.HasValue && aal.Location.StateId == departure.StateId)
                ||
                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId == null && aal.Location.CountryId == departure.CountryId)
                ))
            &&
            dbContext.AircraftAvailabilityLocations
            .Any(aal => aal.AircraftAvailabilityId == availability.Id
              && aal.IsForDeparture == false
              && (
                aal.Location.LocationId == arrival.LocationId
                ||
                (aal.Location.LocationId == null && aal.Location.CityId == arrival.CityId)
                ||
                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId.HasValue && aal.Location.StateId == arrival.StateId)
                ||
                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId == null && aal.Location.CountryId == arrival.CountryId)
                ))
            &&
            dbContext.AircraftsAvailabilityPeriods
            .Any(aap => aap.AircraftAvailabilityId == availability.Id
                && aap.From <= departureDate && aap.To >= departureDate)
            &&
            (returnDate.HasValue == false
             ||
             dbContext.AircraftsAvailabilityPeriods
             .Any(aap => aap.AircraftAvailabilityId == availability.Id
               && aap.From <= departureDate && aap.To >= departureDate))
            && ((bookingType == (byte)BookingTypes.CharterAircraft)
                ||
                (bookingType == (byte)BookingTypes.CharterAircraftSeat && availability.SellCharterSeat == true))
            && availability.Available == true
              && ((decimal)(FlightService.CalculateFlightDuration(estimatedDistance, aircraft.Speed).TotalMinutes / 60) * aircraft.PricePerHour) >= availability.MinimumAcceptablePricePerTrip
            select new
            {
              AircraftAvailabilityId = availability.Id,
                                     AircraftId = aircraft.Id,
                                     AircraftModel = model.Name,
                                     AircraftPax = aircraft.MaxPassengers,
                                     AircraftType = type.Name,
                                     AircraftArgusSafetyRating = aircraft.ArgusSafetyRating,
                                     AircraftSpeed = aircraft.Speed,
                                     AircraftRange = aircraft.Range,
                                     PricePerHour = availability.PricePerHour.HasValue ? availability.PricePerHour.Value : aircraft.PricePerHour,
                                     MinimumAcceptablePricePerTrip = availability.MinimumAcceptablePricePerTrip,
                                     HomeBaseId = aircraft.HomeBaseId,
                                     HomeBaseLat = homebase.Lat,
                                     HomeBaseLng = homebase.Lng,
                                     WiFi = aircraft.WiFi,
                                     BookableDemo = aircraft.BookableDemo,
                                     NumberOfTelevision = aircraft.NumberOfTelevision,
                                     ReroutingRadius = availability.ReroutingRadius,
            })
        .ToList();


        List<SearchCharterAircraftResultDto> finalAircrafts = new List<SearchCharterAircraftResultDto>();

        var aircraftsIds = aircrafts.Select(a => a.AircraftId).ToList();

        aircraftsDefaultImages = (from image in dbContext.AircraftImages
            where image.Order == 1
            && aircraftsIds.Any(id => id == image.AircraftId)
            select image)
          .ToList();

        //get possible departure and arrival airports and compine them in routes
        possibleDepartureAirports = (from location in dbContext.LocationsTree
            where location.Type == (byte)LocationsTypes.Airport
            &&
            (
             (departure.Type == (byte)LocationsTypes.Airport
              && departure.Id == location.Id)
             ||
             (departure.Type != (byte)LocationsTypes.Airport
              && (departure.CityId.HasValue == false || departure.CityId == location.CityId)
              && (departure.StateId.HasValue == false || departure.StateId == location.StateId)
              && departure.CountryId == location.CountryId)
            )
            select location)
          .ToList();

        possibleArrivalAirports = (from location in dbContext.LocationsTree
            where location.Type == (byte)LocationsTypes.Airport
            &&
            (
             (arrival.Type == (byte)LocationsTypes.Airport
              && arrival.Id == location.Id)
             ||
             (arrival.Type != (byte)LocationsTypes.Airport
              && (arrival.CityId.HasValue == false || arrival.CityId == location.CityId)
              && (arrival.StateId.HasValue == false || arrival.StateId == location.StateId)
              && arrival.CountryId == location.CountryId)
            )
            select location)
          .ToList();

        //for each route, do the calculation and add to final result
        foreach (var possibleDepartureAirport in possibleDepartureAirports)
        {
          foreach (var possibleArrivalAirport in possibleArrivalAirports)
          {
            var distance = Utilities.GetDistance(possibleDepartureAirport.Lat.Value, possibleDepartureAirport.Lng.Value,
                possibleArrivalAirport.Lat.Value, possibleArrivalAirport.Lng.Value);

            foreach (var aircraft in aircrafts)
            {

              var finalDistance = distance;

              // pull all availability location entries for each aircraft availability
              var availableLocations = (from availabilityLocation in dbContext.AircraftAvailabilityLocations 
                join location in dbContext.LocationsTree on availabilityLocation.LocationTreeId equals location.Id 
                where availabilityLocation.AircraftAvailabilityId == aircraft.AircraftAvailabilityId
                select new {
                  IsForDeparture = availabilityLocation.IsForDeparture,
                  Rerouting = availabilityLocation.Rerouting,
                  Lat = location.Lat,
                  Lng = location.Lng,
                  LocationTreeId = location.Id,
                  LocationType = location.Type,
                  CountryId = location.CountryId,
                  StateId = location.StateId,
                  CityId = location.CityId

                }).ToList();

              // find shortest possible rerouting scenario, if any, and add to final list if applicable
              double ShortestDepartureReroute = 0;
              var DepartureRerouteNeeded = true;


              double ShortestArrivalReroute = 0;
              var ArrivalRerouteNeeded = true;

              var RemoveFromFinalForDeparture = false;
              var RemoveFromFinalForArrival = false;
              var RemoveFromFinal = false;

              foreach (var availableLoc in availableLocations)
              {
                if (availableLoc.IsForDeparture && DepartureRerouteNeeded) {
                  switch (availableLoc.LocationType) {
                    case 2: //airport
                        if (possibleDepartureAirport.Id == availableLoc.LocationTreeId)
                        {
                          DepartureRerouteNeeded = false;
                        } 
                        else
                        {
                          var currentReroute = Utilities.GetDistance(availableLoc.Lat.Value, availableLoc.Lng.Value, possibleDepartureAirport.Lat.Value, possibleDepartureAirport.Lng.Value);
                          if (ShortestDepartureReroute == 0)
                          {
                            ShortestDepartureReroute = currentReroute;
                          } 
                          else
                          {
                            ShortestDepartureReroute = currentReroute < ShortestDepartureReroute ? currentReroute : ShortestDepartureReroute;
                          }
                        }
                    break;
                    case 16: //country
                      if (availableLoc.CountryId == possibleDepartureAirport.CountryId)
                      {
                        DepartureRerouteNeeded = false;
                        RemoveFromFinalForDeparture = false;
                      } 
                      else if (DepartureRerouteNeeded)
                      {
                        RemoveFromFinalForDeparture = true;
                      }
                    break;
                    case 32: //state
                      if (availableLoc.StateId == possibleDepartureAirport.StateId)
                      {
                        DepartureRerouteNeeded = false;
                        RemoveFromFinalForDeparture = false;
                      }
                      else if (DepartureRerouteNeeded)
                      {
                        RemoveFromFinalForDeparture = true;
                      }
                    break;
                    case 64: //city
                      if (availableLoc.CityId == possibleDepartureAirport.CityId) 
                      {
                        DepartureRerouteNeeded = false;
                        RemoveFromFinalForDeparture = false;
                      }
                      else if (DepartureRerouteNeeded)
                      {
                        RemoveFromFinalForDeparture = true;
                      }
                    break;
                  }
                } 
                else if (availableLoc.IsForDeparture == false && ArrivalRerouteNeeded)
                {
                  switch (availableLoc.LocationType) {
                    case 2: //airport
                      if (possibleArrivalAirport.Id == availableLoc.LocationTreeId)
                      {
                        ArrivalRerouteNeeded = false;
                      } 
                      else
                      {
                        var currentReroute = Utilities.GetDistance(availableLoc.Lat.Value, availableLoc.Lng.Value, possibleArrivalAirport.Lat.Value, possibleArrivalAirport.Lng.Value);
                        if (ShortestArrivalReroute == 0) {
                          ShortestArrivalReroute = currentReroute;
                        }
                        else
                        {
                        ShortestArrivalReroute = currentReroute < ShortestArrivalReroute ? currentReroute : ShortestArrivalReroute;
                        }
                      }
                    break;
                    case 16: //country
                      if (availableLoc.CountryId == possibleArrivalAirport.CountryId)
                      {
                        ArrivalRerouteNeeded = false;
                        RemoveFromFinalForArrival = false;
                      }
                      else if (DepartureRerouteNeeded)
                      {
                        RemoveFromFinalForArrival = true;
                      }
                    break;
                    case 32: //state
                      if (availableLoc.StateId == possibleArrivalAirport.StateId)
                      {
                        ArrivalRerouteNeeded = false;
                        RemoveFromFinalForArrival = false;
                      }
                        else if (DepartureRerouteNeeded)
                      {
                        RemoveFromFinalForArrival = true;
                      }
                    break;
                    case 64: //city
                      if (availableLoc.CityId == possibleArrivalAirport.CityId) 
                      {
                        ArrivalRerouteNeeded = false;
                        RemoveFromFinalForArrival = false;
                      }
                        else if (DepartureRerouteNeeded)
                      {
                        RemoveFromFinalForArrival = true;
                      }
                    break;
                  }
                }
               }
              
              // don't add to final list if rerouting radius is shorter than needed reroute, 
              // or if availability is city/state/country and reroute is still needed ( meaning airport for arrival or departure isn't within the city/state/country)

              // RemoveFromFinal(Dep/Arr) in above loop will switch to true if an availability location was not an airport, and the country/state/city of a didn't match the availability location's
              // here, turn RemoveFromFinal true if RemoveFromFinal(Dep/Arr) is true, and there is no applicable reroute scenario

              if (RemoveFromFinalForDeparture && ShortestDepartureReroute == 0 && DepartureRerouteNeeded) 
              {
                RemoveFromFinal = true;
              }

              if ((aircraft.ReroutingRadius.HasValue && ( DepartureRerouteNeeded == true ) && (ShortestDepartureReroute > aircraft.ReroutingRadius)))
              {
                RemoveFromFinal = true;
              }
              
              if (RemoveFromFinalForArrival && ShortestArrivalReroute == 0 && ArrivalRerouteNeeded) 
              {
                RemoveFromFinal = true;
              }

              if (aircraft.ReroutingRadius.HasValue && (ArrivalRerouteNeeded == true) && (ShortestArrivalReroute > aircraft.ReroutingRadius))
              {
                RemoveFromFinal = true;
              }
              
              if (aircraft.ReroutingRadius.HasValue == false && (DepartureRerouteNeeded || ArrivalRerouteNeeded))
              {
                RemoveFromFinal = true;
              }



              if (RemoveFromFinal == false)
              {
              // including departure reroute for distance

              // if (DepartureRerouteNeeded && ArrivalRerouteNeeded)
              // {
              //   finalDistance += ShortestArrivalReroute + ShortestDepartureReroute; 
              // }
              // else if (DepartureRerouteNeeded)
              // {
              //   finalDistance += ShortestDepartureReroute;
              // }
              // else if (ArrivalRerouteNeeded)
              // {
              //   finalDistance += ShortestArrivalReroute;
              // }

              //changed to only departure reroute distance for cost
              if (DepartureRerouteNeeded) 
              {
                finalDistance += ShortestDepartureReroute;
              }

              // durationwithreroute for cost, duration for flight time
              var durationWithReroute = FlightService.CalculateFlightDuration(finalDistance, aircraft.AircraftSpeed);
              var duration = FlightService.CalculateFlightDuration(distance, aircraft.AircraftSpeed);

              var bookingCost = CalculateCost(bookingType, direction, durationWithReroute, aircraft.PricePerHour, aircraft.MinimumAcceptablePricePerTrip, aircraft.AircraftPax, pax);

              var defaultImage = aircraftsDefaultImages.FirstOrDefault(img => img.AircraftId == aircraft.AircraftId);

              SearchCharterAircraftResultDto finalAircraft = new SearchCharterAircraftResultDto()
              {
                AircraftAvailabilityId = aircraft.AircraftAvailabilityId,
                AircraftId = aircraft.AircraftId,
                Departure = possibleDepartureAirport.DisplayName,
                DepartureId = possibleDepartureAirport.Id,
                Arrival = possibleArrivalAirport.DisplayName,
                ArrivalId = possibleArrivalAirport.Id,
                FlightDurationHours = duration.Hours,
                FlightDurationMinutes = duration.Minutes,
                AircraftModel = aircraft.AircraftModel,
                AircraftPax = aircraft.AircraftPax,
                AircraftType = aircraft.AircraftType,
                AircraftArgusSafetyRating = aircraft.AircraftArgusSafetyRating,
                AircraftSpeed = aircraft.AircraftSpeed,
                AircraftRange = aircraft.AircraftRange,
                DefaultImageUrl = defaultImage == null ? "" : _config["AircraftImagesUrl"] + defaultImage.FileName,
                WiFi = aircraft.WiFi,
                BookableDemo = aircraft.BookableDemo,
                NumberOfTelevision = aircraft.NumberOfTelevision,
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                BookingType = bookingType,
                Direction = direction,
                Pax = pax
              };

              finalAircraft.TotalPrice = bookingCost.TotalCost;
              finalAircraft.ExclusiveTotalPrice = bookingCost.TotalExclusiveCost;
              finalAircraft.TotalFees = bookingCost.TotalFeesCost;
              finalAircraft.TotalTaxes = bookingCost.TotalTaxesCost;

              finalAircrafts.Add(finalAircraft);
              }
            }
          }
        }
        return finalAircrafts;
      }
    }

    public List<SearchCharterFlightResultDto> GetFeaturedFlights()
    {
      List<AircraftImage> aircraftsDefaultImages;
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var featuredFlights =  (from bookingFlight in dbContext.BookingFlights
            join booking in dbContext.Bookings on bookingFlight.BookingId equals booking.Id
            join flight in dbContext.Flights on bookingFlight.FlightId equals flight.Id
            join aircraft in dbContext.Aircrafts on flight.AircraftId equals aircraft.Id
            join model in dbContext.AircraftModels on aircraft.ModelId equals model.Id
            join type in dbContext.AircraftTypes on aircraft.TypeId equals type.Id
            join departure in dbContext.LocationsTree on flight.DepartureId equals departure.Id
            join arrival in dbContext.LocationsTree on flight.ArrivalId equals arrival.Id
            where ((flight.DepartureDate >= DateTime.Today) && flight.NumberOfSeatsAvailable > 0 && (flight.FlightType == 4 || flight.FlightType == 32)) //fix where. max passengers > numseats no good. numseats > 0 - need to change flight creation to incl numofseatsavail
            select new
            {
            FlightId = flight.Id,
            AircraftId = aircraft.Id,
            Departure = departure.DisplayName,
            DepartureId = flight.DepartureId,
            Duration = flight.Duration,
            Arrival = arrival.DisplayName,
            ArrivalId = flight.ArrivalId,
            AircraftType = type.Name,
            AircraftModel = model.Name,
            AircraftPax = aircraft.MaxPassengers,
            AircraftArgusSafetyRating = aircraft.ArgusSafetyRating,
            BookingPax = booking.NumPax,
            AircraftSpeed = aircraft.Speed,
            AircraftRange = aircraft.Range,
            ExclusiveCost = booking.TotalExclusiveCost, // for specific booking. check how this number is found
           // aircraft.BookableDemo,
            DepartureDate = flight.DepartureDate,
            })
        .ToList();

        //create List of objects where each booking's number of passengers is condensed per flight

        List<BookingTrackerItemDto> bookingTracker = new List<BookingTrackerItemDto>();
        foreach (dynamic flight in featuredFlights)
        {
          var index = bookingTracker.FindIndex(e => e.FlightId == flight.FlightId);
          if (index == -1 || bookingTracker.Count==0)
          {
            if (flight.BookingPax == 0)
            {
              BookingTrackerItemDto booking = new BookingTrackerItemDto()
              {
                FlightId = flight.FlightId,
                NumPax = 1
              };
              bookingTracker.Add(booking);
            }
            else
            {
              BookingTrackerItemDto booking = new BookingTrackerItemDto()
              {
                FlightId = flight.FlightId,
                NumPax = flight.BookingPax
              };
            bookingTracker.Add(booking);
            }
          }
          else
          {
            bookingTracker[index].NumPax += flight.BookingPax;
          }
        }

        // add booked pax to flight db context
        foreach ( BookingTrackerItemDto booking in bookingTracker) 
        {
          // var flight = (from flights in dbContext.Flights
          //               join aircraft in dbContext.Aircrafts on flights.AircraftId equals aircraft.Id
          //               where flights.Id == bookingFlightId
          //               select new {
          //                 NumberOfSeats = flights.NumberOfSeats,
          //                 NumberOfSeatsAvailable = flights.NumberOfSeatsAvailable,
          //                 AircraftNumberOfSeats = aircraft.NumberOfSeats
          //               });
          var flight = dbContext.Flights.First(f => f.Id == booking.FlightId);
          System.Console.WriteLine(flight);
          var aircraft = dbContext.Aircrafts.First(ac => ac.Id == flight.AircraftId);

          short MaxPassengers = aircraft.MaxPassengers;
          short BookedPassengers = booking.NumPax;

          flight.NumberOfSeats = aircraft.MaxPassengers;
          flight.NumberOfSeatsAvailable = (short)(MaxPassengers - BookedPassengers);
          dbContext.SaveChanges();

        }

        List<SearchCharterFlightResultDto> finalFeaturedFlights = new List<SearchCharterFlightResultDto>();

        var aircraftsIds = featuredFlights.Select(a => a.AircraftId).ToList();

        aircraftsDefaultImages = (from image in dbContext.AircraftImages
            where image.Order == 1
            && aircraftsIds.Any(id => id == image.AircraftId)
            select image)
          .ToList();
        // filter final flights to only ones with less passengers than max, add pics, return final list
        foreach(var flight in featuredFlights)
        {
          if ((finalFeaturedFlights.FindIndex(e => e.EmptyLegId == flight.FlightId) == -1) && (flight.AircraftPax > bookingTracker.Find(e => e.FlightId == flight.FlightId).NumPax))
          {
          // BookingPaymentDto bookingCost; 
          // bookingCost = CalculateCost(flight.ExclusiveCost / (decimal)flight.BookingPax);
          var bookingCost = flight.ExclusiveCost / flight.BookingPax;

          var defaultImage = aircraftsDefaultImages.FirstOrDefault(img => img.AircraftId == flight.AircraftId);
          var numBookedPax = bookingTracker.Find(b => b.FlightId == flight.FlightId).NumPax;

          SearchCharterFlightResultDto finalFeaturedFlight = new SearchCharterFlightResultDto()
          {
            EmptyLegId = flight.FlightId, // using empty leg terminology to not make a new DTO
            AircraftId = flight.AircraftId,
            Departure = flight.Departure,
            DepartureId = flight.DepartureId,
            Arrival = flight.Arrival,
            ArrivalId = flight.ArrivalId,
            AircraftType = flight.AircraftType,
            AircraftModel =flight.AircraftModel,
            AircraftPax = flight.AircraftPax,
            AircraftSpeed = flight.AircraftSpeed,
            AircraftRange = flight.AircraftRange,
            AircraftArgusSafetyRating = flight.AircraftArgusSafetyRating,
           // aircraft.BookableDemo,
            FlightDurationHours = flight.Duration.Hours,
            FlightDurationMinutes = flight.Duration.Minutes,
            DepartureDate = flight.DepartureDate,
            DefaultImageUrl = defaultImage == null ? "" : _config["AircraftImagesUrl"] + defaultImage.FileName,
            Pax = numBookedPax,
            TotalPrice = (decimal)bookingCost

          };

          // finalFeaturedFlight.TotalPrice = bookingCost.TotalCost;
          // finalFeaturedFlight.ExclusiveTotalPrice = bookingCost.TotalExclusiveCost;
          // finalFeaturedFlight.TotalFees = bookingCost.TotalFeesCost;
          // finalFeaturedFlight.TotalTaxes = bookingCost.TotalTaxesCost;

          finalFeaturedFlights.Add(finalFeaturedFlight);
        }

        }
        return finalFeaturedFlights;
      }
    }
    public List<SearchCharterFlightResultDto> SearchCharterFlights(int departureId, int arrivalId, DateTime departureDate,
        DateTime? returnDate, short pax, byte bookingType, byte direction)
    {
      var departureLocation = _locationService.GetLocation(departureId);
      var arrivalLocation = _locationService.GetLocation(arrivalId);
      List<AircraftImage> aircraftsDefaultImages;

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var emptyLegs = (from emptyLeg in dbContext.EmptyLegs
            join aircraft in dbContext.Aircrafts on emptyLeg.AircraftId equals aircraft.Id
            join model in dbContext.AircraftModels on aircraft.ModelId equals model.Id
            join type in dbContext.AircraftTypes on aircraft.TypeId equals type.Id
            join departure in dbContext.LocationsTree on emptyLeg.DepartureAirportId equals departure.Id
            join arrival in dbContext.LocationsTree on emptyLeg.ArrivalAirportId equals arrival.Id
            where (emptyLeg.DepartureAirportId == departureId)
            && (emptyLeg.ArrivalAirportId == arrivalId)
            && (emptyLeg.DepartureDate == departureDate)
            select new
            {
            EmptyLegId = emptyLeg.Id,
            AircraftId = aircraft.Id,
            AircraftModel = model.Name,
            AircraftPax = aircraft.MaxPassengers,
            AircraftType = type.Name,
            AircraftArgusSafetyRating = aircraft.ArgusSafetyRating,
            AircraftSpeed = aircraft.Speed,
            AircraftRange = aircraft.Range,
            emptyLeg.ExclusiveCost,
            aircraft.WiFi,
            aircraft.BookableDemo,
            aircraft.NumberOfTelevision,
            DepartureId = emptyLeg.DepartureAirportId,
            DepartureName = departure.DisplayName,
            ArrivalId = emptyLeg.ArrivalAirportId,
            ArrivalName = arrival.DisplayName,
            emptyLeg.Distance,
            emptyLeg.Duration,
            emptyLeg.DepartureDate,
            emptyLeg.ReturnDate
            })
        .ToList();


        List<SearchCharterFlightResultDto> finalEmptyLegs = new List<SearchCharterFlightResultDto>();

        var aircraftsIds = emptyLegs.Select(a => a.AircraftId).ToList();

        aircraftsDefaultImages = (from image in dbContext.AircraftImages
            where image.Order == 1
            && aircraftsIds.Any(id => id == image.AircraftId)
            select image)
          .ToList();

        foreach(var emptyLeg in emptyLegs)
        {
          BookingPaymentDto bookingCost; 
          if (bookingType == (byte)BookingTypes.CharterFlight)
          {
            bookingCost = CalculateCost(emptyLeg.ExclusiveCost);
          } else
          {
            bookingCost = CalculateCost(emptyLeg.ExclusiveCost / emptyLeg.AircraftPax);
          }

          var defaultImage = aircraftsDefaultImages.FirstOrDefault(img => img.AircraftId == emptyLeg.AircraftId);

          SearchCharterFlightResultDto finalEmptyLeg = new SearchCharterFlightResultDto()
          {
            EmptyLegId = emptyLeg.EmptyLegId,
                       AircraftId = emptyLeg.AircraftId,
                       Departure = emptyLeg.DepartureName,
                       DepartureId = emptyLeg.DepartureId,
                       Arrival = emptyLeg.ArrivalName,
                       ArrivalId = emptyLeg.ArrivalId,
                       FlightDurationHours = emptyLeg.Duration.Hours,
                       FlightDurationMinutes = emptyLeg.Duration.Minutes,
                       AircraftModel = emptyLeg.AircraftModel,
                       AircraftPax = emptyLeg.AircraftPax,
                       AircraftType = emptyLeg.AircraftType,
                       AircraftArgusSafetyRating = emptyLeg.AircraftArgusSafetyRating,
                       AircraftSpeed = emptyLeg.AircraftSpeed,
                       AircraftRange = emptyLeg.AircraftRange,
                       DefaultImageUrl = defaultImage == null ? "" : _config["AircraftImagesUrl"] + defaultImage.FileName,
                       WiFi = emptyLeg.WiFi,
                       BookableDemo = emptyLeg.BookableDemo,
                       NumberOfTelevision = emptyLeg.NumberOfTelevision,
                       DepartureDate = emptyLeg.DepartureDate,
                       ReturnDate = emptyLeg.ReturnDate,
                       BookingType = bookingType,
                       Direction = direction,
                       Pax = pax
          };

          finalEmptyLeg.TotalPrice = bookingCost.TotalCost;
          finalEmptyLeg.ExclusiveTotalPrice = bookingCost.TotalExclusiveCost;
          finalEmptyLeg.TotalFees = bookingCost.TotalFeesCost;
          finalEmptyLeg.TotalTaxes = bookingCost.TotalTaxesCost;

          finalEmptyLegs.Add(finalEmptyLeg);
        }

        return finalEmptyLegs;
      }
    }


    public BookingFlight GetBookingFlight(Guid bookingFlightId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        return dbContext.BookingFlights
          .Include(bf => bf.Booking.Flyer)
          .Include(bf => bf.Flight.Departure)
          .Include(bf => bf.Flight.Arrival)
          .Include(bf => bf.Flight.Aircraft.Model)
          .Include(bf => bf.Flight.Aircraft.Images)
          .FirstOrDefault(bf => bf.Id == bookingFlightId);
      }
    }

    public dynamic GetSingleFlight(Guid flightId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var queriedFlight = dbContext.Flights
                                .Include(flight => flight.Departure)
                                .Include(flight => flight.Arrival)
                                .Include(flight => flight.Aircraft.Model)
                                .Include(flight => flight.Aircraft.Images)
                                .Include(flight => flight.Aircraft.Type)
                                .FirstOrDefault(flight => flight.Id == flightId);
        
        return queriedFlight;

      }
    }
  }
}
