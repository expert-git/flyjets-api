using FlyJetsV2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace FlyJetsV2.Services
{
  public class AccountService
  {
    private IConfiguration _config;
    private IHttpContextAccessor _httpContextAccessor;
    private StorageManager _storageManager;
    private Guid _accountId;
    private NotificationService _notificationService;
    private MailerService _mailerService;

    public AccountService(IConfiguration config, IHttpContextAccessor httpContextAccessor,
        StorageManager storageManager, NotificationService notificationService, MailerService mailerService)
    {
      _config = config;
      _httpContextAccessor = httpContextAccessor;
      _storageManager = storageManager;
      _notificationService = notificationService;
      _mailerService = mailerService;

      if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
      {
        _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
      }
    }

    public Account GetAccount(string email, string password)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts.FirstOrDefault(acc => acc.Email == email);

        if (account == null)
        {
          return null;
        }

        if (account.ManagedByFlyJets && account.PasswordExpired)
        {
          return null;
        }

        if (PasswordHash.ValidatePassword(password, account.Password))
        {
          account.LastLogInDate = DateTime.UtcNow;
          account.ResetPasswordCode = null;

          if (account.ManagedByFlyJets)
          {
            account.PasswordExpired = true;
            string newPassword = GenerateRandomPassword(16);

            account.Password = PasswordHash.CreateHash(newPassword);
          }

          dbContext.SaveChanges();

          return account;
        }
        else
        {
          return null;
        }
      }
    }

    public Account GetAccount(string email)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts.FirstOrDefault(acc => acc.Email == email);

        return account;
      }
    }

    private static string GenerateRandomPassword(short length)
    {
      Random random = new Random();
      string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$#!&^%@*";

      var newPassword = new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
      return newPassword;
    }

    public Account GetAccount(Guid? id = null)
    {
      if (id.HasValue == false)
      {
        id = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
      }

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts
          .FirstOrDefault(acc => acc.Id == id);

        return account;
      }
    }

    public List<AccountFamilyMember> GetFamilyMembers()
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var currentUserId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);

        var members = (from member in dbContext.AccountFamilyMembers
            join account in dbContext.Accounts on member.FlyJestMemberId equals account.Id into g
            from acc in g.DefaultIfEmpty()
            where member.AccountId == currentUserId
            select new AccountFamilyMember
            {
            Id = member.Id,
            FirstName = member.FlyJestMemberId.HasValue ? acc.FirstName : member.FirstName,
            LastName = member.FlyJestMemberId.HasValue ? acc.LastName : member.LastName,
            DateOfBirth = member.FlyJestMemberId.HasValue ? acc.DateOfBirth : member.DateOfBirth,
            Email = member.FlyJestMemberId.HasValue ? acc.Email : member.Email,
            Address = member.FlyJestMemberId.HasValue ? acc.Address : member.Address,
            Mobile = member.FlyJestMemberId.HasValue ? acc.Mobile : member.Mobile,
            FlyJestMemberId = member.FlyJestMemberId
            })
        .ToList();

        return members;
      }
    }

    public ServiceOperationResult VerifyAccount(string token)
    {
      using(FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts
          .FirstOrDefault(acc => acc.VerificationCode == token);

        //account is present, meaning user has signed up
        if (account != null)
        {
          account.Status = (byte)AccountStatuses.Active;
          try {
            dbContext.SaveChanges();
          } catch(DbUpdateException e) {
            throw e;
          }
          return new ServiceOperationResult() {
            IsSuccessfull = true
          };

        } else 
        {
          return new ServiceOperationResult() {
            IsSuccessfull = false,
            Errors = new List<ErrorCodes>() { 
              ErrorCodes.NotFound 
            }
          };
        }
      }
    }

    public ServiceOperationResult<Guid> CreateAccount(string email, string password, string firstName, string middleName,
        string lastName, AccountTypes accountType, string companyName, string mobile, bool managedByFlyJets)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        if (dbContext.Accounts.Any(acc => acc.Email == email))
        {
          return new ServiceOperationResult<Guid>()
          {
            IsSuccessfull = false,
            Errors = new List<ErrorCodes>() { ErrorCodes.EmailExists }
          };
        }

        string accountNumber = "";
        string referenceCode;

        if (accountType != AccountTypes.Admin)
        {
          switch (accountType)
          {
            case AccountTypes.Flyer:
              referenceCode = "FLYER";
              accountNumber = "FLR{0}{1}";
              break;
            case AccountTypes.AircraftProvider:
              referenceCode = "AIRCRAFTPROVIDER";
              accountNumber = "AP{0}{1}";
              break;
            default:
              throw new Exception("Unsupported account type");
          }

          SqlParameter[] @params =
          {
            new SqlParameter("ReturnVal", SqlDbType.Int) {Direction = ParameterDirection.Output},
            new SqlParameter("ReferenceCode", referenceCode),
            new SqlParameter("Year", DateTime.UtcNow.Year)
          };

          dbContext.Database.ExecuteSqlCommand("FJSP_GetNextNumber @ReferenceCode, @Year, @ReturnVal Output",
              @params);

          var number = @params[0].Value.ToString();

          accountNumber = string.Format(accountNumber, DateTime.UtcNow.Year, number);
        }

        var newAccount = new Account()
        {
          Id = Guid.NewGuid(),
             Number = accountNumber,
             Type = (byte)accountType,
             Email = email,
             Password = PasswordHash.CreateHash(password),
             FirstName = firstName,
             MiddleName = middleName,
             LastName = lastName,
             CreatedOn = DateTime.UtcNow,
             CompanyName = companyName,
             Status = accountType == AccountTypes.AircraftProvider ? (byte)AccountStatuses.PendingApproval : (byte)AccountStatuses.PendingConfirmation,
             LastLogInDate = DateTime.UtcNow,
             NotificationsChannel = (byte)NotificationsChannels.Email,
             Mobile = mobile,
             VerificationCode = Guid.NewGuid().ToString(),
             ManagedByFlyJets = managedByFlyJets,
             PasswordExpired = false
        };

        dbContext.Accounts.Add(newAccount);

        try
        {
          dbContext.SaveChanges();

          var flyJetsId = dbContext.Accounts
            .Where(account => account.Type == (byte)AccountTypes.Admin)
            .Select(account => account.Id)
            .First();

          if (newAccount.Type == (byte)AccountTypes.AircraftProvider)
          {
            _notificationService.NewCreate(flyJetsId,
                NotificationsTypes.NewAircraftProviderRegistrationRequest,
                "New Aircraft Provider",
                new List<NotificationService.NotificationParam>() {
                new NotificationService.NotificationParam() {
                Key = "AccountId",
                Value = newAccount.Id.ToString()
                },
                new NotificationService.NotificationParam() {
                Key = "AccountFirstName",
                Value = newAccount.FirstName 
                },
                new NotificationService.NotificationParam() {
                Key = "AccountLastName",
                Value = newAccount.LastName
                },
                new NotificationService.NotificationParam() {
                Key = "AccountCompany",
                Value = newAccount.CompanyName
                },
                new NotificationService.NotificationParam() {
                Key = "AccountMobile",
                Value = newAccount.Mobile
                },
                new NotificationService.NotificationParam() {
                Key = "AccountEmail",
                Value = newAccount.Email
                },
                new NotificationService.NotificationParam() {
                Key = "AccountCreation",
                Value = newAccount.CreatedOn.ToString()
                }
                });
          }
          else
          {
            _notificationService.NewCreate(flyJetsId,
                NotificationsTypes.NewFlyer,
                "New Flyer",
                new List<NotificationService.NotificationParam>() {
                new NotificationService.NotificationParam() {
                Key = "AccountId",
                Value = newAccount.Id.ToString()
                },
                new NotificationService.NotificationParam() {
                Key = "AccountFirstName",
                Value = newAccount.FirstName
                },
                new NotificationService.NotificationParam() {
                Key = "AccountLastName",
                Value = newAccount.LastName
                },
                new NotificationService.NotificationParam() {
                Key = "AccountMobile",
                Value = newAccount.Mobile
                },
                new NotificationService.NotificationParam() {
                Key = "AccountEmail",
                Value = newAccount.Email
                },
                new NotificationService.NotificationParam() {
                Key = "AccountCreation",
                Value = newAccount.CreatedOn.ToString()
                }
                });
        string receiverName = string.Join(newAccount.FirstName, " ", newAccount.LastName);
        string credEmail = _config.GetSection("CredentialEmail").Value;
        string credPass = _config.GetSection("CredentialPassword").Value; 
        string message = string.Format("Thank you for signing up with FLYJETS. Please visit <a href={0}/app/Verify/{1}>this page</a> to finish signing up", _config.GetSection("MailerUrl").Value, newAccount.VerificationCode);
                      
            _mailerService.Send(newAccount.Email, receiverName, message, "Thank you for signing up with FLYJETS", credEmail, credPass);
          }

          //AzureQueueService.Instance.AddMessage("mailqueue", "Welcome to FlyJets");

          //if (accountType == AccountTypes.AircraftProvider)
          //{
          //    CustomPrincipal userPrincipal = new CustomPrincipal(newAccount.Id);

          //    userPrincipal.FirstName = newAccount.FirstName;
          //    userPrincipal.LastName = newAccount.LastName;
          //    userPrincipal.Email = newAccount.Email;
          //    userPrincipal.Type = accountType;
          //    //userPrincipal.AvatarUrl = newAccount.AvatarUrl;

          //    Thread.CurrentPrincipal = userPrincipal;


          return new ServiceOperationResult<Guid>()
          {
            IsSuccessfull = true,
                          Item = newAccount.Id
          };
        }
        catch (DbUpdateException e)
        {
          //if (e.InnerException != null && e.InnerException.InnerException != null)
          //{
          //    if (e.InnerException.InnerException is SqlException sqlException)
          //    {
          //        if (sqlException.Number == (int)DbUpdateErrorCodes.UniqueKeyError)
          //        {
          //            return new OperationResult<AccountDto>()
          //            {
          //                IsSuccessfull = false,
          //                Errors = new List<ErrorCodes>() { ErrorCodes.EmailExists }
          //            };
          //        }
          //    }
          //}

          return new ServiceOperationResult<Guid>()
          {
            IsSuccessfull = false,
                          Errors = new List<ErrorCodes>() { ErrorCodes.UnKnown }
          };
        }
        catch (Exception e)
        {

          throw new Exception(e.Message);
          return new ServiceOperationResult<Guid>()
          {
            IsSuccessfull = false,
                          Errors = new List<ErrorCodes>() { ErrorCodes.UnKnown }
          };
        }
      }
    }

    public void ForgotPassword(string email)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts
          .FirstOrDefault(acc => acc.Email == email);

        string credEmail = _config.GetSection("CredentialEmail").Value;
        string credPass = _config.GetSection("CredentialPassword").Value; 
        if (account != null)
        {
          //set reset token for later use
          account.ResetPasswordCode = Guid.NewGuid().ToString();

          //get rid of old password temporarily
          account.Password = Guid.NewGuid().ToString();

          string receiverName = string.Join(account.FirstName, " ", account.LastName);
          string message = string.Format("Your password has been reset. please visit {0}/app/ForgotPassword/{1} to reset it again. This reset token will expire in 10 minutes", _config.GetSection("MailerUrl").Value, account.ResetPasswordCode);

          _mailerService.Send(email, receiverName, message, "Reset your FLYJETS password", credEmail, credPass);

          //this is a biiiiiig O O F but it works.
          Timer passwordResetTimeout = new Timer((object state) => {
                using (FlyJetsDbContext _dbContext = new FlyJetsDbContext(_config))
                {
                  var __account = _dbContext.Accounts
                    .FirstOrDefault(acc => acc.Email == email);
                  __account.ResetPasswordCode = null;
                  _dbContext.SaveChanges();
                }
              }, null, 600000, Timeout.Infinite);
          dbContext.SaveChanges();
        } else {
          _mailerService.Send(email, "To Whom it May Concern", "We did not find a FLYJETS account associated with this email address.", "Reset your FLYJETS password", credEmail, credPass);
        }
      }
    }

    public ServiceOperationResult ResetPassword(string email, string password, string token)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts
            .FirstOrDefault(acc => acc.Email == email);
        if (account.ResetPasswordCode != null && account.ResetPasswordCode.ToString() == token)
        {
          account.Password = PasswordHash.CreateHash(password);
          dbContext.SaveChanges();
          return new ServiceOperationResult() {
            IsSuccessfull = true
          };
        } else {
          return new ServiceOperationResult() {
            IsSuccessfull = false,
            Errors = new List<ErrorCodes>() {
              ErrorCodes.UnKnown}
          };
        }
      }
    }

    public ServiceOperationResult UpdateAccount(string firstName, string middleName, string lastName, DateTime? dateOfBirth,
        string email, string mobile, string address, string companyName, string companyAddress, string companyEmail, string companyPhone, byte[] profileImage, string profileImageExtention)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
        var account = dbContext.Accounts.FirstOrDefault(acc => acc.Id == accountId);

        if (account == null)
        {
          return new ServiceOperationResult() { IsSuccessfull = false, Errors = new List<ErrorCodes>() { ErrorCodes.NotFound } };
        }

        var errors = new List<ErrorCodes>();

        if (string.IsNullOrEmpty(firstName))
        {
          errors.Add(ErrorCodes.FirstNameIsRequired);
        }

        if (string.IsNullOrEmpty(lastName))
        {
          errors.Add(ErrorCodes.LastNameIsRequired);
        }

        if ((!string.IsNullOrEmpty(firstName) && firstName.Length > 30) ||
            (!string.IsNullOrEmpty(middleName) && middleName.Length > 30) ||
            (!string.IsNullOrEmpty(lastName) && lastName.Length > 30))
        {
          errors.Add(ErrorCodes.StringLengthExceededMaximumAllowedLength);
        }

        string profileImageName = account.ImageFileName;

        if (profileImage != null && profileImage.Length != 0)
        {
          profileImageName = _storageManager.UploadImage(_config["ProfileImagesContainer"], profileImage, profileImageExtention, true);
        }

        account.FirstName = firstName;
        account.MiddleName = middleName;
        account.LastName = lastName;
        account.DateOfBirth = dateOfBirth;
        account.Email = email;
        account.Mobile = mobile;
        account.Address = address;
        account.ImageFileName = profileImageName;

        account.CompanyName = companyName;
        account.CompanyAddress = companyAddress;
        account.CompanyEmail = companyEmail;
        account.CompanyPhone = companyPhone;

        dbContext.SaveChanges();


        return new ServiceOperationResult()
        {
          IsSuccessfull = true
        };

      }
    }

    public ServiceOperationResult AddMember(string firstName, string lastName, string email, string mobile,
        string address)
    {
      ServiceOperationResult result = new ServiceOperationResult();
      result.IsSuccessfull = true;

      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);

        var member = new AccountFamilyMember()
        {
          Id = Guid.NewGuid(),
             FirstName = firstName,
             LastName = lastName,
             Email = email,
             Mobile = mobile,
             Address = address,
             AccountId = accountId,
             CreatedOn = DateTime.UtcNow
        };

        dbContext.AccountFamilyMembers.Add(member);
        dbContext.SaveChanges();

        return result;
      }
    }

    public ServiceOperationResult DeleteMember(Guid memberId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        ServiceOperationResult result = new ServiceOperationResult();
        result.IsSuccessfull = true;

        var accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);

        var member = new AccountFamilyMember()
        {
          Id = memberId,
             AccountId = accountId
        };

        dbContext.Entry(member).State = EntityState.Deleted;
        dbContext.SaveChanges();

        return result;
      }
    }

    public ServiceOperationResult ChangePassword(string oldPassword, string newPassword)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts.FirstOrDefault(acc => acc.Id == _accountId);

        if (PasswordHash.ValidatePassword(oldPassword, account.Password))
        {
          account.Password = PasswordHash.CreateHash(newPassword);
          dbContext.SaveChanges();

          return new ServiceOperationResult() { IsSuccessfull = true };
        }
        else
        {
          return new ServiceOperationResult() { IsSuccessfull = false, Errors = new List<ErrorCodes>() { ErrorCodes.InvalidPassword } };
        }
      }
    }

    public void UpdateNotificationChannel(byte? channel)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = new Account()
        {
          Id = _accountId,
             NotificationsChannel = channel
        };

        dbContext.Accounts.Attach(account);
        dbContext.Entry(account).Property(x => x.NotificationsChannel).IsModified = true;

        //dbContext.Configuration.ValidateOnSaveEnabled = false;
        dbContext.SaveChanges();
      }
    }

    public byte? GetNotificationChannels()
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        return (from account in dbContext.Accounts
            where account.Id == _accountId
            select account.NotificationsChannel)
          .FirstOrDefault();
      }
    }

    public List<Account> GetFlyers()
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        return dbContext.Accounts
          .Where(acc => acc.Type == (byte)AccountTypes.Flyer)
          .OrderByDescending(acc => acc.CreatedOn)
          .ToList();
      }
    }

    public List<Account> GetAircraftProviders(bool filterByPending)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        return dbContext.Accounts
          .Where(acc => acc.Type == (byte)AccountTypes.AircraftProvider
              && (filterByPending == false || acc.Status == (byte)AccountStatuses.PendingApproval))
          .OrderByDescending(acc => acc.CreatedOn)
          .ToList();
      }
    }

    public string CreateAircraftProviderOneTimePassword(Guid accountId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var account = dbContext.Accounts
          .FirstOrDefault(acc => acc.Id == accountId
              && acc.Type == (byte)AccountTypes.AircraftProvider
              && acc.ManagedByFlyJets == true);

        if (account == null)
        {
          return string.Empty;
        }

        var password = GenerateRandomPassword(12);

        account.Password = PasswordHash.CreateHash(password);
        account.PasswordExpired = false;

        dbContext.SaveChanges();

        return password;
      }
    }

    public List<Account> Search(byte type, string keyword)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        return dbContext.Accounts
          .Where(acc => acc.Type == type
              && (acc.FirstName.StartsWith(keyword)
                || acc.LastName.StartsWith(keyword)))
          .ToList();
      }
    }

    public ServiceOperationResult AcceptAircraftProviderSignupRequest(Guid accountId)
    {
      using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
      {
        var result = new ServiceOperationResult();

        var account = dbContext.Accounts
          .First(acc => acc.Id == accountId);

        account.Status = (byte)AccountStatuses.Active;
        dbContext.SaveChanges();

        result.IsSuccessfull = true;
        
        string receiverName = string.Join(account.FirstName, " ", account.LastName);
        string credEmail = _config.GetSection("CredentialEmail").Value;
        string credPass = _config.GetSection("CredentialPassword").Value; 
        string message = string.Format("Thank you for your patience. Your request to join FLYJETS has been approved. Please visit {0} to log in and begin booking.", _config.GetSection("MailerUrl").Value);
                      
            _mailerService.Send(account.Email, receiverName, message, "Welcome to FLYJETS!", credEmail, credPass);

        return result;
      }
    }
  }
}
