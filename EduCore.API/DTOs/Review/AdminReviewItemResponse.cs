namespace EduCore.API.DTOs.Review
{
    public class AdminReviewItemResponse
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
