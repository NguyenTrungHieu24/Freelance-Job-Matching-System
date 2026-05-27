using API.Configurations;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace API.Services.Payment
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSClient _payOS;
        private readonly PayOSSettings _settings;

        public PayOSService(PayOSClient payOS,IOptions<PayOSSettings> settings)
        {
            _payOS = payOS;
            _settings = settings.Value;
        }

        public async Task<CreatePaymentLinkResponse> CreatePaymentLink(long orderCode, int amount, string description)
        {
            var paymentData = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = description,

                ReturnUrl = _settings.ReturnUrl,
                CancelUrl = _settings.CancelUrl
            };

            var result = await _payOS
                .PaymentRequests
                .CreateAsync(paymentData);

            return result;
        }
    }
}
