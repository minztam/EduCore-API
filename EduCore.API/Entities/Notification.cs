using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Notifications")]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? User { get; set; }

        public string? Content { get; set; }
        public string? Type { get; set; }

        public string? RedirectUrl { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
