using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Stripe;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlyJetsV2.WebApi.Controllers
{
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly StripeClient client;
        private readonly IOptions<StripeOptions> options;
        private readonly ILogger<CustomersController> logger;
        private readonly StripeCustomerService customerService;
        private IConfiguration _config;
        // GET: /<controller>/


        public CustomersController(IConfiguration config)
        {
            // test commit
            customerService = new StripeCustomerService();
            this.client = new StripeClient("sk_test_4eC39HqLyjWDarjtT1zdp7dc");
            _config = config;

        }

        // Get Oauth Link - Connect Account
        [Route("get-oauth-link")]
        [HttpGet]
        public JsonResult get_oauth_link()
        {
            string state = RandomString(16, true);

            string client_id = _config.GetSection("Stripe_Client_ID").Value;

            string url = "https://connect.stripe.com/express/oauth/authorize?state="
                + state
                + "&client_id="
                + client_id;
            return new JsonResult(new
            {
                url = url
            });
        }

        // Get Connected Account Info and save data
        [HttpGet("/connect/oauth")]
        public IActionResult HandleOAuthRedirect([FromQuery] string state, [FromQuery] string code)
        {
            var service = new OAuthTokenService(client);

            // Assert the state matches the state you provided in the OAuth link (optional).
            //if (!StateMatches(state))
            //{
            //     return StatusCode(
            //        StatusCodes.Status403Forbidden,
            //       Json(new { Error = String.Format("Incorrect state parameter: {0}", state) })
            //    );
            //}

            // Send the authorization code to Stripe's API.
            var options = new OAuthTokenCreateOptions
            {
                GrantType = "authorization_code",
                Code = code,
            };

            OAuthToken response = null;

            try
            {
                response = service.Create(options);
            }
            catch (StripeException e)
            {
                if (e.StripeError != null && e.StripeError.Error == "invalid_grant")
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        Json(new { Error = String.Format("Invalid authorization code: {0}", code) })
                    );
                }
                else
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        Json(new { Error = "An unknown error occurred." })
                    );
                }
            }

            var connectedAccountId = response.StripeUserId;
            SaveAccountId(connectedAccountId);

            // Render some HTML or redirect to a different page.
            return new OkObjectResult(Json(new { Success = true }));
        }

        [Route("create-checkout-session")]
        [HttpGet]
        // Create Checkout Session
        public JsonResult CreateCheckoutSession()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Name = "Kavholm rental",
                        Amount = 1000,
                        Currency = "usd",
                        Quantity = 1,
                    },
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    ApplicationFeeAmount = 123,
                    TransferData = new SessionPaymentIntentTransferDataOptions
                    {
                        Destination = "{{CONNECTED_STRIPE_ACCOUNT_ID}}",
                    },
                },
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/failure",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return new JsonResult(new
            {
                sessionId = session.Id
            });
        }

        [Route("express-dashboard-link")]
        [HttpGet]
        public JsonResult express_dashboard_link()
        {
            var account_id = HttpContext.Request.Query["account_id"][0];

            //StripeConfiguration.ApiKey = _config.GetSection("Stripe_Secret_key").Value;
            StripeConfiguration.ApiKey = "sk_test_OqgRgzijpOeoqyZzh7TWFYuH00ic6FnP88";

            var service = new LoginLinkService();

            var options = new LoginLinkCreateOptions { 
                RedirectUrl = "http://localhost"
            };

            //var link = service.Create(
              //  "acct_1GDAT2H4zF7BB8C0", options
            //);

            return new JsonResult(new
            {
                //url = link.Url
                url = "https://connect.stripe.com/express/wOmIYmIcAcmq"
            });
        }
        private object Json(object p)
        {
            throw new NotImplementedException();
        }

        private bool StateMatches(string stateParameter)
        {
            // Load the same state value that you randomly generated for your OAuth link.
            var savedState = "{{ STATE }}";

            return savedState == stateParameter;
        }

        private void SaveAccountId(string id)
        {
            // Save the connected account ID from the response to your database.
            logger.LogInformation($"Connected account ID: {id}");
        }

        [HttpGet("/connect/check_connected_balance")]
        public Balance Check_connected_accounts_balance()
        {
            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = "";
            var service = new BalanceService();
            Balance balance = service.Get(requestOptions);

            return balance;
        }

        [Route("test/testing")]
        [HttpGet]
        public void Create_Instant_Payout()
        {
            var options = new PayoutCreateOptions
            {
                Amount = 1000,
                Currency = "usd",
                Method = "instant",
            };

            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = "{{CONNECTED_STRIPE_ACCOUNT_ID}}";

            var service = new PayoutService();
            var payout = service.Create(options, requestOptions);
        }

        [Route("create-payment-intent")]
        [HttpPost]
        public void create_payment_intent()
        {
            // Set your secret key. Remember to switch to your live secret key in production!
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.ApiKey = "sk_test_OqgRgzijpOeoqyZzh7TWFYuH00ic6FnP88";

            var service = new PaymentIntentService();
            var createOptions = new PaymentIntentCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Amount = 2000,
                Currency = "usd",
                ApplicationFeeAmount = 123,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = "{{CONNECTED_STRIPE_ACCOUNT_ID}}",
                },
            };
            service.Create(createOptions);
        }

        [Route("config")]
        [HttpGet]
        public JsonResult get_config()
        {            
            string pub_key = _config.GetSection("Stripe_Publish_Key").Value;
            string sec_key = _config.GetSection("Stripe_Secret_key").Value;
            string base_price = "10";
            string currency = "$";

            StripeConfiguration.ApiKey = sec_key;

            //var accounts = Stripe.g
            var options = new AccountListOptions { Limit = 10 };
            var service = new AccountService();
            StripeList<Account> accounts = service.List(options);

            return new JsonResult(new
            {
                publicKey = pub_key,
                basePrice = base_price,
                currency = currency,
                accounts = accounts
            });
        }

        [Route("webhook")]
        [HttpPost]
        public async Task<IActionResult> ProcessWebhookEvent()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Uncomment and replace with a real secret. You can find your endpoint's
            // secret in your webhook settings.
            const string webhookSecret = "whsec_...";

            // Verify webhook signature and extract the event.
            // See https://stripe.com/docs/webhooks/signatures for more information.
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    HandleCheckoutSession(session);
                }

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogInformation(e.ToString());
                return BadRequest();
            }
        }

        private void HandleCheckoutSession(Session session)
        {
            logger.LogInformation($"Session: {session}");
        }


        private string RandomString(int size,
                                    bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }    
}
