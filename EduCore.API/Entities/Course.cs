using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Course")]
    public class Course
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }
        public string? PreviewVideoUrl { get; set; }

        public decimal Price { get; set; }

        public string Level { get; set; } = "Beginner";
        public string Language { get; set; } = "vi";

        public int TotalLessons { get; set; } = 0;
        public int TotalDuration { get; set; } = 0; 

        [ForeignKey("CreatedBy")]
        public Guid CreatedBy { get; set; }
        public User? Instructor { get; set; }

        [ForeignKey("CategoryId")]
        public Guid? CategoryId { get; set; }
        public Categories? Category { get; set; }

        public bool IsPublished { get; set; } = false;
        public bool IsHost { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
