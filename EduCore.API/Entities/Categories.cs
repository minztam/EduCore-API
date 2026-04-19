using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Categories")]
    public class Categories
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Type { get; set; } = "Course";
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("ParentId")]
        public Guid? ParentId { get; set; }
        public Categories? Parent { get; set; }
        public ICollection<Categories> Children { get; set; } = new List<Categories>();

    }
}
