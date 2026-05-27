using API.Configurations;
using API.Helper;
using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Configuration;

namespace API.Controllers
{
    [Route("api/payos")]
    [ApiController]
    public class PayOSController : BaseController
    {

        private readonly PayOSSettings _settings;
        private readonly ILogger<PayOSController> _logger;

        public PayOSController(AppDbContext context, IMapper mapper, IUserService user, IOptions<PayOSSettings> settings, ILogger<PayOSController> logger) : base(context, mapper, user)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            var signature = Request.Headers["x-payos-signature"].ToString();

            // 3. verify
            var isValid = PayOSSignatureHelper.Verify(
                rawBody,
                signature,
                _settings.ChecksumKey
            );

            if (!isValid)
            {
                _logger.LogWarning("Invalid PayOS webhook signature");
                return Unauthorized();
            }

            var webhook = System.Text.Json.JsonSerializer.Deserialize<PayOSWebhook>(rawBody);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.TransactionCode == webhook.OrderCode.ToString());

            if (payment == null)
                return Ok(); // tránh retry spam

            payment.Status = webhook.Status == "SUCCESS" ? PaymentStatus.PAID : PaymentStatus.FAILED; // SUCCESS / FAILED
            payment.PaidAt = DateTime.Now;

            var application = await _context.Applications
                .FindAsync(payment.ApplicationId);

            if (application != null && webhook.Status == "PAID")
            {
                application.Status = ApplicationStatus.ACCEPTED;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment success: {webhook?.OrderCode}");

            return Ok(new { success = true });
        }
    }
}
