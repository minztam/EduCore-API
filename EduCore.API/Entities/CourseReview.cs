using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_CourseReview")]
    public class CourseReview
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("CourseId")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public User? Student { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; } = null!;

        public bool IsApproved { get; set; } = false;
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
