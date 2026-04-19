using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Chat
{
    public class ChatMessageRequest
    {
        [Required(ErrorMessage = "Mã phòng chat là bắt buộc")]
        public int ChatRoomId { get; set; }

        public string? Content { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
