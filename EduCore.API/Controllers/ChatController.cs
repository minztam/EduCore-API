using EduCore.API.DTOs.Chat;
using EduCore.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduCore.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _repo;

        public ChatController(IChatRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lấy danh sách tất cả phòng chat mà người dùng đang tham gia
        /// </summary>
        [HttpGet("rooms")]
        public async Task<IActionResult> GetMyRooms()
        {
            var userId = GetUserId();
            var result = await _repo.GetUserRoomsAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy chi tiết một phòng chat (Thông tin đối phương hoặc thông tin nhóm)
        /// </summary>
        [HttpGet("rooms/{roomId}")]
        public async Task<IActionResult> GetRoomDetail(int roomId)
        {
            var userId = GetUserId();
            var result = await _repo.GetRoomDetailAsync(roomId, userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tạo hoặc lấy phòng chat 1:1 với một người dùng khác
        /// </summary>
        [HttpPost("private-room/{receiverId}")]
        public async Task<IActionResult> CreatePrivateRoom(Guid receiverId)
        {
            var userId = GetUserId();
            if (userId == receiverId)
                return BadRequest("Bạn không thể tự chat với chính mình");

            var result = await _repo.CreatePrivateRoomAsync(userId, receiverId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gửi tin nhắn mới (Hỗ trợ Text và File ảnh)
        /// </summary>
        [HttpPost("send")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendMessage([FromForm] ChatMessageRequest model)
        {
            var userId = GetUserId();
            var result = await _repo.SendMessageAsync(model, userId);

            // Note: Sau khi tích hợp SignalR, bạn sẽ gọi Hub ở đây để notify realtime

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy lịch sử tin nhắn của một phòng (Có phân trang)
        /// </summary>
        [HttpGet("messages/{roomId}")]
        public async Task<IActionResult> GetMessages(int roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var currentUserId = GetUserId();
            var result = await _repo.GetMessagesAsync(roomId, currentUserId, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Thu hồi tin nhắn
        /// </summary>
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = GetUserId();
            var result = await _repo.DeleteMessageAsync(messageId, userId);
            return StatusCode(result.StatusCode, result);
        }

        // Hàm helper để lấy UserId từ JWT Token
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("id"); // Tùy vào cách bạn đặt tên Claim
            return Guid.Parse(userIdClaim!);
        }
    }
}
