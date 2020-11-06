using FlyJetsV2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FlyJetsV2.Services
{
    public class PaymentService
    {
        private IConfiguration _config;
        private IHttpContextAccessor _httpContextAccessor;
        private Guid _accountId;

        public PaymentService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                _accountId = Guid.Parse(_httpContextAccessor.HttpContext.User.Identity.Name);
            }

            StripeConfiguration.ApiKey = _config["StripeSecretKey"];
        }

        public List<AccountPaymentMethod> GetPaymentMethods(byte usedFor)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return (from pm in dbContext.AccountPaymentMethods
                        where pm.AccountId == _accountId && pm.UsedFor == usedFor
                        select pm)
                        .ToList();
            }
        }

        public AccountPaymentMethod GetPaymentMethod(Guid paymentMethodId)
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return (from pm in dbContext.AccountPaymentMethods
                        where pm.AccountId == _accountId && pm.Id == paymentMethodId
                        select pm)
                       .FirstOrDefault();
            }
        }

        public AccountPaymentMethod CreateCreditCardPaymentMethod(string cardNumber, long cardExpYear, long cardExpMonth, string cardCvc, byte usedFor)
        {
            try
            {
                /* var options = new PaymentMethodCreateOptions */
                /* { */
                /*   Type = "card", */
                /*   Card = new PaymentMethodCardOptions */
                /*   { */
                /*     Number = cardNumber, */
                /*     ExpMonth = cardExpMonth, */
                /*     ExpYear = cardExpYear, */
                /*     Cvc = cardCvc */
                /*   }, */
                /* }; */
                /* var service = new PaymentMethodService(); */
                /* PaymentMethod pm = service.Create(options); */
                var tokenOptions = new TokenCreateOptions
                {
                    Card = new CreditCardOptions
                    {
                        Number = cardNumber,
                        ExpYear = cardExpYear,
                        ExpMonth = cardExpMonth,
                        Cvc = cardCvc
                    }
                };

                var tokenService = new TokenService();
                Token token = tokenService.Create(tokenOptions);

                var stripeCustomer = GetStripeCustomer();

                var cardOptions = new CardCreateOptions
                {
                    Source = token.Id
                };

                var service = new CardService();
                Card card = service.Create(stripeCustomer.Id, cardOptions);

                using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
                {
                    var paymentMehtod = new AccountPaymentMethod()
                    {
                        Id = Guid.NewGuid(),
                        AccountId = _accountId,
                        PaymentMethod = (byte)PaymentMethods.CreditCard,
                        TokenId = token.Id,
                        CreditCardBrand = token.Card.Brand,
                        CreditCardLast4 = token.Card.Last4,
                        CreditCardExpMonth = token.Card.ExpMonth,
                        CreditCardExYear = token.Card.ExpYear,
                        UsedFor = usedFor,
                        ReferencePaymentMethodId = card.Id,
                        CreatedOn = DateTime.UtcNow
                    };

                    dbContext.AccountPaymentMethods.Add(paymentMehtod);
                    dbContext.SaveChanges();

                    return paymentMehtod;
                }
            }
            catch (StripeException e)
            {
                switch (e.StripeError.ErrorType)
                {
                    case "card_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "api_connection_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "api_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "authentication_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "invalid_request_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "rate_limit_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    case "validation_error":
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                    default:
                      Console.WriteLine("Code: " + e.StripeError.Code);
                      Console.WriteLine("Message: " + e.StripeError.Message);
                        break;
                }

                return null;
            }
        }

        public AccountPaymentMethod CreateBankAccountPaymentMethod(string accountHolderName, string accountHolderType, string routingNumber, string accountNumber, byte usedFor)
        {
            try
            {
                var tokenOptions = new TokenCreateOptions
                {
                    BankAccount = new BankAccountOptions
                    {
                        Country = "US",
                        Currency = "usd",
                        AccountHolderName = accountHolderName,
                        AccountHolderType = accountHolderType,
                        RoutingNumber = routingNumber,
                        AccountNumber = accountNumber
                    }
                };

                var tokenService = new TokenService();
                Token token = tokenService.Create(tokenOptions);

                var stripeCustomer = GetStripeCustomer();

                var bankAccountOptions = new BankAccountCreateOptions
                {
                    Source = token.Id,
                };

                var bankAccountService = new BankAccountService();
                BankAccount bankAccount = bankAccountService.Create(stripeCustomer.Id, bankAccountOptions);

                using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
                {
                    var paymentMehtod = new AccountPaymentMethod()
                    {
                        Id = Guid.NewGuid(),
                        AccountId = _accountId,
                        PaymentMethod = (byte)PaymentMethods.BankTransfer,
                        TokenId = token.Id,
                        BankName = token.BankAccount.BankName,
                        RoutingNumber = token.BankAccount.RoutingNumber,
                        HolderName = token.BankAccount.AccountHolderName,
                        AccountLast4 = token.BankAccount.Last4,
                        UsedFor = usedFor,
                        ReferencePaymentMethodId = bankAccount.Id,
                        CreatedOn = DateTime.UtcNow
                    };

                    dbContext.AccountPaymentMethods.Add(paymentMehtod);
                    dbContext.SaveChanges();

                    return paymentMehtod;
                }
            }
            catch (StripeException e)
            {
                switch (e.StripeError.ErrorType)
                {
                    case "card_error":
                        break;
                    case "api_connection_error":
                        break;
                    case "api_error":
                        break;
                    case "authentication_error":
                        break;
                    case "invalid_request_error":
                        break;
                    case "rate_limit_error":
                        break;
                    case "validation_error":
                        break;
                    default:
                        break;
                }

                return null;
            }
        }

        public ServiceOperationResult<Charge> Charge(long amount, string description, string customerId, string sourceId, Dictionary<string, string> metadata, bool capture)
        {
            try
            {
                var options = new ChargeCreateOptions
                {
                    Amount = amount,
                    Currency = "usd",
                    Description = description,
                    Customer = customerId,
                    Metadata = metadata,
                    Source = sourceId,
                    Capture = capture
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                ServiceOperationResult<Charge> result = new ServiceOperationResult<Charge>();

                result.IsSuccessfull = true;
                result.Item = charge;

                return result;
            }
            catch (StripeException e)
            {
                ServiceOperationResult<Charge> result = new ServiceOperationResult<Charge>();

                result.IsSuccessfull = false;
                result.Errors = new List<ErrorCodes>();

                switch (e.StripeError.ErrorType)
                {
                    case "card_error":

                        switch (e.StripeError.Code)
                        {
                            case "amount_too_small":
                                result.Errors.Add(ErrorCodes.PaymentAmountTooSmall);
                                break;
                            case "amount_too_large":
                                result.Errors.Add(ErrorCodes.PaymentAmountTooLarge);
                                break;
                            case "balance_insufficient":
                                result.Errors.Add(ErrorCodes.PaymentBalanceInsufficient);
                                break;
                            case "expired_card":
                                result.Errors.Add(ErrorCodes.PaymentExpiredCard);
                                break;
                            default:
                                result.Errors.Add(ErrorCodes.PaymentUnknownError);
                                break;
                        }

                        break;
                    case "api_connection_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "api_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "authentication_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "invalid_request_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "rate_limit_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "validation_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    default:
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                }

                return result;
            }
        }

        public ServiceOperationResult<Charge> CaptureCharge(string chargeId)
        {
            try
            {
                var service = new ChargeService();
                Charge charge = service.Capture(chargeId, null);

                ServiceOperationResult<Charge> result = new ServiceOperationResult<Charge>();

                result.IsSuccessfull = true;
                result.Item = charge;

                return result;
            }
            catch (StripeException e)
            {
                ServiceOperationResult<Charge> result = new ServiceOperationResult<Charge>();

                result.IsSuccessfull = false;
                result.Errors = new List<ErrorCodes>();

                switch (e.StripeError.ErrorType)
                {
                    case "card_error":

                        switch (e.StripeError.Code)
                        {
                            case "amount_too_small":
                                result.Errors.Add(ErrorCodes.PaymentAmountTooSmall);
                                break;
                            case "amount_too_large":
                                result.Errors.Add(ErrorCodes.PaymentAmountTooLarge);
                                break;
                            case "balance_insufficient":
                                result.Errors.Add(ErrorCodes.PaymentBalanceInsufficient);
                                break;
                            case "expired_card":
                                result.Errors.Add(ErrorCodes.PaymentExpiredCard);
                                break;
                            default:
                                result.Errors.Add(ErrorCodes.PaymentUnknownError);
                                break;
                        }

                        break;
                    case "api_connection_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "api_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "authentication_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "invalid_request_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "rate_limit_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    case "validation_error":
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                    default:
                        result.Errors.Add(ErrorCodes.PaymentUnknownError);
                        break;
                }

                return result;
            }
        }

        private Customer GetStripeCustomer()
        {
            Data.Account currentAccount;

            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                currentAccount = (from account in dbContext.Accounts
                            where account.Id == _accountId
                            select account)
                           .FirstOrDefault();


                var customerService = new CustomerService();

                if (string.IsNullOrEmpty(currentAccount.StripeCustomerId))
                {
                    var customerOptions = new CustomerCreateOptions
                    {
                        Email = currentAccount.Email,
                        Metadata = new Dictionary<string, string>() { { "AccountId", currentAccount.Id.ToString() } }
                    };

                    Customer customer = customerService.Create(customerOptions);

                    currentAccount.StripeCustomerId = customer.Id;
                    dbContext.SaveChanges();

                    return customer;
                }
                else
                {
                    return customerService.Get(currentAccount.StripeCustomerId);
                }
            }
        }
    }
}
