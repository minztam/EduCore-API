using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repo;
        public NotificationController(INotificationRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("header")]
        public async Task<IActionResult> GetHeaderNotifications()
        {
            var result = await _repo.GetHeaderNotificationsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("page")]
        public async Task<IActionResult> GetNotificationsForPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _repo.GetAllForPageAsync(page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _repo.MarkAsReadAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
