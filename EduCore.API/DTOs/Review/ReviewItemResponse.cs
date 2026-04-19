namespace EduCore.API.DTOs.Review
{
    public class ReviewItemResponse
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
