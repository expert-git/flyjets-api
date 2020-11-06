using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FlyJetsV2.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using FlyJetsV2.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FlyJetsV2.Services.Hubs
{
  public class NotificationHub : Hub 
  {
    private IConfiguration _config;

    public NotificationHub(IConfiguration config)
    {
      _config = config;
    }

    public void NewNetworkSignup(List<Notification> notifications)
    {
      try
      {
        Clients.All.SendAsync("New Notification", notifications);
      } catch (Exception e) 
      {
        Console.WriteLine(e);
      }
    }

    public void SendNotification(Notification notification)
    {
      Clients.All.SendAsync("Notification Received", JsonConvert.SerializeObject(notification));
    }
  }
}

