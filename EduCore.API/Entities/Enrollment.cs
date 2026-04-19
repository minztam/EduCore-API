using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Enrollments")]
    public class Enrollment
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public User? Student { get; set; }

        [ForeignKey("CourseId")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public double ProgressPercent { get; set; } = 0;
        public int CompletedLessons { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        public bool IsPaid { get; set; } = false;

        public string Status { get; set; } = "Active";
        public DateTime? PaidAt { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public string CompletedLessonIds { get; set; } = "";
    }
}
