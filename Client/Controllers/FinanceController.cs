using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Controllers
{
    [Route("finance")]
    [Authorize]
    public class FinanceController : BaseController
    {
        public FinanceController(IHttpClientFactory factory) : base(factory) { }

        [HttpGet("wallets")]
        [Authorize(Roles = "FINANCE_MANAGER")]
        public async Task<IActionResult> Wallets([FromQuery] string? keyword)
        {
            try
            {
                var endpoint = "api/finance/wallets";
                if (!string.IsNullOrWhiteSpace(keyword))
                    endpoint += $"?keyword={Uri.EscapeDataString(keyword)}";

                var wallets = await GetAsync<List<WalletUserDto>>(endpoint);
                ViewBag.Keyword = keyword;
                return View(wallets ?? new List<WalletUserDto>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot load wallets: " + ex.Message;
                return View(new List<WalletUserDto>());
            }
        }

        [HttpPost("deposit")]
        [Authorize(Roles = "FINANCE_MANAGER")]
        public async Task<IActionResult> Deposit(AdjustBalanceDto dto)
        {
            try
            {
                await PostAsync<AdjustBalanceDto, object>("api/finance/deposit", dto);
                TempData["Success"] = $"Deposited {dto.Amount:N0} VNĐ successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Deposit failed: " + ex.Message;
            }
            return RedirectToAction("Wallets");
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = "FINANCE_MANAGER")]
        public async Task<IActionResult> Withdraw(AdjustBalanceDto dto)
        {
            try
            {
                await PostAsync<AdjustBalanceDto, object>("api/finance/withdraw", dto);
                TempData["Success"] = $"Withdrawn {dto.Amount:N0} VNĐ successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Withdraw failed: " + ex.Message;
            }
            return RedirectToAction("Wallets");
        }

        [HttpGet("api-balance")]
        public async Task<IActionResult> ApiBalance()
        {
            try
            {
                var data = await GetAsync<WalletBalanceDto>("api/wallet");
                return Json(data);
            }
            catch
            {
                return Json(new { balance = 0 });
            }
        }
    }
}
