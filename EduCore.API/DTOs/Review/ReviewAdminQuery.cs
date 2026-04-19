namespace EduCore.API.DTOs.Review
{
    public class ReviewAdminQuery
    {
        public Guid? CourseId { get; set; }
        public int? Rating { get; set; }
        public bool? Approved { get; set; }
        public bool? Featured { get; set; }
        public string? Keyword { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
