using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FlyJetsV2.Data;
using FlyJetsV2.Services;
using FlyJetsV2.WebApi.Models;

namespace FlyJetsV2.WebApi.Controllers
{
  [Route("api/history")]
  [ApiController]
  public class SearchHistoryController : ControllerBase
  {
    private SearchHistoryService _searchHistoryService;
    private IConfiguration _config;

    public SearchHistoryController(SearchHistoryService searchHistoryService, IConfiguration config)
    {
      _searchHistoryService = searchHistoryService;
      _config = config;

    }
    [Authorize]
    [Route("create", Name="SaveSearch")]
    [HttpPost]
    public IActionResult Create(SearchHistoryModel model)
    {
      var result = _searchHistoryService.Create(model.DepartureId, model.ArrivalId, model.DepartureDate, model.ArrivalDate, model.BookingType, model.Passengers);
      if (result.IsSuccessfull == true) {
        return Ok();
      } else {
        return BadRequest();
      }
    }

    [Authorize]
    [Route("list", Name="GetSearches")]
    [HttpGet]
    public List<SearchHistory> GetList()
    {
      var list = _searchHistoryService.GetSearchHistories();

      return list;
    }
  }
}
