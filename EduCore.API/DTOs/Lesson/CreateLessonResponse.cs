namespace EduCore.API.DTOs.Lesson
{
    public class CreateLessonResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string? VideoUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public int OrderIndex { get; set; }
        public Guid ChapterId { get; set; }
    }
}
