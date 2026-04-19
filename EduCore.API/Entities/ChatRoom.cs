using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsGroup { get; set; }
        public Guid? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
