namespace EduCore.API.DTOs.Lesson
{
    public class UpdateLessonRequest
    {
        public Guid Id { get; set; }

        public string? Title { get; set; } 
        public string? Slug { get; set; }
        public string? ContentType { get; set; }

        public IFormFile? VideoFile { get; set; }
        public IFormFile? DocumentFile { get; set; }

        public string? Content { get; set; }
        public int? OrderIndex { get; set; }
        public bool IsPublished { get; set; }
        public int? Duration { get; set; }
    }
}
