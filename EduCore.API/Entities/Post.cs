using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Post")]
    public class Post
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("CategoryId")]
        public Guid? CategoryId { get; set; }
        public Categories? Category { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Thumbnail { get; set; }
        public string? Content { get; set; }
        public string? AuthorName { get; set; }
        public bool IsPublished { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
