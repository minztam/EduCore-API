namespace EduCore.API.DTOs.Post
{
    public class PostQuery
    {
        public string? Keyword { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsPublished { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
