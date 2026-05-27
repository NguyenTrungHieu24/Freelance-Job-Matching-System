using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/payos")]
    [ApiController]
    public class PayOSController : BaseController
    {
        public PayOSController(AppDbContext context, IMapper mapper, IUserService user) : base(context, mapper, user)
        {
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PayOSWebhook webhook)
        {
            // 1. log raw data
            Console.WriteLine($"Webhook received: {webhook.OrderCode}");

            // 2. find payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(x => x.TransactionCode == webhook.OrderCode.ToString());

            if (payment == null)
                return Ok(); // tránh retry spam

            // 3. update status
            payment.Status = webhook.Status == "SUCCESS" ? PaymentStatus.PAID : PaymentStatus.FAILED; // SUCCESS / FAILED
            payment.PaidAt = DateTime.Now;

            // 4. update application
            var application = await _context.Applications
                .FindAsync(payment.ApplicationId);

            if (application != null && webhook.Status == "PAID")
            {
                application.Status = ApplicationStatus.ACCEPTED;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
