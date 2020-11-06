using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlyJetsV2.Services;
using FlyJetsV2.Services.Dtos;
using FlyJetsV2.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FlyJetsV2.Data;

namespace FlyJetsV2.WebApi.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private BookingService _bookingService;
        private AccountService _accountService;
        private PaymentService _paymentService;
        private IConfiguration _config;

        public BookingsController(BookingService bookingService, AccountService accountService, PaymentService paymentService, IConfiguration config)
        {
            _bookingService = bookingService;
            _accountService = accountService;
            _paymentService = paymentService;
            _config = config;
        }
        [Route("flights/featured")]
        [HttpGet]
        public JsonResult GetFeaturedflights()
        {
            var featuredFlights = _bookingService.GetFeaturedFlights();
            return new JsonResult(featuredFlights);
        }

        [Route ("flights/featured/{flightId}")]
        [HttpGet]
        public JsonResult GetFeaturedFlight(Guid flightId)
        {
            var featuredFlight = _bookingService.GetSingleFlight(flightId);
            return new JsonResult(featuredFlight);
        }

        [Authorize]
        [Route("flightsrequests/create", Name = "CreateFlightRequest")]
        [HttpPost]
        public IActionResult CreateFlightRequest(FligthRequestEditModel model)
        {
            _bookingService.CreateFlightRequest(model.Direction, model.BookingType, model.DepartureId,
                model.ArrivalId, model.DepartureDate, model.ReturnDate, model.Pax,
                model.MinPrice, model.MaxPrice, model.Notes, model.AircraftType);

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("flightsrequests/list/{getCurrent}", Name = "GetFlightsRequests")]
        public JsonResult GetFlightsRequests(bool getCurrent)
        {
            var requests = _bookingService.GetFlightsRequests(getCurrent);

            return new JsonResult(requests.Select(request => new
            {
                Id = request.Id,
                Number = request.Number,
                Direction = request.Direction,
                BookingType = request.BookingType,
                Departure = request.Departure.DisplayName,
                Arrival = request.Arrival.DisplayName,
                FullName = request.Requester.FirstName + " " + request.Requester.LastName,
                DepartureDate = request.DepartureDate,
                ReturnDate = request.ReturnDate,
                Pax = request.PassengersNumber,
                AircraftType = request.AircraftType,
                MaxPrice = request.MaxPrice,
                MinPrice = request.MinPrice,
                CreatedOn = request.CreatedOn,
                Status = request.Status
            })
            .ToList());
        }

        [Authorize]
        [Route("flightsrequests/{flightRequestId}", Name = "GetFlightRequest")]
        [HttpGet]
        public JsonResult GetFlightRequest(Guid flightRequestId)
        {
            var request = _bookingService.GetFlightRequest(flightRequestId);

            return new JsonResult(new
            {
                Id = request.Id,
                Number = request.Number,
                Direction = request.Direction,
                BookingType = request.BookingType,
                Departure = request.Departure.DisplayName,
                Arrival = request.Arrival.DisplayName,
                FullName = request.Requester.FirstName + " " + request.Requester.LastName,
                DepartureDate = request.DepartureDate,
                ReturnDate = request.ReturnDate,
                Pax = request.PassengersNumber,
                AircraftType = request.AircraftType,
                MaxPrice = request.MaxPrice,
                MinPrice = request.MinPrice,
                CreatedOn = request.CreatedOn,
                Status = request.Status
            });
        }

        [Authorize]
        [HttpGet]
        [Route("list/{bookingType}/{alternative}", Name = "GetBookings")]
        public JsonResult GetBookings(byte bookingType, byte alternative)
        {
            var bookings = _bookingService.GetBookings(false, bookingType, alternative);

            return new JsonResult(bookings.Select(b => new
            {
                b.Id,
                b.Number,
                CreatedBy = b.Flyer.FirstName + " " + b.Flyer.LastName,
                b.CreatedOn
            })
            .ToList());
        }


        [Authorize]
        [HttpGet]
        [Route("list/confirmed/{bookingType}/{alternative}", Name="GetConfirmedBookings")]
        public JsonResult GetConfirmedBookings(byte bookingType, byte alternative)
        {
          var bookings = _bookingService.GetBookings(true, bookingType, alternative);

            return new JsonResult(bookings.Select(b => new
            {
                b.Id,
                b.Number,
                CreatedBy = b.Flyer.FirstName + " " + b.Flyer.LastName,
                b.CreatedOn,
                b.BookingFlights
            })
            .ToList());
        }

        [Authorize]
        [Route("createofflinebooking", Name = "CreateOfflineBooking")]
        [HttpPost]
        public IActionResult CreateOfflineBooking(CreateOfflineBookingModel model)
        {
            _bookingService.CreateOfflineBooking(model.FlightRequestId, model.AircraftProviderId,
                model.AircraftId, model.Pax, model.BookingType, model.Direction, model.OutboundFlightDepartureId,
                model.OutboundFlightArrivalId, model.OutboundFlightDepartureDate, model.OutboundFlightArrivalDate,
                model.InboundFlightDepartureId, model.InboundFlightArrivalId, model.InboundFlightDepartureDate,
                model.InboundFlightArrivalDate, model.ExclusiveBookingCost, model.BookingPax);

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("{bookingId}", Name = "GetBooking")]
        public JsonResult GetBooking(Guid bookingId)
        {
            var booking = _bookingService.GetBooking(bookingId);

            return new JsonResult(new
            {
                bookingId = booking.Id,
                booking.Number,
                booking.Direction,
                booking.BookingType,
                booking.CreatedOn
            });
        }

        [Authorize]
        [HttpGet]
        [Route("{bookingId}/flights", Name = "GetBookingFlights")]
        public JsonResult GetBookingFlights(Guid bookingId)
        {
            var bookingFlights = _bookingService.GetBookingFlights(bookingId);

            return new JsonResult(bookingFlights.Select(bf => new
            {
                bf.Flight.DepartureId,
                departure = bf.Flight.Departure.DisplayName,
                bf.Flight.DepartureDate,
                bf.Flight.DepartureTime,
                bf.Flight.ArrivalId,
                arrival = bf.Flight.Arrival.DisplayName,
                bf.Flight.ArrivalDate,
                bf.Flight.ArrivalTime,
                bf.Flight.Order,
                bf.Flight.Distance,
                bf.Flight.Duration,
                bf.Flight.NumberOfSeats,
                bf.Flight.NumberOfSeatsAvailable,
                bf.Flight.AircraftId
            })
            .ToList());
        }


        [Authorize]
        [HttpGet]
        [Route("{bookingId}/payment/calculate", Name = "CalculateOfflineBookingCost")]
        public JsonResult CalculateOfflineBookingCost(Guid bookingId)
        {
            List<ErrorCodes> errors = new List<ErrorCodes>();

            var bookingCost = _bookingService.CalculateCost(bookingId);

            return new JsonResult(new {
                bookingCost.TotalCost,
                bookingCost.TotalExclusiveCost,
                bookingCost.TotalFeesCost,
                bookingCost.TotalTaxesCost
            });
        }

        [Authorize]
        [Route("{bookingId}/confirm", Name = "ConfirmBooking")]
        [HttpPost]
        public IActionResult ConfirmBooking(Guid bookingId, ConfirmBookingModel model)
        {
            ServiceOperationResult bookingResult;

            bookingResult = _bookingService.ConfirmOfflineBooking(bookingId, model.PaymentMethodId,
                    model.Travelers.Select(t => new CreateBookingTravelerDto()
                    {
                        Email = t.Email,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        Id = t.Id
                    }).ToList());

            return Ok();
        }

        [Authorize]
        [Route("flights/list/{filterBy}", Name = "GetFlights")]
        [HttpGet]
        public ActionResult GetFlights(byte filterBy)
        {
            var bookingsFlights = _bookingService.GetBookingsFlights((FilterBookedFlightsBy)filterBy);

            return new JsonResult(bookingsFlights.Select(bf => new {
                bookingFlightId = bf.Id,
                flightId = bf.FlightId,
                departure = bf.Flight.Departure.Name,
                arrival = bf.Flight.Arrival.Name,
                departureDate = bf.Flight.DepartureDate,
                departureTime = bf.Flight.DepartureTime.HasValue ? bf.Flight.DepartureTime.Value.ToString("hh:mm tt") : string.Empty,
                arrivalDate = bf.Flight.ArrivalDate,
                arrivalTime = bf.Flight.ArrivalTime.HasValue ? bf.Flight.ArrivalTime.Value.ToString("hh:mm tt") : string.Empty,
                durationHours = bf.Flight.Duration.Hours,
                durationMinutes = bf.Flight.Duration.Minutes,
                flightNumber = bf.Flight.Number,
                bookingNumber = bf.Booking.Number,
                flyerName = bf.Booking.Flyer.FirstName + " " + bf.Booking.Flyer.LastName
          }));
        }

        [Authorize]
        [Route("edit", Name="EditBooking")]
        [HttpPatch]
        public JsonResult EditBooking(EditBookingModel model)
        {
          var bookingConfirmation = _bookingService.EditBooking(model.BookingNo, model.Travelers);

          return new JsonResult(bookingConfirmation);
        }

        [Authorize]
        [Route("create", Name = "CreateBooking")]
        [HttpPost]
        public JsonResult CreateBooking(CreateBookingModel model)
        {
          var result = _bookingService.Create(model.AircraftAvailabilityId, model.Direction, model.BookingType,
                model.DepartureId, model.ArrivalId, model.DepartureDate, model.ReturnDate, model.PassengersNum,
                model.PaymentMethodId,
                model.Travelers.Select(t => new CreateBookingTravelerDto()
                {
                    Email = t.Email,
                    FirstName = t.FirstName,
                    LastName = t.LastName
                }).ToList(),
                model.BookingPax);

            return new JsonResult(new {
                bookingNo = result.Number
                });
        }

        [Route("final-booking-info/{bookingId}", Name="FinalBookingInfo")]
        [HttpGet]
        public JsonResult FinalizeInfo(Guid bookingId)
        {
          var fullBookingInfo = _bookingService.GetFullBooking(bookingId);
          return fullBookingInfo;
        }

        [Route("finalize", Name="MailCheckout")]
        [HttpPost]
        public IActionResult SendCheckoutEmail(CheckoutModel model)
        {
          _bookingService.SendBookingConfirmationEmail(model.BookingId, model.Email, model.FirstName, model.LastName);
          return Ok();
        }


        [Route("checkout", Name="Checkout")]
        [HttpPost]
        public string Checkout(CheckoutModel model)
        {
          /* var paymentMethod = _paymentService.GetPaymentMethod(model.PaymentMethodId); var booking = _bookingService.GetBooking(model.BookingId); */
          /* var totalCost = (long)(Math.Round(booking.TotalExclusiveCost + booking.TotalFees + booking.TotalTaxes, 2) * 100); */
          /* var account = _accountService.GetAccount(model.Email); */

          /* var paymentResult = _paymentService.Charge(totalCost, "Payment for Booking #" + booking.Number, */
          /*     account.StripeCustomerId, paymentMethod.ReferencePaymentMethodId, */
          /*     new Dictionary<string, string>() { { "Booking#", booking.Number }, { "BookingId", booking.Id.ToString() } }, false); */

          /* return paymentResult; */
          return "hihihi";
        }

        [Authorize]
        [HttpPost]
        [Route("searchCharterAircrafts")]
        public JsonResult SearchCharterAircrafts(SearchCharterAircraftsModel model)
        {
            var aircrafts = _bookingService.SearchCharterAircrafts(model.DepartureId, model.ArrivalId, model.DepartureDate,
                model.ReturnDate, model.Pax, model.BookingType, model.Direction);

            return new JsonResult(aircrafts);
        }

        [Authorize]
        [HttpPost]
        [Route("searchCharterFlights")]
        public JsonResult SearchCharterFlights(SearchCharterAircraftsModel model)
        {
            var aircrafts = _bookingService.SearchCharterFlights(model.DepartureId, model.ArrivalId, model.DepartureDate,
                model.ReturnDate, model.Pax, model.BookingType, model.Direction);

            return new JsonResult(aircrafts);
        }

        [Authorize]
        [HttpGet]
        [Route("travelers/getmember/{email}")]
        public JsonResult GetMemberToAddTraveler(string email)
        {
            var account = _accountService.GetAccount(email);

            if(account == null)
            {
                return new JsonResult(null);
            }

            return new JsonResult(new {
                account.Id,
                account.FirstName,
                account.LastName,
                account.Email
            });
        }

        [Authorize]
        [Route("flights/{bookingFlightId}", Name = "GetFlight")]
        [HttpGet]
        public JsonResult GetFlight(Guid bookingFlightId)
        {
            var bookingFlight = _bookingService.GetBookingFlight(bookingFlightId);

            return new JsonResult(new {
                bookingFlight.BookingId,
                bookingNumber = bookingFlight.Booking.Number,
                flightNumber = bookingFlight.Flight.Number,
                flyerName = bookingFlight.Booking.Flyer.FirstName + ' ' + bookingFlight.Booking.Flyer.LastName,
                departure = bookingFlight.Flight.Departure.DisplayName,
                arrival = bookingFlight.Flight.Arrival.DisplayName,
                departureDate = bookingFlight.Flight.DepartureDate,
                departureTime = bookingFlight.Flight.DepartureTime.HasValue ? bookingFlight.Flight.DepartureTime.Value.ToString("hh\\:mm") : null,
                arrivalDate = bookingFlight.Flight.ArrivalDate,
                arrivalTime = bookingFlight.Flight.ArrivalTime.HasValue ? bookingFlight.Flight.ArrivalTime.Value.ToString("hh\\:mm") : null,
                durationHours = bookingFlight.Flight.Duration.Hours,
                durationMinutes = bookingFlight.Flight.Duration.Minutes,
                aircraftModel = bookingFlight.Flight.Aircraft.Model.Name,
                aircraftPhotos = bookingFlight.Flight.Aircraft.Images.OrderBy(img => img.Order)
                            .Select(img => new
                            {
                                img.Name,
                                img.Order,
                                Url = _config["AircraftImagesUrl"] + img.FileName
                            })
            });
        }
    }
}
