namespace EduCore.API.DTOs.Payment
{
    public class PaymentAdminHistory
    {
        public Guid Id { get; set; }
        public string? OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? BankCode { get; set; }
        public string? TransactionNo { get; set; }
        public string? CourseTitle { get; set; }
        public string? StudentName { get; set; }
    }
}
