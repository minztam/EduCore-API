namespace EduCore.API.DTOs.Course
{
    public class UpdateCourseRespone
    {
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
        public int TotalLessons { get; set; }
        public int TotalDuration { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public InstructorRespone? Instructor { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
    }
}
