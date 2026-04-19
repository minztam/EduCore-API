namespace EduCore.API.DTOs.Chapter
{
    public class UpdateChapterRequest
    {
        public string? Title { get; set; }
        public int? Order { get; set; }
        public Guid? CourseId { get; set; }
    }
}
