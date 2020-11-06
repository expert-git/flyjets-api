using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FlyJetsV2.Data;
using FlyJetsV2.Services;
using FlyJetsV2.WebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FlyJetsV2.WebApi.Controllers
{
  [Route("api/accounts")]
  [ApiController]
  public class AccountsController : ControllerBase
  {
    private AccountService _accountService;
    private PaymentService _paymentService;
    private NotificationService _notificationService;
    private IConfiguration _config;
    private MailerService _mailerService;

    public AccountsController(AccountService accountService, PaymentService paymentService, NotificationService notificationService, IConfiguration config, MailerService mailerService)
    {
      _accountService = accountService;
      _paymentService = paymentService;
      _notificationService = notificationService;
      _config = config;
      _mailerService = mailerService;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    public IActionResult Login(LoginModel model)
    {
      Account account = _accountService.GetAccount(model.Email, model.Password);
      bool isUserValid = account != null;

      if (isUserValid && account.Status == (byte)AccountStatuses.Active)
      {
        var claims = new List<Claim>();

        claims.Add(new Claim(ClaimTypes.Name, account.Id.ToString()));

        switch ((AccountTypes)account.Type)
        {
          case AccountTypes.Admin:
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            break;
          case AccountTypes.AircraftProvider:
            claims.Add(new Claim(ClaimTypes.Role, "AircraftProvider"));
            break;
          case AccountTypes.Flyer:
            claims.Add(new Claim(ClaimTypes.Role, "Flyer"));
            break;
          default:
            throw new Exception("Unsupported Account Type");
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties()
        {
          IsPersistent = true//model.RememberMe
        };

        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props)
          .Wait();

        return Ok(new
            {
            firstName = account.FirstName,
            lastName = account.LastName
            });
      }
        //mailer service to resend email
      else if (isUserValid && account.Status == (byte)AccountStatuses.PendingConfirmation) {
        string receiverName = string.Join(account.FirstName, " ", account.LastName);
        string credEmail = _config.GetSection("CredentialEmail").Value;
        string credPass = _config.GetSection("CredentialPassword").Value; 
        string message = string.Format("Thank you for signing up with FLYJETS. Please visit {0}/app/Verify/{1} to finish signing up", _config.GetSection("MailerUrl").Value, account.VerificationCode);

        _mailerService.Send(account.Email, receiverName, message, "Thank you for signing up with FLYJETS", credEmail, credPass);
      return Unauthorized();
      }
      else
      {
        return NotFound();
      }
    }

    [Authorize]
    [HttpGet]
    [Route("logout")]
    public IActionResult Logout()
    {
      HttpContext.SignOutAsync();

      return Ok();
    }

    [HttpGet]
    [Route("checklogin")]
    public IActionResult IsUserLoggedIn()
    {
      if (HttpContext.User.Identity.IsAuthenticated)
      {
        Account account = _accountService.GetAccount(Guid.Parse(HttpContext.User.Identity.Name));

        var notifications = new List<Notification>();
        if (account.Type != (byte)AccountTypes.Flyer) {
          notifications = _notificationService.GetNotifications(account.Id);
        }
        return Ok(new
            {
            firstName = account.FirstName,
            lastName = account.LastName,
            imageUrl = account.ThumbnailImageFileName,
            type = account.Type,
            account = account,
            notifications = notifications
            });
      }
      else
      {
        return Unauthorized();
      }
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("create")]
    public IActionResult Create(CreateAccountModel model)
    {
        var newAccount = _accountService.CreateAccount(model.Email, model.Password,
          model.FirstName, model.MiddleName, model.LastName, (AccountTypes)model.AccountType,
          model.CompanyName, model.Mobile, false);

      if (newAccount.IsSuccessfull)
      {
        if(model.AccountType == (byte)AccountTypes.Flyer)
        {
          var claims = new List<Claim>();

          claims.Add(new Claim(ClaimTypes.Name, newAccount.Item.ToString()));

          claims.Add(new Claim(ClaimTypes.Role, "Flyer"));

          var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

          var principal = new ClaimsPrincipal(identity);

          var props = new AuthenticationProperties()
          {
            IsPersistent = true//model.RememberMe
          };

          HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props)
            .Wait();
        }

        return Ok();
      }
      else if (newAccount.Errors[0] == ErrorCodes.EmailExists)
      {
        return StatusCode(StatusCodes.Status302Found);
      }
      else
      {
        return BadRequest();
      }
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public IActionResult VerifyAccount(VerifyAccountModel model)
    {
      var accountToVerify = _accountService.VerifyAccount(model.Token);
      
      if (accountToVerify.IsSuccessfull == true) {
        return Ok();
      } else {
        return NotFound();
      }
    }


    [AllowAnonymous]
    [HttpPost]
    [Route("forgotpass")]
    public IActionResult ForgotPassword(LoginModel model)
    {
      _accountService.ForgotPassword(model.Email);

      return Ok();
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("resetpass")]
    public IActionResult ResetPassword(ResetPasswordModel model)
    {
      var result = _accountService.ResetPassword(model.Email, model.Password, model.Token);
      if (result.IsSuccessfull) {
        return Ok();
      } else {
        return Unauthorized();
      }
    }

    [Authorize]
    [HttpGet]
    [Route("get")]
    public JsonResult Get()
    {
      var account = _accountService.GetAccount();
      var familyMembers = _accountService.GetFamilyMembers();

      return new JsonResult(new
          {
          account.FirstName,
          account.MiddleName,
          account.LastName,
          account.DateOfBirth,
          account.Email,
          account.Mobile,
          account.Address,
          account.CompanyName,
          account.CompanyEmail,
          account.CompanyPhone,
          account.CompanyAddress,
          familyMembers = familyMembers
          });
    }

    [Authorize]
    [HttpPost]
    [Route("update")]
    public IActionResult Update(UpdateProfileModel model)
    {

      byte[] fileBytes = null;
      string fileExt = null;

      if (model.Image != null && model.Image.Length != 0)
      {
        using (var ms = new MemoryStream())
        {
          model.Image.CopyTo(ms);
          fileBytes = ms.ToArray();
        }

        fileExt = model.Image.FileName.Substring(model.Image.FileName.LastIndexOf('.'));
      }

      var updatedAccount = _accountService.UpdateAccount(
          model.FirstName,
          model.MiddleName,
          model.LastName,
          model.DateOfBirth,
          model.Email,
          model.Mobile,
          model.Address,
          model.CompanyName,
          model.CompanyAddress,
          model.CompanyEmail,
          model.CompanyPhone,
          fileBytes,
          fileExt
          );

      return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("addMember")]
    public IActionResult AddFamilyMember(AddFamilyMemberModel model)
    {
      List<ErrorCodes> errors = new List<ErrorCodes>();

      if (string.IsNullOrEmpty(model.FirstName))
      {
        errors.Add(ErrorCodes.FirstNameIsRequired);
      }

      if (string.IsNullOrEmpty(model.LastName))
      {
        errors.Add(ErrorCodes.LastNameIsRequired);
      }

      var result = _accountService.AddMember(model.FirstName, model.LastName, model.Email,
          model.Mobile, model.Address);

      if (result.IsSuccessfull)
      {
        return Ok();
      }
      else
      {
        return StatusCode(StatusCodes.Status422UnprocessableEntity, errors);
      }
    }

    [Authorize]
    [HttpGet]
    [Route("deleteMember/{memberId}")]
    public IActionResult DeleteFamilyMember(Guid memberId)
    {
      _accountService.DeleteMember(memberId);

      return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("changepass", Name = "ChangePassword")]
    public IActionResult ChangePassword(ChangePasswordModel model)
    {
      List<string> errors = new List<string>();

      var result = _accountService.ChangePassword(model.OldPassword, model.NewPassword);

      if (!result.IsSuccessfull)
      {
        foreach (var error in result.Errors)
        {
          errors.Add(error.ToString());
        }

        return StatusCode(StatusCodes.Status403Forbidden, errors);
      }

      return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("updatenotchannel", Name = "UpdateNotificationsChannel")]
    public IActionResult UpdateNotificationsChannel(UpdateNotificationsChannelModel model)
    {
      _accountService.UpdateNotificationChannel(model.Channel);

      return Ok();
    }

    [Authorize]
    [HttpGet]
    [Route("getnotchannel", Name = "GetNotificationsChannel")]
    public JsonResult GetNotificationsChannel()
    {
      var channels = _accountService.GetNotificationChannels();

      return new JsonResult(channels);
    }

    [Authorize]
    [HttpGet]
    [Route("list/{type}", Name = "GetAccounts")]
    public JsonResult GetAccounts(byte type)
    {
      List<Account> accounts;

      if (type == (byte)AccountTypes.Flyer)
      {
        accounts = _accountService.GetFlyers();
        _notificationService.SetRead("New Flyer");
      }
      else if (type == (byte)AccountTypes.AircraftProvider)
      {
        accounts = _accountService.GetAircraftProviders(false);
      }
      else
      {
        accounts = new List<Account>();
      }

      return new JsonResult(accounts.Select(account => new {
            account.Id,
            account.Number,
            account.FirstName,
            account.LastName,
            account.Email,
            account.Mobile,
            account.CreatedOn
            })
          .ToList());
    }

    [Authorize]
    [HttpGet]
    [Route("paymethods/{usedFor}", Name = "PaymentMethods")]
    public JsonResult PaymentMethods(byte usedFor)
    {
      var paymentMethods = _paymentService.GetPaymentMethods(usedFor);

      return new JsonResult(new
          {
          CreditCards = paymentMethods
          .Where(pm => string.IsNullOrEmpty(pm.CreditCardLast4) == false)
          .Select(pm => new
              {
              pm.Id,
              pm.CreditCardBrand,
              pm.CreditCardLast4,
              pm.CreditCardExpMonth,
              pm.CreditCardExYear
              }).ToList(),
          BankAccounts = paymentMethods
          .Where(pm => string.IsNullOrEmpty(pm.BankName) == false)
          .Select(pm => new
              {
              pm.Id,
              pm.BankName,
              pm.HolderName,
              pm.RoutingNumber,
              pm.AccountLast4,
              })
          .ToList()
          });
    }

    [Authorize]
    [HttpPost]
    [Route("paymethods/creatcc", Name = "CreateCreditCardPaymentMehtod")]
    public JsonResult CreateCreditCardPaymentMehtod(CreditCardPaymentMethodModel model)
    {
      var card = _paymentService.CreateCreditCardPaymentMethod(model.Number, model.ExpiryYear, model.ExpiryMonth, model.Cvc, model.UsedFor);

      return new JsonResult(new
          {
          card.Id,
          card.CreditCardBrand,
          card.CreditCardLast4,
          card.CreditCardExpMonth,
          card.CreditCardExYear
          });
    }

    [Authorize]
    [HttpPost]
    [Route("paymethods/creatbankacc", Name = "CreateBankAccountPaymentMehtod")]
    public JsonResult CreateBankAccountPaymentMehtod(BankAccountPaymentMethodModel model)
    {
      var bankAccount = _paymentService.CreateBankAccountPaymentMethod(model.AccountHolderName, model.AccountHolderType, model.RoutingNumber, model.AccountNumber, model.UsedFor);

      return new JsonResult(new
          {
          bankAccount.Id,
          bankAccount.BankName,
          bankAccount.HolderName,
          bankAccount.RoutingNumber,
          bankAccount.AccountLast4
          });
    }

    [Authorize]
    [HttpGet]
    [Route("onetimepass/{accountId}", Name = "GetOneTimePassword")]
    public JsonResult GetOneTimePassword(Guid accountId)
    {
      var password = _accountService.CreateAircraftProviderOneTimePassword(accountId);

      return new JsonResult(password);
    }

    [Authorize]
    [HttpGet]
    [Route("autocomplete/{type}/{query}")]
    public JsonResult Search(string query, byte type)
    {
      var accounts = _accountService.Search(type, query);

      return new JsonResult(accounts.Select(acc => new
            {
            id = acc.Id,
            name = acc.FirstName + " " + acc.LastName + (string.IsNullOrEmpty(acc.CompanyName) ? "" : "(" + acc.CompanyName + ")"),
            }));
    }

    [Authorize]
    [HttpGet]
    [Route("chatjwt")]
    public IActionResult GetJwtForChat()
    {
      Account account = _accountService.GetAccount(Guid.Parse(HttpContext.User.Identity.Name));

      var iat = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
      var exp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(5)).ToUnixTimeSeconds();

      var payload = "{"+
        "\"iat\":" + iat.ToString() + ", " +
        "\"exp\":" + exp.ToString() + ", " +
        "\"external_id\":\"" + account.Id.ToString() + "\", " +
        "\"name\":\"" + account.FirstName + "\", " +
        "\"email\":\"" + account.Email + "\"" +
        "}";

      var jsonWebTokenHandler = new JsonWebTokenHandler();

      //var jsonWebToken = new JsonWebToken("{\"typ\":\"JWT\", \"alg\":\"HS256\"}", payload);

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["ZendeskSharedKey"]));

      var signingCredentials = new SigningCredentials(key, "HS256");

      var token = jsonWebTokenHandler.CreateToken(payload, signingCredentials); 

      return Ok(token);
    }

    [Authorize]
    [HttpGet]
    [Route("apsPendingApproval", Name = "GetAircraftProvidersPendingApproval")]
    public JsonResult GetAircraftProvidersPendingApproval()
    {
      List<Account> accounts;

      accounts = _accountService.GetAircraftProviders(true);

      _notificationService.SetRead("New Aircraft Provider");

      return new JsonResult(accounts.Select(account => new {
            account.Id,
            account.Number,
            account.FirstName,
            account.LastName,
            account.CompanyName,
            account.Email,
            account.Mobile,
            account.CreatedOn
            })
          .ToList());
    }

    [Authorize]
    [HttpPost]
    [Route("acceptSignupRequest", Name = "AcceptAircraftProviderSignupRequestModel")]
    public IActionResult AcceptAircraftProviderSignupRequest(AcceptAircraftProviderSignupRequestModel model)
    {
      _accountService.AcceptAircraftProviderSignupRequest(model.AccountId);

      return Ok();
    }
  }
}
