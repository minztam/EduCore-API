namespace EduCore.API.DTOs.Course
{
    public class CreateCourseRequest
    {
        public string instructorId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;

        public IFormFile? Thumbnail { get; set; }
        public IFormFile? PreviewVideo { get; set; }

        public decimal Price { get; set; }
        public string Level { get; set; } = "Beginner";
        public string Language { get; set; } = "vi";
        public Guid? CategoryId { get; set; }
    }
}
