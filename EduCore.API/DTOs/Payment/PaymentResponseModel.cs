namespace EduCore.API.DTOs.Payment
{
    public class PaymentResponseModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string VnPayResponseCode { get; set; }
        public string BankCode { get; set; }
        public string PayDate { get; set; }
        public string OrderDescription { get; set; }
        public string PaymentMethod { get; set; }
    }
}
