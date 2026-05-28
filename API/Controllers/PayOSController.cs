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
using PayOS.Models.Webhooks;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<IActionResult> Webhook([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("data", out JsonElement dataElement) ||
                    !body.TryGetProperty("signature", out JsonElement signatureElement))
                {
                    _logger.LogWarning("Invalid webhook payload structure");
                    return BadRequest("Invalid payload");
                }

                string signature = signatureElement.GetString() ?? "";

                bool isValid = PayOSSignatureHelper.Verify(dataElement, signature, _settings.ChecksumKey);

                if (!isValid)
                {
                    _logger.LogWarning("Invalid PayOS signature received");
                    return Unauthorized();
                }

                var webhookData = JsonSerializer.Deserialize<PayOSWebhookData>(
                    dataElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (webhookData == null)
                {
                    return BadRequest("Cannot parse payment data");
                }

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(x => x.TransactionCode == webhookData.orderCode.ToString());

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for OrderCode: {OrderCode}", webhookData.orderCode);
                    return Ok(); // Return 200 to acknowledge receipt
                }

                if (payment.Status == PaymentStatus.PAID)
                {
                    return Ok();
                }

                payment.Status = webhookData.code == "00" ? PaymentStatus.PAID : PaymentStatus.FAILED;
                payment.PaidAt = DateTime.UtcNow;   // Better to use UTC

                if (webhookData.code == "00")
                {
                    var application = await _context.Applications
                        .FindAsync(payment.ApplicationId);

                    if (application != null)
                    {
                        application.Status = ApplicationStatus.ACCEPTED;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment processed successfully. OrderCode: {OrderCode}, Status: {Status}",
                    webhookData.orderCode, payment.Status);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayOS webhook. Body: {Body}", body.GetRawText());
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("webhook")]
        public string Ping()
        {
            return "Oke";
        }
    }
}
