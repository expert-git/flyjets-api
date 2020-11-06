using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlyJetsV2.Services;
using FlyJetsV2.Services.Dtos;
using FlyJetsV2.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlyJetsV2.WebApi.Controllers
{
    [Route("api/aircrafts")]
    [ApiController]
    public class AircraftsController : ControllerBase
    {
        private AircraftService _aircraftService;
        private IConfiguration _config;

        public AircraftsController(AircraftService aircraftService, IConfiguration config)
        {
            _aircraftService = aircraftService;
            _config = config;
        }

        [Authorize]
        [HttpGet]
        [Route("autocomplete/{aircraftProviderId}/{tailNumber}")]
        public JsonResult Search(Guid aircraftProviderId, string tailNumber)
        {
            var aircrafts = _aircraftService.Search(aircraftProviderId, tailNumber);

            return new JsonResult(aircrafts.Select(aircraft => new
            {
                id = aircraft.Id,
                name = aircraft.TailNumber,
            }));
        }

        [Authorize]
        [Route("save", Name = "SaveAircraft")]
        [HttpPost]
        public IActionResult Save(SaveAircraftModel model)
        {
            if (model.AircraftId.HasValue)
            {
                _aircraftService.Update(model.AircraftId.Value, model.TailNumber, model.TypeId, model.ModelId, model.HomebaseId, model.ArgusSafetyRating,
                    model.WyvernSafetyRating, model.ManufactureYear, model.LastIntRefurbish, model.LastExtRefurbish, model.MaxPassengers,
                    model.HoursFlown, model.Speed, model.Range, model.WiFi, model.BookableDemo, model.NumberOfTelevision,
                    model.CargoCapability, model.SellAsCharterAircraft, model.SellAsCharterSeat, model.PricePerHour, model.Images, 
                    model.Documents);
            }
            else
            {
                _aircraftService.Create(model.TailNumber, model.TypeId, model.ModelId, model.HomebaseId, model.ArgusSafetyRating,
                    model.WyvernSafetyRating, model.ManufactureYear, model.LastIntRefurbish, model.LastExtRefurbish, model.MaxPassengers,
                    model.HoursFlown, model.Speed, model.Range, model.WiFi, model.BookableDemo, model.NumberOfTelevision,
                    model.CargoCapability, model.SellAsCharterAircraft, model.SellAsCharterSeat, model.PricePerHour, model.Images,
                    model.Documents);
            }

            return Ok();
        }


        [Authorize]
        [HttpGet]
        [Route("{aircraftId}")]
        public JsonResult GetAircraft(Guid aircraftId)
        {
            var aircraft = _aircraftService.Get(aircraftId);

            return new JsonResult(new
            {
                aircraftId = aircraft.Id,
                modelName = aircraft.Model.Name,
                aircraft.ModelId,
                typeName = aircraft.Type.Name,
                aircraft.TypeId,
                aircraft.TailNumber,
                HomebaseName = aircraft.HomeBase.Name,
                HomebaseId = aircraft.HomeBase.Id,
                aircraft.PricePerHour,
                aircraft.ArgusSafetyRating,
                aircraft.WyvernSafetyRating,
                aircraft.ManufactureYear,
                aircraft.LastExtRefurbish,
                aircraft.LastIntRefurbish,
                aircraft.MaxPassengers,
                aircraft.HoursFlown,
                aircraft.Range,
                aircraft.Speed,
                aircraft.CargoCapability,
                wifi = aircraft.WiFi,
                bookableDemo = aircraft.BookableDemo,
                aircraft.NumberOfTelevision,
                images = aircraft.Images.OrderBy(img => img.Order)
                            .Select(img => new
                            {
                                img.Name,
                                img.Order,
                                Url = _config["AircraftImagesUrl"] + img.FileName
                            })
            });
        }

        [Authorize]
        [HttpGet]
        [Route("list")]
        public JsonResult GetAircrafts()
        {
            var aircrafts = _aircraftService.GetProviderAircrafts();

            return new JsonResult(aircrafts.Select(aircraft => new
            {
                aircraftId = aircraft.Id,
                modelName = aircraft.Model.Name,
                typeName = aircraft.Type.Name,
                aircraft.TailNumber,
                HomebaseName = aircraft.HomeBase.Name,
                aircraft.ArgusSafetyRating,
                aircraft.WyvernSafetyRating,
                aircraft.ManufactureYear,
                aircraft.LastExtRefurbish,
                aircraft.LastIntRefurbish,
                aircraft.MaxPassengers,
                aircraft.HoursFlown,
                aircraft.Range,
                aircraft.Speed,
                aircraft.CargoCapability,
                aircraft.WiFi,
                aircraft.BookableDemo,
                aircraft.NumberOfTelevision,
                images = aircraft.Images.OrderBy(img => img.Order)
                            .Select(img => new
                            {
                                img.Name,
                                img.Order,
                                img.FileName
                            })
            })
            .ToList());
        }

        [HttpGet]
        [Route("models/{aircraftTypeId}/{keyword}")]
        public JsonResult GetAircraftModels(Guid aircraftTypeId, string keyword)
        {
            var models = _aircraftService.GetModels(aircraftTypeId, keyword);

            return new JsonResult(models.Select(model =>
            new
            {
                model.Id,
                model.Name
            })
            .ToList());
        }

        [HttpGet]
        [Route("types")]
        public JsonResult GetAircraftTypes()
        {
            var types = _aircraftService.GetTypes();

            return new JsonResult(types.Select(type =>
            new
            {
                type.Id,
                type.Name
            })
            .ToList());
        }

        [HttpGet]
        [Route("types/{keyword}")]
        public JsonResult GetAircraftTypes(string keyword)
        {
            var types = _aircraftService.GetTypes(keyword);

            return new JsonResult(types.Select(type =>
            new
            {
                type.Id,
                type.Name
            })
            .ToList());
        }

        [Authorize]
        [HttpPost]
        [Route("uploadfiles")]
        public JsonResult UploadAircraftFiles()
        {
            //string uploadedFileName = string.Empty;
            //byte[] fileBytes = null;
            //string fileExt = null;
            //Dictionary<string, string> uploadedPhotos = new Dictionary<string, string>();
            //List<string> uploadedAirwothinessCertificates = new List<string>();
            //List<string> uploadedInsuranceDocuments = new List<string>();

            //foreach (var photo in model.Photos)
            //{
            //    if (photo.File.Length != 0)
            //    {
            //        using (var ms = new MemoryStream())
            //        {
            //            photo.File.CopyTo(ms);
            //            fileBytes = ms.ToArray();
            //        }

            //        fileExt = photo.File.FileName.Substring(photo.File.FileName.LastIndexOf('.'));

            //        uploadedFileName = _aircraftService.SaveImage(fileBytes, fileExt);

            //        uploadedPhotos.Add(photo.Name, uploadedFileName);
            //    }
            //}

            //foreach (var doc in model.AirwothinessCertificates)
            //{
            //    if (doc.File.Length != 0)
            //    {
            //        using (var ms = new MemoryStream())
            //        {
            //            doc.File.CopyTo(ms);
            //            fileBytes = ms.ToArray();
            //        }

            //        fileExt = doc.File.FileName.Substring(doc.File.FileName.LastIndexOf('.'));

            //        uploadedFileName = _aircraftService.SaveDocument(fileBytes, fileExt);

            //        uploadedAirwothinessCertificates.Add(uploadedFileName);
            //    }
            //}

            //foreach (var doc in model.InsuranceDocuments)
            //{
            //    if (doc.File.Length != 0)
            //    {
            //        using (var ms = new MemoryStream())
            //        {
            //            doc.File.CopyTo(ms);
            //            fileBytes = ms.ToArray();
            //        }

            //        fileExt = doc.File.FileName.Substring(doc.File.FileName.LastIndexOf('.'));

            //        uploadedFileName = _aircraftService.SaveDocument(fileBytes, fileExt);

            //        uploadedInsuranceDocuments.Add(uploadedFileName);
            //    }
            //}

            //return new JsonResult(new
            //{
            //    photos = uploadedPhotos,
            //    airwothinessCertificates = uploadedAirwothinessCertificates,
            //    insuranceDocuments = uploadedInsuranceDocuments
            //});

            return new JsonResult(new
            {

            });
        }

        [Authorize]
        [HttpPost]
        [Route("uploadphoto")]
        public JsonResult UploadAircraftPhotos([FromForm] AircraftFile photo)
        {
            string uploadedFileName = string.Empty;
            byte[] fileBytes = null;
            string fileExt = null;

            if (photo.File.Length != 0)
            {
                using (var ms = new MemoryStream())
                {
                    photo.File.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                fileExt = photo.File.FileName.Substring(photo.File.FileName.LastIndexOf('.'));

                uploadedFileName = _aircraftService.SaveImage(fileBytes, fileExt);
            }

            return new JsonResult(new
            {
                name = photo.Name,
                fileName = uploadedFileName
            });
        }

        [Authorize]
        [HttpPost]
        [Route("uploaddocument")]
        public JsonResult UploadAircraftDocument([FromForm] AircraftFile document)
        {
            string uploadedFileName = string.Empty;
            byte[] fileBytes = null;
            string fileExt = null;

            if (document.File.Length != 0)
            {
                using (var ms = new MemoryStream())
                {
                    document.File.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                fileExt = document.File.FileName.Substring(document.File.FileName.LastIndexOf('.'));

                uploadedFileName = _aircraftService.SaveDocument(fileBytes, fileExt);
            }

            return new JsonResult(new
            {
                fileName = uploadedFileName
            });
        }

        [Authorize]
        [HttpPost]
        [Route("availability/save")]
        public IActionResult SaveAircraftAvailability(AircraftAvailabilityModel model)
        {
            if (model.AircraftAvailabilityId.HasValue)
            {
                _aircraftService.UpdateAvailability(model.AircraftAvailabilityId.Value, model.AircraftId,
                    model.ReroutingRadius,
                    model.DepartureLocations,
                    model.ArrivalLocations,
                    model.Periods,
                    model.PricePerHour, model.MinimumAcceptablePrice, model.SellCharterSeat);
            }
            else
            {
                _aircraftService.CreateAvailability(model.AircraftId,
                    model.ReroutingRadius,
                    model.DepartureLocations,
                    model.ArrivalLocations,
                    model.Periods,
                    model.PricePerHour, model.MinimumAcceptablePrice, model.SellCharterSeat);
            }

            return Ok();
        }

        [Authorize]
        [HttpPatch]
        [Route("availability/remove")]
        public JsonResult RemoveAircraftAvailability(EditAircraftAvailabilityModel model)
        {
          var availableAircrafts = _aircraftService.SetAircraftUnavailable(model.aircraftId);

          return new JsonResult(new {
              aircrafts = availableAircrafts
          });
        }

        [Authorize]
        [HttpPatch]
        [Route("availability/remove-availability")]
        public JsonResult RemoveAvailability(RemoveAvailabilityModel model)
        {
          var availabilities = _aircraftService.SetAvailabilityUnavailable(model.availabilityId);

          return new JsonResult(new {
              availabilities = availabilities
          });
        }

        [Authorize]
        [HttpGet]
        [Route("availability/list")]
        public JsonResult GetAvailability()
        {
            var availability = _aircraftService.GetAvailability();

            return new JsonResult(availability.Select(av => new
            {
                availabilityId = av.Id,
                aircraftNumber = av.Aircraft.TailNumber,
                aircraftModel = av.Aircraft.Model.Name,
                aircraftType = av.Aircraft.Type.Name
            })
            .ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("availability/{availabilityId}")]
        public JsonResult GetAvailability(Guid availabilityId)
        {
            var availability = _aircraftService.GetAvailability(availabilityId);

            return new JsonResult(new
            {
                availabilityId = availability.Id,
                availability.AircraftId,
                aircraftNumber = availability.Aircraft.TailNumber,
                availability.ReroutingRadius,
                availability.PricePerHour,
                availability.MinimumAcceptablePricePerTrip,
                departureLocations = availability.Locations
                                      .Where(loc => loc.IsForDeparture == true && loc.Rerouting == false)
                                      .Select(loc => new
                                      {
                                          loc.LocationTreeId,
                                          loc.Location.DisplayName
                                      })
                                      .ToList(),
                arrivalLocations = availability.Locations
                                      .Where(loc => loc.IsForDeparture == false && loc.Rerouting == false)
                                      .Select(loc => new
                                      {
                                          loc.LocationTreeId,
                                          loc.Location.DisplayName
                                      })
                                      .ToList(),
                periods = availability.Periods
                                      .Select(period => new
                                      {
                                          period.Id,
                                          from = period.From.ToString("MM/dd/yyy"),
                                          to = period.To.ToString("MM/dd/yyy")
                                      }).ToList()
            });
        }
    }
}
