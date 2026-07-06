using System;
using System.IO;
using System.Threading.Tasks;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ICoinService _coinService;
        private readonly IConfiguration _configuration;

        public WebhookController(ICoinService coinService, IConfiguration configuration)
        {
            _coinService = coinService;
            _configuration = configuration;
        }

        /// <summary>
        /// Handles Stripe Webhook events
        /// </summary>
        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];
            var webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        await _coinService.ConfirmPurchaseAsync(paymentIntent.Id);
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                // Invalid signature or Stripe error
                Console.WriteLine($"Stripe webhook signature verification failed: {ex.Message}");
                return BadRequest("Invalid Stripe Webhook Signature");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stripe webhook processing error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}
