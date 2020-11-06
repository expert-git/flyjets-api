using FlyJetsV2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlyJetsV2.Services.Hubs;

namespace FlyJetsV2.Services
{
  public class NotificationService
  {
    private IConfiguration _config;
    private IHttpContextAccessor _httpContextAccessor;
    private NotificationHub _notificationHub;
    private Guid? _accountId = null;
    private Guid adminId;

    public NotificationService(IConfiguration config, IHttpContextAccessor httpContextAccessor, NotificationHub notificationHub)
    {
      _config = config;
      _httpContextAccessor = httpContextAccessor;
      _notificationHub = notificationHub;

      if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
      {
        _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
      }
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        adminId = dbContext.Accounts
          .Where(account => account.Type == (byte)AccountTypes.Admin)
          .Select(account => account.Id)
          .FirstOrDefault();
      }
    }

    public void NewCreate(Guid receiverId, NotificationsTypes type, string text, List<NotificationParam> @params)
    {
      var newNotification = PrepareNotification(receiverId, type, text, @params);

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        //send notification to database
        dbContext.Notifications.Add(newNotification);

        dbContext.SaveChanges();
      }
        //get list of unread notifications
        /* GetNotifications(receiverId); */
    }

    private Notification PrepareNotification(Guid receiverId, NotificationsTypes type, string text, List<NotificationParam> @params)
    {
      var notification = new Notification()
      {
        Id = Guid.NewGuid(),
           Text = text,
           ReceiverId = receiverId,
           SenderId = _accountId,
           Read = false,
           CreatedOn = DateTime.UtcNow,
           Params = JsonConvert.SerializeObject(@params)
      };

      return notification;
    }

    public List<Notification> GetNotifications(Guid id)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        List<byte> messagesNotificationsTypes = new List<byte>()
        {
          (byte)NotificationsTypes.NewFlightRequestMessage,
            (byte)NotificationsTypes.NewMessage
        };

        var query = dbContext.Notifications
          .Where(not => not.ReceiverId == id
               && not.Read == false);

        _notificationHub.NewNetworkSignup(query.OrderByDescending(not => not.CreatedOn).ToList());

        return query.OrderByDescending(not => not.CreatedOn)
          .ToList();
      }
    }

    public void SetRead(string text)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var query = dbContext.Notifications
          .Where(notification => notification.Text == text)
          .ToArray();

        foreach (var notification in query)
        {
          notification.Read = true;
        }

        dbContext.SaveChanges();
        GetNotifications(adminId);
      }
    }

    public class NotificationParam
    {
      public string Key { get; set; }
      public string Value { get; set; }
    }
  }
}
