using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using MailKit.Security;

namespace FlyJetsV2.Services {
  public class MailerService 
  {
    private readonly ILogger<MailerService> _logger;
    private readonly IConfiguration _config;

    public MailerService(ILogger<MailerService> logger, IConfiguration config)
    {
      _logger = logger;
      _config = config;
    }

    public void Send(string receiverEmail, string receiverName, string message, string subject, string senderEmail, string senderPass) {
      var _message = new MimeMessage();
      _message.To.Add(new MailboxAddress(receiverName, receiverEmail));
      _message.From.Add(new MailboxAddress("The FLYJETS Team", "information@flyjets.com"));
      _message.Subject = subject;
      _message.Body = new TextPart(TextFormat.Html)
      {
        Text = message
      };

      using (SmtpClient client = new SmtpClient())
      {
        client.ServerCertificateValidationCallback = (s,c,h,e) => true;

        client.Connect("smtp.gmail.com", 587, false);

        client.Authenticate(senderEmail, senderPass);

        client.Send(_message);
        client.Disconnect(true);
      }
    }
  }
}
