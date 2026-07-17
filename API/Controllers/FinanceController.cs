using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/finance")]
    [Authorize(Policy = "FinanceOnly")]
    public class FinanceController : BaseController
    {
        public FinanceController(AppDbContext context, IMapper mapper, IUserService user)
            : base(context, mapper, user) { }

        /// <summary>
        /// Danh sách tất cả ví Employer + Freelancer (có search keyword)
        /// </summary>
        [HttpGet("wallets")]
        public async Task<IActionResult> GetWallets([FromQuery] string? keyword)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Wallet)
                .Where(u => u.RoleId == (int)RoleEnum.EMPLOYER
                         || u.RoleId == (int)RoleEnum.FREELANCER)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u =>
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword));
            }

            var users = await query
                .OrderBy(u => u.Role.Name)
                .ThenBy(u => u.FullName)
                .ToListAsync();

            var result = users.Select(u => new WalletUserDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.Name,
                Balance = u.Wallet?.Balance ?? 0,
                IsActive = u.IsActive
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Nạp tiền cho user
        /// </summary>
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] AdjustBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
                return NotFound("User not found");

            // Tạo ví nếu chưa có
            if (user.Wallet == null)
            {
                user.Wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0
                };
                _context.Wallets.Add(user.Wallet);
            }

            user.Wallet.Balance += dto.Amount;
            user.Wallet.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { balance = user.Wallet.Balance });
        }

        /// <summary>
        /// Rút tiền từ user
        /// </summary>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] AdjustBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
                return NotFound("User not found");

            if (user.Wallet == null || user.Wallet.Balance < dto.Amount)
                return BadRequest("Insufficient balance");

            user.Wallet.Balance -= dto.Amount;
            user.Wallet.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { balance = user.Wallet.Balance });
        }
    }
}
