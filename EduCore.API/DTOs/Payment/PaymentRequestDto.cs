namespace EduCore.API.DTOs.Payment
{
    public class PaymentRequestDto
    {
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
        public double Amount { get; set; }
    }
}
