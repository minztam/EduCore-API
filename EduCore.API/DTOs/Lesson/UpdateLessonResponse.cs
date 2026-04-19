namespace EduCore.API.DTOs.Lesson
{
    public class UpdateLessonResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ContentType { get; set; } = null!;

        public string? VideoUrl { get; set; }
        public string? DocumentUrl { get; set; }

        public string? Content { get; set; }
        public int OrderIndex { get; set; }
        public Guid ChapterId { get; set; }
        public bool IsPublished { get; set; }
        public int Duration { get; set; }
    }
}
