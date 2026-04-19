using EduCore.API.DTOs.Payment;

namespace EduCore.API.Service.Vnpay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        public VnPayService(IConfiguration configuration) => _configuration = configuration;

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var pay = new VnPayLibrary();
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((long)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", _configuration["Vnpay:ReturnUrl"]);
            pay.AddRequestData("vnp_TxnRef", model.OrderId); // Lấy từ DB

            return pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    pay.AddResponseData(key, value);
            }

            var secureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            bool isValid = pay.ValidateSignature(secureHash, _configuration["Vnpay:HashSecret"]);

            return new PaymentResponseModel
            {
                Success = isValid,
                OrderId = pay.GetResponseData("vnp_TxnRef"),
                TransactionId = pay.GetResponseData("vnp_TransactionNo"),
                VnPayResponseCode = pay.GetResponseData("vnp_ResponseCode"),
                BankCode = pay.GetResponseData("vnp_BankCode"),
                PayDate = pay.GetResponseData("vnp_PayDate")
            };
        }
    }
}