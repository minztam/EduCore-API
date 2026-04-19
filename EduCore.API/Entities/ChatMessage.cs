using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ChatRoomId { get; set; }
        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom? ChatRoom { get; set; }

        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;
    }
}