using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : BaseController
    {
        public WalletController(AppDbContext context, IMapper mapper, IUserService user)
            : base(context, mapper, user) { }

        /// <summary>
        /// Lấy số dư ví hiện tại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == _user.UserId);

            return Ok(new WalletBalanceDto
            {
                Balance = wallet?.Balance ?? 0
            });
        }
    }
}
