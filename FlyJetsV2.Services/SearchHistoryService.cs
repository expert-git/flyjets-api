using FlyJetsV2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace FlyJetsV2.Services
{
  public class SearchHistoryService
  {
    private IConfiguration _config;
    private IHttpContextAccessor _httpContextAccessor;
    private Guid _accountId;
    
    public SearchHistoryService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
      _config = config;
      _httpContextAccessor = httpContextAccessor;

      if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
      {
        _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
      }
    }
    public List<SearchHistory> GetSearchHistories()
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var histories = dbContext.SearchHistories
          .Include("Flyer")
          .Include("Departure")
          .Include("Arrival")
          .OrderByDescending(h => h.CreatedOn)
          .ToList();

        return histories;
      }
    }

    public ServiceOperationResult Create(int departureId, int arrivalId, DateTime departureDate, DateTime? arrivalDate, byte bookingType, int passengers)
    {
      var result = new ServiceOperationResult();
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var newSearch = new SearchHistory()
        {
          Id = Guid.NewGuid(),
          FlyerId = _accountId,
          DepartureId = departureId,
          DepartureDate = departureDate,
          ArrivalId = arrivalId,
          ArrivalDate = arrivalDate,
          BookingType = bookingType,
          CreatedOn = DateTime.UtcNow,
          Passengers = passengers
        };

        dbContext.SearchHistories.Add(newSearch);
        try {
          dbContext.SaveChanges();
          result.IsSuccessfull = true;
        } catch (Exception e)
        {
          result.IsSuccessfull = false;
          result.Errors = new List<ErrorCodes>() {
            ErrorCodes.UnKnown};
        }
      }
      return result;
    }
  }
}
