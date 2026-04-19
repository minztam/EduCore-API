using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Lesson
{
    public class CreateLessonRequest
    {
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public IFormFile? VideoUrl { get; set; }
        public IFormFile? DocumentUrl { get; set; }
        public string? Content { get; set; }
        public int OrderIndex { get; set; }
        public Guid ChapterId { get; set; }
    }
}
