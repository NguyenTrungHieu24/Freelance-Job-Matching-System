using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    [Authorize]
    [Route("notifications")]
    public class NotificationsController : BaseController
    {
        public NotificationsController(IHttpClientFactory factory) : base(factory)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var data = await GetAsync<List<ClientNotificationDto>>("api/notifications");
                return Json(data ?? new List<ClientNotificationDto>());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var success = await PutAsync<object>($"api/notifications/{id}/read", new { });
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var success = await PutAsync<object>("api/notifications/read-all", new { });
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class ClientNotificationDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
