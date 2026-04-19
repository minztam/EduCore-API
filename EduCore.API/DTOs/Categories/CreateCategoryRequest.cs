namespace EduCore.API.DTOs.Categories
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string Type { get; set; } = "Course";

        public Guid? ParentId { get; set; }

        public int SortOrder { get; set; }
    }
}