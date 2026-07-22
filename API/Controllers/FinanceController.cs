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
        /// Danh sach tat ca vi Employer + Freelancer (co search keyword)
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
        /// Nap tien cho user
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

            // Tao vi neu chua co
            if (user.Wallet == null)
            {
                user.Wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0
                };
                _context.Wallets.Add(user.Wallet);
                await _context.SaveChangesAsync();
            }

            user.Wallet.Balance += dto.Amount;
            user.Wallet.UpdatedAt = DateTime.Now;

            _context.Transactions.Add(new Transaction
            {
                WalletId = user.Wallet.Id,
                Type = TransactionType.DEPOSIT,
                Amount = dto.Amount,
                BalanceAfter = user.Wallet.Balance,
                Description = dto.Description ?? "Nap tien tu Finance Manager"
            });

            _context.Notifications.Add(new Notification
            {
                AccountId = user.Id,
                Content = $"Tai khoan cua ban da duoc nap {dto.Amount:N0} VND vao He thong. Ly do: {dto.Description ?? "Khong co mo ta"}. So du hien tai: {user.Wallet.Balance:N0} VND."
            });

            await _context.SaveChangesAsync();

            return Ok(new { balance = user.Wallet.Balance });
        }

        /// <summary>
        /// Rut tien tu user
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

            _context.Transactions.Add(new Transaction
            {
                WalletId = user.Wallet.Id,
                Type = TransactionType.WITHDRAW,
                Amount = -dto.Amount,
                BalanceAfter = user.Wallet.Balance,
                Description = dto.Description ?? "Rut tien tu Finance Manager"
            });

            _context.Notifications.Add(new Notification
            {
                AccountId = user.Id,
                Content = $"Tai khoan cua ban da bi rut {dto.Amount:N0} VND boi He thong. Ly do: {dto.Description ?? "Khong co mo ta"}. So du hien tai: {user.Wallet.Balance:N0} VND."
            });

            await _context.SaveChangesAsync();

            return Ok(new { balance = user.Wallet.Balance });
        }
    }
}
