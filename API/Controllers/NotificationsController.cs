using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : BaseController
    {
        public NotificationsController(AppDbContext context, IMapper mapper, IUserService user) 
            : base(context, mapper, user)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = _user.UserId;
            var list = await _context.Notifications
                .Where(n => n.AccountId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Content,
                    n.IsRead,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _user.UserId;
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.AccountId == userId);

            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _user.UserId;
            var notifications = await _context.Notifications
                .Where(n => n.AccountId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok(true);
        }
    }
}
