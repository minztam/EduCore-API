namespace EduCore.API.DTOs.Chapter
{
    public class CreateChapterRequest
    {
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public Guid CourseId { get; set; }
    }
}
