using BusinessObjects.Common;
using PayOS.Models.V2.PaymentRequests;

namespace API.Services.Payment
{
    public interface IPayOSService
    {
        Task<CreatePaymentLinkResponse> CreatePaymentLink(
            long orderCode,
            int amount,
            string description);
    }
}
