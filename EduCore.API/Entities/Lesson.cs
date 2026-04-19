using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Lesson")]
    public class Lesson
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string? VideoUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Content { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPublished { get; set; } = false;
        public int Duration { get; set; }
        public bool IsFreePreview { get; set; }

        [ForeignKey("ChapterId")]
        public Guid ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;

    }
}
