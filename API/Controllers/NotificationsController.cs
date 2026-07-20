using API.Services.Auth;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : BaseController
    {
        public NotificationsController(AppDbContext context, IMapper mapper, IUserService user)
            : base(context, mapper, user)
        {
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = _user.UserId;
            var notifications = await _context.Notifications
                .Where(n => n.AccountId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = _user.UserId;
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.AccountId == userId);

            if (notification == null)
                return NotFound("Notification not found");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Marked as read successfully" });
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _user.UserId;
            var count = await _context.Notifications
                .CountAsync(n => n.AccountId == userId && !n.IsRead);

            return Ok(new { count });
        }
    }
}
