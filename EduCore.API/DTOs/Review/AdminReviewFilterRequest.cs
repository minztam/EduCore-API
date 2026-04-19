namespace EduCore.API.DTOs.Review
{
    public class AdminReviewFilterRequest
    {
        public string? Keyword { get; set; }
        public int? Rating { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsHidden { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
