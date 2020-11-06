using FlyJetsV2.Data;
using FlyJetsV2.Services.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlyJetsV2.Services
{
    public class AircraftService
    {

        private IConfiguration _config;
        private LocationService _locationService;
        private IHttpContextAccessor _httpContextAccessor;
        private Guid _accountId;
        private FlightService _flightService;
        private StorageManager _storageManager;

        public AircraftService(IConfiguration config, LocationService locationService,
            IHttpContextAccessor httpContextAccessor,
            FlightService flightService, StorageManager storageManager)
        {
            _config = config;
            _locationService = locationService;
            _httpContextAccessor = httpContextAccessor;
            _flightService = flightService;
            _storageManager = storageManager;

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
            }
        }


        public List<Aircraft> Search(Guid aircraftProviderId, string tailNumber)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return (from aircraft in dbContext.Aircrafts
                        where aircraft.ProviderId == aircraftProviderId
                            && aircraft.TailNumber.StartsWith(tailNumber)
                        select aircraft)
                        .ToList();
            }
        }

        //public List<AircraftAvailabilityResultDto> SearchAvailability2(int departureId, int arrivalId, DateTime departureDate,
        //    DateTime? returnDate, short pax, byte bookingType, byte direction)
        //{
        //    using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
        //    {
        //        var departure = _locationService.GetLocation(departureId);
        //        var arrival = _locationService.GetLocation(arrivalId);

        //        var distance = Utilities.GetDistance(departure.Lat.Value, departure.Lng.Value, arrival.Lat.Value, arrival.Lng.Value) / 1852;

        //        var aircrafts = (from aircraft in dbContext.Aircrafts
        //                         join model in dbContext.AircraftModels on aircraft.ModelId equals model.Id
        //                         join type in dbContext.AircraftTypes on aircraft.TypeId equals type.Id
        //                         join homebase in dbContext.LocationsTree on aircraft.HomeBaseId equals homebase.Id
        //                         join availability in dbContext.AircraftsAvailability on aircraft.Id equals availability.AircraftId
        //                         where dbContext.AircraftAvailabilityLocations
        //                                    .Any(aal => aal.AircraftAvailabilityId == availability.Id
        //                                            && aal.IsForDeparture == true
        //                                            && (
        //                                                aal.Location.LocationId == departure.LocationId
        //                                                ||
        //                                                (aal.Location.LocationId == null && aal.Location.CityId == departure.CityId)
        //                                                ||
        //                                                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId.HasValue && aal.Location.StateId == departure.StateId)
        //                                                ||
        //                                                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId == null && aal.Location.CountryId == departure.CountryId)
        //                                                ))
        //                                &&
        //                                    dbContext.AircraftAvailabilityLocations
        //                                    .Any(aal => aal.AircraftAvailabilityId == availability.Id
        //                                            && aal.IsForDeparture == false
        //                                            && (
        //                                                aal.Location.LocationId == arrival.LocationId
        //                                                ||
        //                                                (aal.Location.LocationId == null && aal.Location.CityId == arrival.CityId)
        //                                                ||
        //                                                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId.HasValue && aal.Location.StateId == arrival.StateId)
        //                                                ||
        //                                                (aal.Location.LocationId == null && aal.Location.CityId == null && aal.Location.StateId == null && aal.Location.CountryId == arrival.CountryId)
        //                                                ))
        //                             &&
        //                                dbContext.AircraftsAvailabilityPeriods
        //                                .Any(aap => aap.AircraftAvailabilityId == availability.Id
        //                                            && aap.From <= departureDate && aap.To >= departureDate)
        //                             &&
        //                                (returnDate.HasValue == false
        //                                ||
        //                                dbContext.AircraftsAvailabilityPeriods
        //                                .Any(aap => aap.AircraftAvailabilityId == availability.Id
        //                                            && aap.From <= departureDate && aap.To >= departureDate))
        //                             && ((bookingType == (byte)BookingTypes.CharterAircraft)
        //                                 ||
        //                                 (bookingType == (byte)BookingTypes.CharterSeat && availability.SellCharterSeat == true))
        //                         select new
        //                         {
        //                             AircraftAvailabilityId = availability.Id,
        //                             AircraftId = aircraft.Id,
        //                             Departure = departure.DisplayName,
        //                             Arrival = arrival.DisplayName,
        //                             AircraftModel = model.Name,
        //                             AircraftPax = aircraft.MaxPassengers,
        //                             AircraftType = type.Name,
        //                             AircraftArgusSafetyRating = aircraft.ArgusSafetyRating,
        //                             AircraftSpeed = aircraft.Speed,
        //                             AircraftRange = aircraft.Range,
        //                             PricePerHour = availability.PricePerHour,
        //                             MinimumAcceptablePricePerTrip = availability.MinimumAcceptablePricePerTrip,
        //                             HomeBaseId = aircraft.HomeBaseId,
        //                             HomeBaseLat = homebase.Lat,
        //                             HomeBaseLng = homebase.Lng,
        //                             WiFi = aircraft.WiFi,
        //                             NumberOfTelevision = aircraft.NumberOfTelevision
        //                         })
        //                .ToList();


        //        List<AircraftAvailabilityResultDto> finalAircrafts = new List<AircraftAvailabilityResultDto>();

        //        var aircraftsIds = aircrafts.Select(a => a.AircraftId).ToList();
        //        var aircraftsDefaultImages = (from image in dbContext.AircraftImages
        //                                      where image.Order == 1
        //                                            && aircraftsIds.Any(id => id == image.AircraftId)
        //                                      select new { AircraftId = image.AircraftId, FileName = image.FileName })
        //                                      .ToList();

        //        foreach (var aircraft in aircrafts)
        //        {
        //            if (departureId != aircraft.HomeBaseId)
        //            {
        //                var homebaseGC = new { aircraft.HomeBaseLat, aircraft.HomeBaseLng };

        //                var reroutingDestance = Utilities.GetDistance(homebaseGC.HomeBaseLat.Value, homebaseGC.HomeBaseLng.Value, departure.Lat.Value, departure.Lng.Value) / 1852;

        //                distance += reroutingDestance;
        //            }

        //            var duration = _flightService.CalculateFlightDuration(distance, aircraft.AircraftSpeed);

        //            var bookingCost = _bookingService.CalculateCost(bookingType, direction, duration,
        //                aircraft.PricePerHour, aircraft.MinimumAcceptablePricePerTrip, aircraft.AircraftPax);

        //            var defaultImage = aircraftsDefaultImages.FirstOrDefault(img => img.AircraftId == aircraft.AircraftId);

        //            AircraftAvailabilityResultDto finalAircraft = new AircraftAvailabilityResultDto()
        //            {
        //                AircraftAvailabilityId = aircraft.AircraftAvailabilityId,
        //                AircraftId = aircraft.AircraftId,
        //                Departure = aircraft.Departure,
        //                Arrival = aircraft.Arrival,
        //                FlightDurationHours = duration.Hours,
        //                FlightDurationMinutes = duration.Minutes,
        //                AircraftModel = aircraft.AircraftModel,
        //                AircraftPax = aircraft.AircraftPax,
        //                AircraftType = aircraft.AircraftType,
        //                AircraftArgusSafetyRating = aircraft.AircraftArgusSafetyRating,
        //                AircraftSpeed = aircraft.AircraftSpeed,
        //                AircraftRange = aircraft.AircraftRange,
        //                DefaultImageUrl = defaultImage == null ? "" : _config["AircraftImagesUrl"] + defaultImage.FileName,
        //                WiFi = aircraft.WiFi,
        //                NumberOfTelevision = aircraft.NumberOfTelevision
        //            };

        //            finalAircraft.TotalPrice = bookingCost.TotalCost;
        //            finalAircraft.ExclusiveTotalPrice = bookingCost.TotalExclusiveCost;
        //            finalAircraft.TotalFees = bookingCost.TotalFeesCost;
        //            finalAircraft.TotalTaxes = bookingCost.TotalTaxesCost;

        //            finalAircrafts.Add(finalAircraft);
        //        }

        //        return finalAircrafts;
        //    }
        //}

        public Aircraft Get(Guid aircraftId)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                var aircraft = dbContext.Aircrafts
                    .Include(a => a.Model)
                    .Include(a => a.Type)
                    .Include(a => a.HomeBase)
                    .Include(a => a.Images)
                    .FirstOrDefault(a => a.Id == aircraftId);

                return aircraft;
            }
        }

        public List<Aircraft> SetAircraftUnavailable(Guid aircraftId)
        {
          using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
          {
            var aircraft = dbContext.Aircrafts
              .FirstOrDefault(a => a.Id == aircraftId);

            var availabilities = dbContext.AircraftsAvailability
              .Where(av => av.AircraftId == aircraftId);

            var emptyLegs = dbContext.EmptyLegs
              .Where(el => el.AircraftId == aircraftId);

            foreach (var availability in availabilities)
            {
              availability.Available = false;
            }

            foreach(var leg in emptyLegs)
            {
              leg.Available = false;
            }

            dbContext.Aircrafts.Attach(aircraft);

            aircraft.Available = false;

            dbContext.SaveChanges();

            return GetProviderAircrafts();
          }
        }

        public List<AircraftAvailability> SetAvailabilityUnavailable(Guid availabilityId)
        {
          using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
          {
            var availability = dbContext.AircraftsAvailability
              .FirstOrDefault(a => a.Id == availabilityId);

            dbContext.AircraftsAvailability.Attach(availability);

            availability.Available = false;

            dbContext.SaveChanges();

            return GetAvailability();
          }
        }

        public List<Aircraft> GetProviderAircrafts()
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                var aircrafts = dbContext.Aircrafts
                    .Include(a => a.Model)
                    .Include(a => a.Type)
                    .Include(a => a.HomeBase)
                    .Include(a => a.Images)
                    .Where(a => a.ProviderId == _accountId && a.Available == true)
                    .OrderByDescending(a => a.CreatedOn)
                    .ToList();

                return aircrafts;
            }
        }

        public List<AircraftModel> GetModels(Guid aircraftTypeId, string keyword = null)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return dbContext.AircraftModels
                        .Where(m => m.AircraftTypeId == aircraftTypeId)
                    .ToList();
                }
                else
                {
                    return dbContext.AircraftModels
                        .Where(m => m.AircraftTypeId == aircraftTypeId && m.Name.Contains(keyword))
                    .ToList();
                }
            }
        }

        public List<AircraftType> GetTypes(string keyword = null)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return dbContext.AircraftTypes
                    .ToList();
                }
                else
                {
                    return dbContext.AircraftTypes
                        .Where(t => t.Name.Contains(keyword))
                    .ToList();
                }
            }
        }

        public string SaveImage(byte[] file, string fileExtention)
        {
            string fileName = string.Empty;

            if (file != null && file.Length != 0)
            {
                fileName = _storageManager.UploadImage(_config["AircraftImagesContainer"], file, fileExtention, true);
            }

            return fileName;
        }

        public string SaveDocument(byte[] file, string fileExtention)
        {
            string fileName = string.Empty;

            if (file != null && file.Length != 0)
            {
                fileName = _storageManager.UploadFile(_config["AircraftDocumentsContainer"], file, fileExtention);
            }

            return fileName;
        }

        public ServiceOperationResult Create(string tailNumber, Guid typeId, Guid modelId, int HomeBaseId,
            string argusSafetyRating, string wyvernSafetyRating, short? manufactureYear, short? lastIntRefurbish,
            short? lastExtRefurbish, byte maxPassengers, short? hoursFlown, short speed, short range,
            bool wiFi, bool bookableDemo, short? numberOfTelevision, short? cargoCapability, bool sellAsCharterAircraft,
            bool sellAsCharterSeat, decimal pricePerHour, List<AircraftDocumentDto> images, List<AircraftDocumentDto> documents)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                ServiceOperationResult result = new ServiceOperationResult();
                result.IsSuccessfull = true;

                var newAircraft = new Aircraft();

                newAircraft.Id = Guid.NewGuid();
                newAircraft.TailNumber = tailNumber;
                newAircraft.TypeId = typeId;
                newAircraft.ModelId = modelId;
                newAircraft.HomeBaseId = HomeBaseId;
                newAircraft.ArgusSafetyRating = argusSafetyRating;
                newAircraft.WyvernSafetyRating = wyvernSafetyRating;
                newAircraft.ManufactureYear = manufactureYear;
                newAircraft.LastIntRefurbish = lastIntRefurbish;
                newAircraft.LastExtRefurbish = lastExtRefurbish;
                newAircraft.MaxPassengers = maxPassengers;
                newAircraft.HoursFlown = hoursFlown;
                newAircraft.Speed = speed;
                newAircraft.Range = range;
                newAircraft.WiFi = wiFi;
                newAircraft.BookableDemo = bookableDemo;
                newAircraft.Television = numberOfTelevision.HasValue;
                newAircraft.NumberOfTelevision = numberOfTelevision;
                newAircraft.CargoCapability = cargoCapability;
                newAircraft.SellAsCharterAircraft = true;//sellAsCharterAircraft;
                newAircraft.SellAsCharterSeat = true; //sellAsCharterSeat;
                newAircraft.PricePerHour = pricePerHour;
                newAircraft.Available = true;

                newAircraft.ProviderId = _accountId;
                newAircraft.CreatedById = _accountId;
                newAircraft.CreatedOn = DateTime.UtcNow;

                newAircraft.Images = new List<AircraftImage>();

                foreach (var image in images)
                {
                    newAircraft.Images.Add(new AircraftImage()
                    {
                        Id = Guid.NewGuid(),
                        AircraftId = newAircraft.Id,
                        Name = image.Name,
                        FileName = image.FileName,
                        Order = image.Order
                    });
                }

                newAircraft.Documents = new List<AircraftDocument>();

                foreach (var document in documents)
                {
                    newAircraft.Documents.Add(new AircraftDocument()
                    {
                        Id = Guid.NewGuid(),
                        AircraftId = newAircraft.Id,
                        FileName = document.FileName,
                        Type = document.Type,
                        Name = "Document"
                    });
                }


                dbContext.Aircrafts.Add(newAircraft);
                dbContext.SaveChanges();

                return result;
            }
        }


        public ServiceOperationResult Update(Guid aircraftId, string tailNumber, Guid typeId, Guid modelId, int HomeBaseId,
            string argusSafetyRating, string wyvernSafetyRating, short? manufactureYear, short? lastIntRefurbish,
            short? lastExtRefurbish, byte maxPassengers, short? hoursFlown, short speed, short range,
            bool wiFi, bool bookableDemo, short? numberOfTelevision, short? cargoCapability, bool sellAsCharterAircraft,
            bool sellAsCharterSeat, decimal pricePerHour, List<AircraftDocumentDto> images, List<AircraftDocumentDto> documents)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                ServiceOperationResult result = new ServiceOperationResult();
                result.IsSuccessfull = true;

                var aircraft = dbContext.Aircrafts
                    .Include("Images")
                    .Include("Documents")
                    .FirstOrDefault(a => a.Id == aircraftId);

                aircraft.TailNumber = tailNumber;
                aircraft.TypeId = typeId;
                aircraft.ModelId = modelId;
                aircraft.HomeBaseId = HomeBaseId;
                aircraft.ArgusSafetyRating = argusSafetyRating;
                aircraft.WyvernSafetyRating = wyvernSafetyRating;
                aircraft.ManufactureYear = manufactureYear;
                aircraft.LastIntRefurbish = lastIntRefurbish;
                aircraft.LastExtRefurbish = lastExtRefurbish;
                aircraft.MaxPassengers = maxPassengers;
                aircraft.HoursFlown = hoursFlown;
                aircraft.Speed = speed;
                aircraft.Range = range;
                aircraft.WiFi = wiFi;
                aircraft.BookableDemo = bookableDemo;
                aircraft.Television = numberOfTelevision.HasValue && numberOfTelevision.Value > 0 ? true : false;
                aircraft.NumberOfTelevision = numberOfTelevision;
                aircraft.CargoCapability = cargoCapability;
                aircraft.SellAsCharterAircraft = true;// sellAsCharterAircraft;
                aircraft.SellAsCharterSeat = true;// sellAsCharterSeat;
                aircraft.PricePerHour = pricePerHour;

                foreach(var image in images)
                {
                    var currentImage = aircraft.Images.FirstOrDefault(img => img.Order == image.Order);

                    if(currentImage != null)
                    {
                        currentImage.FileName = image.FileName;
                    }
                    else
                    {
                        aircraft.Images.Add(new AircraftImage()
                        {
                            Id = Guid.NewGuid(),
                            AircraftId = aircraft.Id,
                            Name = image.Name,
                            FileName = image.FileName,
                            Order = image.Order
                        });
                    }
                }

                foreach (var document in documents)
                {
                    aircraft.Documents.Add(new AircraftDocument()
                    {
                        Id = Guid.NewGuid(),
                        AircraftId = aircraft.Id,
                        FileName = document.FileName,
                        Name = "Document",
                        Type = document.Type
                    });
                }



                dbContext.SaveChanges();

                return result;
            }
        }


        public List<AircraftAvailability> GetAvailability()
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.AircraftsAvailability
                    .Include("Aircraft.Model")
                    .Include("Aircraft.Type")
                    .Where(av => av.Aircraft.ProviderId == _accountId && av.Available == true)
                    .ToList();
            }
        }

        public ServiceOperationResult CreateAvailability(Guid aircraftId, int? reroutingRadius,
            List<AircraftAvailabilityLocationDto> departureLocations,
            List<AircraftAvailabilityLocationDto> arrivalLocations, List<AircraftAvailabilityPeriodDto> availabileDates,
            decimal? pricePerHour, decimal? minimumAcceptablePrice, bool sellCharterSeat)
        {
            using (var dbContext = new FlyJetsDbContext(_config))
            {
                ServiceOperationResult result = new ServiceOperationResult();

                result.IsSuccessfull = true;

                var newAvailability = new AircraftAvailability()
                {
                    Id = Guid.NewGuid(),
                    AircraftId = aircraftId,
                    PricePerHour = pricePerHour,
                    MinimumAcceptablePricePerTrip = minimumAcceptablePrice,
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = _accountId,
                    SellCharterSeat = sellCharterSeat,
                    ReroutingRadius = reroutingRadius,
                    Available = true
                };

                newAvailability.Locations = new List<AircraftAvailabilityLocation>();

                if (reroutingRadius.HasValue)
                {
                    var homebaseLoc = (from aircraft in dbContext.Aircrafts
                                       join homebase in dbContext.LocationsTree on aircraft.HomeBaseId equals homebase.Id
                                       where aircraft.Id == aircraftId
                                       select new { Lat = homebase.Lat, Lng = homebase.Lng })
                                        .First();

                    var reroutingLocations = _locationService.GetLocationsWithinXMiles(homebaseLoc.Lat.Value, homebaseLoc.Lng.Value,
                                    reroutingRadius.Value, (byte)LocationsTypes.Airport);

                    foreach (var reroutingLocation in reroutingLocations)
                    {
                        var newLoc = new AircraftAvailabilityLocation()
                        {
                            Id = Guid.NewGuid(),
                            AircraftAvailabilityId = newAvailability.Id,
                            LocationTreeId = reroutingLocation.Id,
                            IsForDeparture = true,
                            Rerouting = true
                        };

                        newAvailability.Locations.Add(newLoc);
                    }
                }

                foreach (var departureLocation in departureLocations)
                {
                    var newLoc = new AircraftAvailabilityLocation()
                    {
                        Id = Guid.NewGuid(),
                        AircraftAvailabilityId = newAvailability.Id,
                        LocationTreeId = departureLocation.LocationTreeId,
                        IsForDeparture = true,
                        Rerouting = false
                    };

                    newAvailability.Locations.Add(newLoc);
                }

                foreach (var arrivalLocation in arrivalLocations)
                {
                    var newLoc = new AircraftAvailabilityLocation()
                    {
                        Id = Guid.NewGuid(),
                        AircraftAvailabilityId = newAvailability.Id,
                        LocationTreeId = arrivalLocation.LocationTreeId,
                        IsForDeparture = false,
                        Rerouting = false
                    };

                    newAvailability.Locations.Add(newLoc);
                }

                newAvailability.Periods = new List<AircraftAvailabilityPeriod>();

                foreach (var availableDate in availabileDates)
                {
                    newAvailability.Periods.Add(new AircraftAvailabilityPeriod()
                    {
                        Id = Guid.NewGuid(),
                        AircraftAvailabilityId = newAvailability.Id,
                        From = availableDate.From,
                        To = availableDate.To
                    });
                }

                dbContext.AircraftsAvailability.Add(newAvailability);
                dbContext.SaveChanges();

                return result;
            }
        }

        public ServiceOperationResult UpdateAvailability(Guid aircraftAvailabilityId, Guid aircraftId, int? reroutingRadius,
            List<AircraftAvailabilityLocationDto> departureLocations,
            List<AircraftAvailabilityLocationDto> arrivalLocations, List<AircraftAvailabilityPeriodDto> availabileDates,
            decimal? pricePerHour, decimal? minimumAcceptablePrice, bool sellCharterSeat)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                var operationResult = new ServiceOperationResult();
                operationResult.IsSuccessfull = true;

                var availability = dbContext.AircraftsAvailability
                    .FirstOrDefault(av => av.Id == aircraftAvailabilityId);

                if (availability == null)
                {
                    operationResult.Errors = new List<ErrorCodes>() { ErrorCodes.NotFound };
                    operationResult.IsSuccessfull = false;

                    return operationResult;
                }

                availability.AircraftId = aircraftId;
                availability.PricePerHour = pricePerHour;
                availability.MinimumAcceptablePricePerTrip = minimumAcceptablePrice;
                availability.SellCharterSeat = sellCharterSeat;
                availability.ReroutingRadius = reroutingRadius;

                var oldLocations = dbContext.AircraftAvailabilityLocations
                    .Where(loc => loc.AircraftAvailabilityId == aircraftAvailabilityId)
                    .ToList();

                foreach (var location in oldLocations)
                {
                    dbContext.Entry(location).State = EntityState.Deleted;
                }

                availability.Locations = new List<AircraftAvailabilityLocation>();

                if (reroutingRadius.HasValue)
                {
                    var homebaseLoc = (from aircraft in dbContext.Aircrafts
                                       join homebase in dbContext.LocationsTree on aircraft.HomeBaseId equals homebase.Id
                                       where aircraft.Id == aircraftId
                                       select new { Lat = homebase.Lat, Lng = homebase.Lng })
                                        .First();

                    var reroutingLocations = _locationService.GetLocationsWithinXMiles(homebaseLoc.Lat.Value, homebaseLoc.Lng.Value,
                                    reroutingRadius.Value, (byte)LocationsTypes.Airport);

                    foreach (var reroutingLocation in reroutingLocations)
                    {
                        var newLoc = new AircraftAvailabilityLocation()
                        {
                            Id = Guid.NewGuid(),
                            AircraftAvailabilityId = availability.Id,
                            LocationTreeId = reroutingLocation.Id,
                            IsForDeparture = true,
                            Rerouting = true
                        };

                        availability.Locations.Add(newLoc);
                    }
                }

                foreach (var departureLocation in departureLocations)
                {
                    var newLoc = new AircraftAvailabilityLocation()
                    {
                        Id = Guid.NewGuid(),
                        AircraftAvailabilityId = availability.Id,
                        LocationTreeId = departureLocation.LocationTreeId,
                        IsForDeparture = true,
                        Rerouting = false
                    };

                    availability.Locations.Add(newLoc);
                }

                foreach (var arrivalLocation in arrivalLocations)
                {
                    var newLoc = new AircraftAvailabilityLocation()
                    {
                        Id = Guid.NewGuid(),
                        AircraftAvailabilityId = availability.Id,
                        LocationTreeId = arrivalLocation.LocationTreeId,
                        IsForDeparture = false,
                        Rerouting = false
                    };

                    availability.Locations.Add(newLoc);
                }

                var oldPeriods = dbContext.AircraftsAvailabilityPeriods
                    .Where(p => p.AircraftAvailabilityId == aircraftAvailabilityId)
                    .ToList();

                foreach (var period in oldPeriods)
                {
                    dbContext.Entry(period).State = EntityState.Deleted;
                }

                availability.Periods = new List<AircraftAvailabilityPeriod>();

                foreach (var availableDate in availabileDates)
                {
                    availability.Periods.Add(new AircraftAvailabilityPeriod()
                    {
                        Id = Guid.NewGuid(),
                        AircraftAvailabilityId = availability.Id,
                        From = availableDate.From,
                        To = availableDate.To
                    });
                }

                dbContext.SaveChanges();

                return operationResult;
            }
        }

        public AircraftAvailability GetAvailability(Guid aircraftAvailabilityId)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                var availability = dbContext.AircraftsAvailability
                    .Include("Aircraft.Model")
                    .Include("Locations.Location.City")
                    .Include("Locations.Location.State")
                    .Include("Locations.Location.Country")
                    .Include("Periods")
                    .FirstOrDefault(av => av.Id == aircraftAvailabilityId);

                return availability;
            }
        }
    }
}
