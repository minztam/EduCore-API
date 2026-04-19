using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.DTOs.Course
{
    public class UpdateCourseRequest
    {
        public string InstructorId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public IFormFile? PreviewVideo { get; set; }
        public decimal? Price { get; set; }
        public string? Level { get; set; }
        public string? Language { get; set; }

        public Guid? CategoryId { get; set; }
    }
}
