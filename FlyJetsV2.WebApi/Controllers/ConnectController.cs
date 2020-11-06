using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Logging;

using Stripe;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlyJetsV2.WebApi.Controllers
{
    public class ConnectController : Controller
    {
        private readonly ILogger<ConnectController> logger;

        public ConnectController(
          ILogger<ConnectController> logger
        )
        {
            this.logger = logger;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ProcessWebhookEvent()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Uncomment and replace with a real secret. You can find your endpoint's
            // secret in your webhook settings.
            // const string webhookSecret = "whsec_..."
            string webhookSecret = "whsec_...";

            // Verify webhook signature and extract the event.
            // See https://stripe.com/docs/webhooks/signatures for more information.
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    handleSuccessfulPaymentIntent(paymentIntent);
                }

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogInformation(e.ToString());
                return BadRequest();
            }
        }

        private void handleSuccessfulPaymentIntent(PaymentIntent paymentIntent)
        {
            logger.LogInformation($"PaymentIntent: {paymentIntent}");
        }
    }
}
