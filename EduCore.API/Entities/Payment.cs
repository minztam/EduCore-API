using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_Payments")]
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public Guid StudentId { get; set; }
        [ForeignKey("StudentId")]
        public User? Student { get; set; }

        public decimal Amount { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "VNPAY";
        public string Status { get; set; } = "Pending";
        public string? TransactionNo { get; set; }

        public string? BankCode { get; set; }

        public string? ResponseCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }
    }
}
