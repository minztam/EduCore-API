namespace EduCore.API.DTOs.Review
{
    public class CreateReviewRequest
    {
        public Guid CourseId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
    }
}
