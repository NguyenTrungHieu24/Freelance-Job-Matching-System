using API.Services.Auth;
using API.Services.Payment;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : BaseController
    {
        private readonly IPayOSService _payOSService;

        public PaymentController(AppDbContext context, IMapper mapper, IUserService user, IPayOSService payOSService) : base(context, mapper, user)
        {
            _payOSService = payOSService;
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        {
            long orderCode = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var result = await _payOSService
                .CreatePaymentLink(
                    request.OrderCode,
                    request.Amount,
                    request.Description
                );

            return Ok(result);
        }
    }
}
