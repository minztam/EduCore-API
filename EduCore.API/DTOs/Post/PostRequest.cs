namespace EduCore.API.DTOs.Post
{
    public class PostRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public string? Content { get; set; }
        public string? AuthorName { get; set; }
        public Guid? CategoryId { get; set; }
        public bool IsPublished { get; set; }
    }
}
