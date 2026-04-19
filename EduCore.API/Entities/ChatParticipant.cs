using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    public class ChatParticipant
    {
        public int Id { get; set; }
        public int ChatRoomId { get; set; }
        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom? ChatRoom { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")] 
        public virtual User? User { get; set; }
        
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
